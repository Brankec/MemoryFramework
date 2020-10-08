using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace Memory
{
    class MemoryFramework
    {
        private static ConcurrentDictionary<string, Value> memoryAddressIdToFreeze = new ConcurrentDictionary<string, Value>();
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
        /// Causes the given xyz positions to move based on the cameras xy angle position. Requires some sort of key press system to call this method in a loop.
        /// </summary>
        /// <param name="xId"></param>
        /// <param name="zId"></param>
        /// <param name="yId"></param>
        /// <param name="xAngleId"></param>
        /// <param name="yAngleId"></param>
        /// <param name="speed"></param>
        public void DirectionalPositionMovement(string xId, string zId, string yId, string xAngleId, string yAngleId, float speed)
        {
            Vector3 d = new Vector3();

            d.X = (float)memoryHandler.ReadFloatValueFromAddress(xId);
            d.Z = (float)memoryHandler.ReadFloatValueFromAddress(zId);
            d.Y = (float)memoryHandler.ReadFloatValueFromAddress(yId);

            d.X += (float)(Math.Cos(DegreeToRadian((float)(memoryHandler.ReadFloatValueFromAddress(xAngleId)))) * speed);
            d.Y -= (float)(Math.Sin(DegreeToRadian((float)(memoryHandler.ReadFloatValueFromAddress(yAngleId)))) * speed);
            d.Z += (float)(Math.Sin(DegreeToRadian((float)(memoryHandler.ReadFloatValueFromAddress(xAngleId)))) * speed);

            memoryHandler.WriteValueToAddress(xId, d.X.ToString(), "float");
            memoryHandler.WriteValueToAddress(yId, d.Y.ToString(), "float");
            memoryHandler.WriteValueToAddress(zId, d.Z.ToString(), "float");
        }

        /// <summary>
        /// Adds the memory address from MemoryHandler to freeze
        /// </summary>
        /// <param name="id"></param>
        public void AddAddressIdToFreeze(string id, string value = "1337", string type = "int")
        {
            //if (memoryHandler.ErrorStatus() != 0)
            //{
            //    memoryHandler.NotifyError(String.Format("Uh Oh, Something went wrong! Error code: {0}", memoryHandler.ErrorStatus()));
            //    return;
            //}

            var temp = memoryHandler.memoryAddresses.FirstOrDefault(x => x.Key == id);
            if (temp.Key == null)
            {
                memoryHandler.NotifyError(String.Format("Could not find an address with id {0}", id));
                return;
            }

            var temp2 = memoryAddressIdToFreeze.FirstOrDefault(x => x.Key == id);
            if (temp.Key == null)
            {
                memoryAddressIdToFreeze.TryAdd(temp.Key, new Value { value = value, type = type });
            } 
            else if(temp2.Value.value != value)
            {
                memoryAddressIdToFreeze[temp.Key] = new Value { value = value, type = type };
            }
        }

        public void RemoveAddressIdToFreeze(string id)
        {
            //if (memoryHandler.ErrorStatus() != 0)
            //{
            //    memoryHandler.NotifyError(String.Format("Uh Oh, Something went wrong! Error code: {0}", memoryHandler.ErrorStatus()));
            //    return;
            //}

            var temp = memoryHandler.memoryAddresses.FirstOrDefault(x => x.Key == id);
            if (temp.Key == null)
            {
                memoryHandler.NotifyError(String.Format("Could not find an address with id {0}", id));
                return;
            }

            var temp2 = memoryAddressIdToFreeze.FirstOrDefault(x => x.Key == id);
            if (temp2.Key != null)
            {
                memoryAddressIdToFreeze.TryRemove(temp2.Key, out Value v);
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
            //ThreadStart childref = new ThreadStart(FreezeAddressValuesThread);
            //Thread childThread = new Thread(childref);
            //childThread.Start();
            Task t = Task.Factory.StartNew(() => {
                FreezeAddressValuesThread();
            });
        }

        float DegreeToRadian(float degrees)
        {
            return degrees * (3.1415927f / 180);
        }

        public void TriggerBot(string id, float valueToLook, float valueToStop)
        {

        }
    }
}
