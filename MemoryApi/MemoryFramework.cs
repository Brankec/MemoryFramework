using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;

namespace Memory
{
    class MemoryFramework
    {
        private static Dictionary<string, int> memoryAddressIdToFreeze = new Dictionary<string, int>();

        /// <summary>
        /// Adds the memory address from MemoryHandler to later be frozen by a while loop
        /// </summary>
        /// <param name="id"></param>
        public void AddMemoryAddressToFreeze(string id, int value = 1337)
        {
            if (MemoryHandler.ErrorStatus() != 0)
            {
                MemoryHandler.NotifyError(String.Format("Uh Oh, Somethign went wrong! Error code: {0}", MemoryHandler.ErrorStatus()));
                return;
            }

            var temp = MemoryHandler.memoryAddresses.FirstOrDefault(x => x.Key == id);
            if ((temp.Key == null) || (temp.Key == ""))
            {
                MemoryHandler.NotifyError(String.Format("Could not find an address with id {0}", id));
                return;
            }

            var temp2 = memoryAddressIdToFreeze.FirstOrDefault(x => x.Key == id);
            if ((temp.Key == null) || (temp.Key == ""))
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
                Thread.Sleep(100);

                foreach (var memoryId in memoryAddressIdToFreeze)
                    MemoryHandler.ChangeValueToAddress(memoryId.Key, memoryId.Value);
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
