using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Timers;

namespace Memory
{
    public struct Value
    {
        public string value;
        public string type;
    }

    public class MemoryHandler
    {
        public Process hProc;
        IntPtr pHandle;
        //IntPtr modBase;
        //IntPtr modBase2;
        public Dictionary<string, IntPtr> memoryAddresses = new Dictionary<string, IntPtr>();
        //public Dictionary<string, IntPtr> modules = new Dictionary<string, IntPtr>();
        private ProcessModule mainModule;
        static bool isProcessLoaded { get; set; }
        public bool throwException { get; set; }

        /// <summary>
        /// Loads the client process and initializes the mod base
        /// </summary>
        /// <param name="processName"></param>
        /// <returns>Returns false if it fails successful load the process</returns>
        public bool OpenProcess(int pid)
        {
            if (!IsAdmin())
            {
                Debug.WriteLine("WARNING: You are NOT running this program as admin! Visit https://github.com/erfg12/memory.dll/wiki/Administrative-Privileges");
                NotifyError("WARNING: You are NOT running this program as admin! Visit https://github.com/erfg12/memory.dll/wiki/Administrative-Privileges");
                throw new Exception("WARNING: You are NOT running this program as admin! Visit https://github.com/erfg12/memory.dll/wiki/Administrative-Privileges");
                //MessageBox.Show("WARNING: You are NOT running this program as admin!");
            }

            if (pid <= 0)
            {
                Debug.WriteLine("ERROR: OpenProcess given proc ID 0.");
                return false;
            }


            if (hProc != null && hProc.Id == pid)
                return true;

            try
            {
                hProc = Process.GetProcessById(pid);

                if (hProc != null && !hProc.Responding)
                {
                    Debug.WriteLine("ERROR: OpenProcess: Process is not responding or null.");
                    return false;
                }

                pHandle = MemoryApi.OpenProcess(0x1F0FFF, true, pid);
                Process.EnterDebugMode();

                if (pHandle == IntPtr.Zero)
                {
                    var eCode = Marshal.GetLastWin32Error();
                    Debug.WriteLine("ERROR: OpenProcess has failed opening a handle to the target process (GetLastWin32ErrorCode: " + eCode + ")");
                    Process.LeaveDebugMode();
                    hProc = null;
                    return false;
                }

                isProcessLoaded = true;
                mainModule = hProc.MainModule;
                GetModules();

                Debug.WriteLine("Program is operating at Administrative level. Process #" + hProc + " is open and modules are stored.");

                return true;
            }
            catch
            {
                Debug.WriteLine("ERROR: OpenProcess has crashed. Are you trying to hack a x64 game? https://github.com/erfg12/memory.dll/wiki/64bit-Games");
                return false;
            }
        }

        /// <summary>
        /// Dictionary with our opened process module names with addresses.
        /// </summary>
        public Dictionary<string, IntPtr> modules = new Dictionary<string, IntPtr>();

        /// <summary>
        /// Builds the process modules dictionary (names with addresses).
        /// </summary>
        public void GetModules()
        {
            if (hProc == null)
                return;

            modules.Clear();
            foreach (ProcessModule Module in hProc.Modules)
            {
                if (!string.IsNullOrEmpty(Module.ModuleName) && !modules.ContainsKey(Module.ModuleName))
                    modules.Add(Module.ModuleName, Module.BaseAddress);
            }
        }

        /// <summary>
        /// Open the PC game process with all security and access rights.
        /// </summary>
        /// <param name="proc">Use process name or process ID here.</param>
        /// <returns></returns>
        public bool OpenProcess(string proc)
        {
            return OpenProcess(GetProcIdFromName(proc));
        }

        /// <summary>
        /// Get the process ID number by process name.
        /// </summary>
        /// <param name="name">Example: "eqgame". Use task manager to find the name. Do not include .exe</param>
        /// <returns></returns>
        public int GetProcIdFromName(string name)
        {
            Process[] processlist = Process.GetProcesses();

            if (name.ToLower().Contains(".exe"))
                name = name.Replace(".exe", "");

            foreach (Process theprocess in processlist)
            {
                if (theprocess.ProcessName.Equals(name, StringComparison.CurrentCultureIgnoreCase))
                    return theprocess.Id;
            }

            return 0;
        }

        /// <summary>
        /// Adds a memory address to a Dictionary
        /// </summary>
        /// <param name="id"></param>
        /// <param name="address"></param>
        /// <param name="offsets"></param>
        public void AddAddress(string id, Int32 address, int[] offsets, string module = "main")
        {
            IntPtr? baseAddress;

            if (!isProcessLoaded)
            {
                NotifyError("Process not loaded!");
            }

            var temp = memoryAddresses.FirstOrDefault(x => x.Key == id);

            if (temp.Key != null)
            {
                NotifyError(String.Format("Address with id {0} already exists", id));
                return;
            }

            if (module != "main")
            {
                baseAddress = modules.FirstOrDefault(x => x.Key == module).Value;

                if (baseAddress == null)
                    NotifyError(String.Format("Could not find module with name {0}", baseAddress));
            }
            else
                baseAddress = mainModule.BaseAddress;

            memoryAddresses.Add(id, MemoryApi.FindDMAAddy(pHandle, (IntPtr)(baseAddress + address), offsets));
        }

        /// <summary>
        /// Changes the Address value
        /// </summary>
        /// <param name="id"></param>
        /// <param name="newValue"></param>
        public void WriteValueToAddress(string id, string newValue, string type = "int")
        {
            if (!isProcessLoaded)
            {
                NotifyError("Process not loaded!");
                return;
            } 

            var temp = memoryAddresses.FirstOrDefault(x => x.Key == id);

            if (temp.Key == null)
            {
                NotifyError(String.Format("Could not find an address with id {0}", id));
                return;
            }

            switch(type)
            {
                case "int":
                    MemoryApi.WriteProcessMemory(pHandle, temp.Value, Int32.Parse(newValue), 4, out _);
                    break;

                case "float":
                    MemoryApi.WriteProcessMemory(pHandle, temp.Value, float.Parse(newValue), 4, out _);
                    break;
            }
        }

        /// <summary>
        /// Read a float value from an address.
        /// </summary>
        /// <param name="code">address, module + pointer + offset, module + offset OR label in .ini file.</param>
        /// <param name="file">path and name of ini file. (OPTIONAL)</param>
        /// <param name="round">Round the value to 2 decimal places</param>
        public float? ReadFloatValueFromAddress(string id, bool round = true)
        {
            if (!isProcessLoaded)
            {
                NotifyError("Process not loaded!");
                return null;
            }

            IntPtr address = memoryAddresses.FirstOrDefault(x => x.Key == id).Value;

            if (address == null)
            {
                NotifyError(String.Format("Could not find an address with id {0}", id));
                return null;
            }

            byte[] memory = new byte[4];
            try
            {
                if (MemoryApi.ReadProcessMemory(pHandle, address, memory, 4, out _))
                {
                    float addressValue = BitConverter.ToSingle(memory, 0);
                    float returnValue = (float)addressValue;
                    if (round)
                        returnValue = (float)Math.Round(addressValue, 2);
                    return returnValue;
                }
                else
                    return 0;
            }
            catch
            {
                return 0;
            }
        }


        /// <summary>
        /// Check if program is running with administrative privileges. Read about it here: https://github.com/erfg12/memory.dll/wiki/Administrative-Privileges
        /// </summary>
        /// <returns></returns>
        public bool IsAdmin()
        {
            using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
            {
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }

        /// <summary>
        /// Checks the last error status thrown by the program
        /// </summary>
        /// <returns>ErrorCode</returns>
        public int ErrorStatus()
        {
            return Marshal.GetLastWin32Error();
        }

        /// <summary>
        /// Throws the error in the command line as a message or as an exception
        /// </summary>
        /// <param name="errorMessage"></param>
        public void NotifyError(string errorMessage)
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
