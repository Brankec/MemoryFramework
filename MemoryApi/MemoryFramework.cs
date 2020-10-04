using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Memory
{
    class MemoryFramework
    {
        private static Dictionary<string, int> memoryAddressIdToFreeze = new Dictionary<string, int>();
        private static MemoryHandler memoryHandler = new MemoryHandler();

        public void AddAddress(string id, Int32 address, int[] offsets)
        {
            memoryHandler.AddAddress(id, address, offsets);
        }

        public bool OpenProcess(int pid)
        {
            return memoryHandler.OpenProcess(pid);
        }

        public bool OpenProcess(string pid)
        {
            return memoryHandler.OpenProcess(pid);
        }

        /// <summary>
        /// Adds the memory address from MemoryHandler to freeze
        /// </summary>
        /// <param name="id"></param>
        public void AddAddressIdFreeze(string id, int value = 1337)
        {
            if (memoryHandler.ErrorStatus() != 0)
            {
                memoryHandler.NotifyError(String.Format("Uh Oh, Something went wrong! Error code: {0}", memoryHandler.ErrorStatus()));
                return;
            }

            var temp = memoryHandler.memoryAddresses.FirstOrDefault(x => x.Key == id);
            if (temp.Key == null)
            {
                memoryHandler.NotifyError(String.Format("Could not find an address with id {0}", id));
                return;
            }

            var temp2 = memoryAddressIdToFreeze.FirstOrDefault(x => x.Key == id);
            if (temp.Key == null)
            {
                memoryAddressIdToFreeze.Add(temp.Key, value);
            } 
            else if(temp2.Value != value)
            {
                memoryAddressIdToFreeze[temp.Key] = value;
            }
        }

        private static void FreezeAddressValuesThread()
        {
            while (true)
            {
                foreach (var memoryId in memoryAddressIdToFreeze)
                    memoryHandler.ChangeValueToAddress(memoryId.Key, memoryId.Value);

                Thread.Sleep(100);
            }
        }

        public void FreezeAddressValues()
        {
            ThreadStart childref = new ThreadStart(FreezeAddressValuesThread);
            Thread childThread = new Thread(childref);
            childThread.Start();
        }
    }
}
