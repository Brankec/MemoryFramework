using System;
using System.Threading;

namespace Memory
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                MemoryFramework memFram = new MemoryFramework();

                //AssaultCube game example
                MemoryHandler.LoadProcess("ac_client");

                MemoryHandler.AddAddress("ammo", 0x0010F418, new int[] { 0x58, 0x1F8, 0x14, 0x0 });

                memFram.AddMemoryAddressToFreeze("ammo", 100);

                memFram.FreezeAddressValues();

                memFram.AddMemoryAddressToFreeze("ammo", 1000);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.InnerException);
            }
        }
    }
}
