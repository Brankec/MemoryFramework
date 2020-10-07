using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Memory
{
    class MemoryFramework
    {
        private static Dictionary<string, Value> memoryAddressIdToFreeze = new Dictionary<string, Value>();
        public static MemoryHandler memoryHandler = new MemoryHandler();

        public void AddAddress(string id, Int32 address, int[] offsets, string module = "main")
        {
            memoryHandler.AddAddress(id, address, offsets, module);
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
        public void AddAddressIdToFreeze(string id, string value = "1337", string type = "int")
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
                memoryAddressIdToFreeze.Add(temp.Key, new Value { value = value, type = type });
            } 
            else if(temp2.Value.value != value)
            {
                memoryAddressIdToFreeze[temp.Key] = new Value { value = value, type = type };
            }
        }

        private static void FreezeAddressValuesThread()
        {
            while (true)
            {
                Thread.Sleep(10);

                foreach (var memoryId in memoryAddressIdToFreeze)
                    memoryHandler.WriteValueToAddress(memoryId.Key, memoryId.Value.value, memoryId.Value.type);
            }
        }

        public void FreezeAddressValues()
        {
            ThreadStart childref = new ThreadStart(FreezeAddressValuesThread);
            Thread childThread = new Thread(childref);
            childThread.Start();
        }

        public void TriggerBot(string id, float valueToLook, float valueToStop)
        {

        }
    }
}
