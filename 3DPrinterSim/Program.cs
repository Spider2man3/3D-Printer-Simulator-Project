﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Threading;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Hardware;
using Firmware;
using Host;
using System.Windows;

namespace PrinterSimulator
{
    class PrintSim
    {
        static void PrintFile(PrinterControl simCtl)
        {
            try
            {
                System.IO.StreamReader file = new System.IO.StreamReader("..\\..\\..\\SampleSTLs\\F-35_Corrected.gcode");

                Stopwatch swTimer = new Stopwatch();
                swTimer.Start();

                // Todo - Read GCODE file and send data to firmware for printing

                swTimer.Stop();
                long elapsedMS = swTimer.ElapsedMilliseconds;

                Console.WriteLine("Total Print Time: {0}", elapsedMS / 1000.0);
                Console.WriteLine("Press any key to continue");
                Console.ReadKey();
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e.StackTrace);
            }
        }

        //[STAThread]
        //
        //[DllImport("kernel32.dll", SetLastError = true)]
        //static extern IntPtr GetConsoleWindow();
        //
        //[DllImport("user32.dll", SetLastError = true)]
        //internal static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);
        //
        //[DllImport("user32.dll", SetLastError = true)]
        //static extern bool SetForegroundWindow(IntPtr hWnd);
        //[DllImport("user32.dll", SetLastError = true)]
        //static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [STAThread]
        static void Main()
        {
            try
            {
                //IntPtr ptr = GetConsoleWindow();
                //MoveWindow(ptr, 0, 0, 1000, 400, true);

                // Start the printer - DO NOT CHANGE THESE LINES
                PrinterThread printer = new PrinterThread();
                Thread oThread = new Thread(new ThreadStart(printer.Run));
                oThread.Start();
                printer.WaitForInit();

                // Start the firmware thread - DO NOT CHANGE THESE LINES
                FirmwareController firmware = new FirmwareController(printer.GetPrinterSim());
                oThread = new Thread(new ThreadStart(firmware.Start));
                oThread.Start();
                firmware.WaitForInit();
                HostHandler handler = new HostHandler(printer);

                //SetForegroundWindow(ptr);
                // Hide Console
                //ShowWindow(ptr, 0);
                Application app = new Application();
                app.Run(new MainWindow(handler));

                printer.Stop();
                firmware.Stop();
                //bool fDone = false;
                //while (!fDone)
                //{
                //    
                //    Console.Clear();
                //    Console.WriteLine("3D Printer Simulation - Control Menu\n");
                //    Console.WriteLine("P - Print");
                //    Console.WriteLine("T - Test");
                //    Console.WriteLine("Q - Quit");
                //
                //    char ch = Char.ToUpper(Console.ReadKey().KeyChar);
                //    switch (ch)
                //    {
                //        case 'P': // Print
                //            PrintFile(printer.GetPrinterSim());
                //            break;
                //
                //        case 'T': // Test menu
                //            break;
                //
                //        case 'Q' :  // Quite
                //            printer.Stop();
                //            firmware.Stop();
                //            fDone = true;
                //            break;
                //    }
                //
                //}
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e.StackTrace);
            }

        }
    }
}