using Memory;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HalfLife1Trainer
{
    public partial class Form1 : Form
    {
        private static MemoryFramework memoryFramework = new MemoryFramework();
        private GlobalKeyboardHook _globalKeyboardHook;
        private bool FlyMode = false;

        public Form1()
        {
            InitializeComponent();
            Init();

            Visible = false;
            ShowInTaskbar = false;

            Load += Form1_Load;
            FormClosing += Form1_FormClosing;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            SetupKeyboardHooks();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            _globalKeyboardHook?.Dispose();
        }

        public void Init()
        {
            try
            {
                //Half Life 1 game example

                var processFound = memoryFramework.OpenProcess("hl");
                while (!processFound)
                {
                    Thread.Sleep(1000);
                    processFound = memoryFramework.OpenProcess("hl");
                }


                memoryFramework.AddAddress("health", 0x00843D60, new int[] { 0x7C, 0x4, 0x2AC, 0x4, 0x74, 0x4, 0x160 }, "hw.dll");
                memoryFramework.AddAddress("armor", 0x00843D60, new int[] { 0x7C, 0x4, 0x2AC, 0x4, 0x74, 0x4, 0x1BC }, "hw.dll");
                memoryFramework.AddAddress("xPosition", 0x00843D60, new int[] { 0x7C, 0x4, 0x2AC, 0x4, 0x74, 0x4, 0x8 }, "hw.dll");
                memoryFramework.AddAddress("zPosition", 0x00843D60, new int[] { 0x7C, 0x4, 0x2AC, 0x4, 0x74, 0x4, 0xC }, "hw.dll");
                memoryFramework.AddAddress("yPosition", 0x00843D60, new int[] { 0x7C, 0x4, 0x2AC, 0x4, 0x74, 0x4, 0x10 }, "hw.dll");
                memoryFramework.AddAddress("verticalAxis", 0x00843D60, new int[] { 0x7C, 0x4, 0x2AC, 0x4, 0x74, 0x4, 0x74 }, "hw.dll"); //read only
                memoryFramework.AddAddress("horizontalAxis", 0x00843D60, new int[] { 0x7C, 0x4, 0x2AC, 0x4, 0x74, 0x4, 0x78 }, "hw.dll"); //read only
                memoryFramework.AddAddress("xVelocity", 0x00807694, new int[] { 0x7C, 0x4, 0x2AC, 0x4, 0x280, 0xA0 }, "hw.dll");
                memoryFramework.AddAddress("zVelocity", 0x00807694, new int[] { 0x7C, 0x4, 0x2AC, 0x4, 0x280, 0xA4 }, "hw.dll");
                memoryFramework.AddAddress("yVelocity", 0x00807694, new int[] { 0x7C, 0x4, 0x2AC, 0x4, 0x280, 0xA8 }, "hw.dll");

                memoryFramework.AddAddressIdToFreeze("health", "1000", "float");
                memoryFramework.AddAddressIdToFreeze("armor", "1000", "float");

                memoryFramework.FreezeAddressValues();
            }
            catch (Exception e)
            {
                Console.WriteLine(String.Format("Exception: {0}", e.Message));
                Console.WriteLine(String.Format("Inner Exception: {0}", e.InnerException));
            }
        }

        public void SetupKeyboardHooks()
        {
            _globalKeyboardHook = new GlobalKeyboardHook();
            _globalKeyboardHook.KeyboardPressed += OnKeyPressed;
        }

        private void OnKeyPressed(object sender, GlobalKeyboardHookEventArgs e)
        {
            //Debug.WriteLine(e.KeyboardData.VirtualCode);

            if (e.KeyboardData.VirtualCode != 70)
                return;

            if (e.KeyboardState == GlobalKeyboardHook.KeyboardState.KeyDown)
            {
                if (!FlyMode)
                {
                    FlyMode = true;
                    memoryFramework.AddAddressIdToFreeze("xVelocity", "0", "float");
                    memoryFramework.AddAddressIdToFreeze("zVelocity", "0", "float");
                    memoryFramework.AddAddressIdToFreeze("yVelocity", "5", "float");

                    Task t = Task.Factory.StartNew(() =>
                    {
                        while (FlyMode)
                        {
                            Task.Delay(1);
                            memoryFramework.DirectionalPositionMovement("xPosition", "zPosition", "yPosition", "horizontalAxis", "verticalAxis", 0.2f);
                        }
                    });
                }
                e.Handled = true;
            }
            if (e.KeyboardState == GlobalKeyboardHook.KeyboardState.KeyUp)
            {
                FlyMode = false;
                memoryFramework.RemoveAddressIdToFreeze("xVelocity");
                memoryFramework.RemoveAddressIdToFreeze("zVelocity");
                memoryFramework.RemoveAddressIdToFreeze("yVelocity");
                e.Handled = true;
            }
        }
    }
}
