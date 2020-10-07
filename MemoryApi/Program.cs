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

                var processFound = memFram.OpenProcess("hl");
                while (!processFound)
                {
                    Thread.Sleep(1000);
                    processFound = memFram.OpenProcess("hl");
                }


                memFram.AddAddress("health", 0x00843D60, new int[] { 0x7C, 0x4, 0x2AC, 0x4, 0x74, 0x4, 0x160 }, "hw.dll");
                memFram.AddAddress("armor", 0x00843D60, new int[] { 0x7C, 0x4, 0x2AC, 0x4, 0x74, 0x4, 0x1BC }, "hw.dll");
                memFram.AddAddress("xPosition", 0x00843D60, new int[] { 0x7C, 0x4, 0x2AC, 0x4, 0x74, 0x4, 0x4 }, "hw.dll");
                memFram.AddAddress("zPosition", 0x00843D60, new int[] { 0x7C, 0x4, 0x2AC, 0x4, 0x74, 0x4, 0xC }, "hw.dll");
                memFram.AddAddress("yPosition", 0x00843D60, new int[] { 0x7C, 0x4, 0x2AC, 0x4, 0x74, 0x4, 0x10 }, "hw.dll");
                memFram.AddAddress("verticalAxis", 0x00843D60, new int[] { 0x7C, 0x4, 0x2AC, 0x4, 0x74, 0x4, 0x74 }, "hw.dll"); //read only
                memFram.AddAddress("horizontalAxis", 0x00843D60, new int[] { 0x7C, 0x4, 0x2AC, 0x4, 0x74, 0x4, 0x78 }, "hw.dll"); //read only
                memFram.AddAddress("velocity", 0x00807694, new int[] { 0x7C, 0x4, 0x2AC, 0x4, 0x280, 0xA8}, "hw.dll");

                memFram.AddAddressIdToFreeze("health", "1000", "float");
                memFram.AddAddressIdToFreeze("armor", "1000", "float");
                memFram.AddAddressIdToFreeze("velocity", "5", "float");

                memFram.FreezeAddressValues();
            }
            catch (Exception e)
            {
                Console.WriteLine(String.Format("Exception: {0}", e.Message));
                Console.WriteLine(String.Format("Inner Exception: {0}", e.InnerException));
            }
        }
    }
}
