using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Timers;

namespace Memory
{
    public class MemoryHandler
    {
        static IntPtr hProc;
        static IntPtr modBase;
        static IntPtr modBase2;
        public static Dictionary<string, IntPtr> memoryAddresses = new Dictionary<string, IntPtr>();
        static bool isProcessLoaded { get; set; }
        public static bool throwException { get; set; }

        /// <summary>
        /// Loads the client process and initializes the mod base
        /// </summary>
        /// <param name="processName"></param>
        /// <returns>Returns false if it fails successful load the process</returns>
        public static bool LoadProcess(string processName)
        {
            Process proc = Process.GetProcessesByName(processName)[0];

            hProc = Memory.MemoryApi.OpenProcess(Memory.MemoryApi.ProcessAccessFlags.All, false, proc.Id);
            modBase = Memory.MemoryApi.GetModuleBaseAddress(proc, String.Format("{0}.exe", processName));
            modBase2 = Memory.MemoryApi.GetModuleBaseAddress(proc.Id, String.Format("{0}.exe", processName));

            if ((hProc == IntPtr.Zero) || (modBase == IntPtr.Zero) || (modBase2 == IntPtr.Zero) || (ErrorStatus() != 0))
            {
                isProcessLoaded = false;
                return false;
            }
            else
            {
                isProcessLoaded = true;
                return true;
            }
        }

        /// <summary>
        /// Adds a memory address to a Dictionary
        /// </summary>
        /// <param name="id"></param>
        /// <param name="address"></param>
        /// <param name="offsets"></param>
        public static void AddAddress(string id, Int32 address, int[] offsets)
        {
            if (!isProcessLoaded)
            {
                NotifyError("Process not loaded!");
            }

            memoryAddresses.Add(id, Memory.MemoryApi.FindDMAAddy(hProc, (IntPtr)(modBase2 + address), offsets));
        }

        /// <summary>
        /// Changes the Address value
        /// </summary>
        /// <param name="id"></param>
        /// <param name="newValue"></param>
        public static void ChangeValueToAddress(string id, int newValue)
        {
            if (!isProcessLoaded)
            {
                NotifyError("Process not loaded!");
            }

            var temp = memoryAddresses.FirstOrDefault(x => x.Key == id);

            if ((temp.Key == null) || (temp.Key == ""))
                NotifyError(String.Format("Could not find an address with id {0}", id));

            Memory.MemoryApi.WriteProcessMemory(hProc, temp.Value, newValue, 4, out _);
        }

        /// <summary>
        /// Checks the last error status thrown by the program
        /// </summary>
        /// <returns>ErrorCode</returns>
        public static int ErrorStatus()
        {
            return Marshal.GetLastWin32Error();
        }

        /// <summary>
        /// Throws the error in the command line as a message or as an exception
        /// </summary>
        /// <param name="errorMessage"></param>
        public static void NotifyError(string errorMessage)
        {
            if (throwException)
            {
                throw new Exception(errorMessage);
            }
            else
            {
                Console.WriteLine(errorMessage);
            }
        }
    }
}
