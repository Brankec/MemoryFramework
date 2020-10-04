﻿using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace MemoryApi
{
    class Program
    {
        static void Main(string[] args)
        {
            MemoryHandler memHan = new MemoryHandler();

            try
            {
                //AssaultCube game example
                memHan.LoadProcess("ac_client");

                memHan.AddAddress("ammo", 0x0010F418, new int[] { 0x58, 0x1F8, 0x14, 0x0 });

                while (true)
                {
                    Thread.Sleep(100);
                    memHan.ChangeValueToAddress("ammo", 1337);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.InnerException);
            }
        }
    }
}
