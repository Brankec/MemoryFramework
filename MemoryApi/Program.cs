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
                //AssaultCube game example
                MemoryFramework memFram = new MemoryFramework();

                var processFound = memFram.OpenProcess("ac_client");
                while (!processFound)
                {
                    Thread.Sleep(1000);
                    processFound = memFram.OpenProcess("ac_client");
                }

                memFram.FreezeAddressValues();

                memFram.AddAddress("ammo", 0x0010F418, new int[] { 0x58, 0x1F8, 0x14, 0x0 });
                memFram.AddAddress("health", 0x00110C64, new int[] { 0x930, 0x14, 0x0, 0x3DC });
                memFram.AddAddress("health-singleplayer", 0x0010F418, new int[] { 0x58, 0x1E0, 0x78, 0xF8 });
                memFram.AddAddress("Y Coordinate", 0x10F4F4, new int[] { 0x3C });

                //MemoryFramework.memoryHandler.WriteValueToAddress("Y Coordinate", "10", "float");

                memFram.AddAddressIdFreeze("ammo", "100");
                memFram.AddAddressIdFreeze("health", "1000");
                memFram.AddAddressIdFreeze("health-singleplayer", "1000");
                memFram.AddAddressIdFreeze("Y Coordinate", "10", "float");
            }
            catch (Exception e)
            {
                Console.WriteLine(String.Format("Exception: {0}", e.Message));
                Console.WriteLine(String.Format("Inner Exception: {0}", e.InnerException));
            }
        }
    }
}
