using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Hardware;

namespace Firmware
{

    public class FirmwareController
    {
        String firmwareVersion = "1.0";
        PrinterControl printer;
        bool fDone = false;
        bool fInitialized = false;

        public FirmwareController(PrinterControl printer)
        {
            this.printer = printer;
        }
        public bool read(byte[] buffer, int expectedBytes)
        {
            if (expectedBytes == 0)
            {
                return true;
            }
            int timeout = 10;
            int readResult = 0;
            while (timeout > 0 && readResult == 0)
            {
                readResult = printer.ReadSerialFromHost(buffer, expectedBytes);
                if (readResult == 0)
                {
                    timeout--;
                    Thread.Sleep(10);
                }
            }
            if (timeout == 0)
            {
                var message = Encoding.UTF8.GetBytes("TIMEOUT");
                printer.WriteSerialToHost(message, message.Length);
                return false;
            }
            return true;
        }

        public void Process()
        {
            while (!fDone)
            {
                var header = new byte[4];
                if (printer.ReadSerialFromHost(header, 4) == 0)
                {
                    continue; // skip rest of loop 
                }
                printer.WriteSerialToHost(header, 4);
                var ackOrNak = new byte[1];
                if (!read(ackOrNak, 1))
                {
                    continue; // skip rest of loop
                }
                if (ackOrNak.SequenceEqual(new byte[1] { 0xA5 }))
                {
                    bool checksum = true;
                    var paramSize = int.Parse(header[1].ToString());
                    var paramBytes = new byte[paramSize];
                    if (read(paramBytes, paramSize))
                    {
                        //checksum = validateChecksum(header, paramBytes);
                        if (checksum == true)
                        {
                            // Run command header[0]
                            executeCommand(header[0], paramBytes);
                            var message = Encoding.UTF8.GetBytes("VERSION:" + firmwareVersion + Char.MinValue);
                            printer.WriteSerialToHost(message, message.Length);
                        }
                        else
                        {
                            var message = Encoding.UTF8.GetBytes("CHECKSUM");
                            printer.WriteSerialToHost(message, message.Length);
                        }
                    }
                }
                else
                    continue;
            }
        }

        public void executeCommand(byte command, byte[] param)
        {
            switch (command)
            {
                case 0x00:
                    return;
                case 0x01:
                    printer.ResetStepper();
                    break;
                case 0x02:
                    float direction = BitConverter.ToSingle(param, 0);
                    if (direction == 1)
                    {
                        bool result = printer.StepStepper(PrinterControl.StepperDir.STEP_UP);
                    }
                    else if (direction == 0)
                    {
                        bool result = printer.StepStepper(PrinterControl.StepperDir.STEP_DOWN);
                    }
                    break;
            }
        }

        public void Start()
        {
            fInitialized = true;

            Process(); // this is a blocking call
        }

        public void Stop()
        {
            fDone = true;
        }

        public void WaitForInit()
        {
            while (!fInitialized)
                Thread.Sleep(100);
        }
    }
}
