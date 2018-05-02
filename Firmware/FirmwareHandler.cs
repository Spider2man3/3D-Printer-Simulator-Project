using Hardware;
using PrinterSimulator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Firmware
{
    public class FirmwareHandler
    {
        PrinterControl printer;
        public bool fDone = false;
        string firmwareVersion;
        private ZRail zRail;
        public FirmwareHandler(PrinterControl printer, string firmwareVersion)
        {
            this.firmwareVersion = firmwareVersion;
            this.printer = printer;
            this.zRail = new ZRail(printer);
        }
        public bool read(byte[] buffer, int expectedBytes)
        {
            if (expectedBytes == 0)
            {
                return true;
            }
            int readResult = 0;
            while (readResult == 0)
            {
                readResult = printer.ReadSerialFromHost(buffer, expectedBytes);
            }
            return true;
        }
        private String printMessage(byte[] message)
        {
            string result = "";
            foreach (byte b in message)
            {
                result += b.ToString() + ", ";
            }
            return result;
        }

        public void Process()
        {
            while (!fDone)
            {
                var header = new byte[4];
                if (printer.ReadSerialFromHost(header, 4) == 4)
                {
                    //Console.WriteLine("Firmware: Received command bytes of {0}", printMessage(header));
                    //Console.WriteLine("Firmware: Receieved command from host");
                    //Console.WriteLine("Firmware: Sending header back to host");
                    //Console.WriteLine("Firmware: Writing bytes {0}, back to host", printMessage(header));
                    printer.WriteSerialToHost(header, 4);
                    var ackOrNak = new byte[1];
                    if (!read(ackOrNak, 1))
                    {
                        //Console.WriteLine("Firmware: ackornack bytes were {0}", printMessage(ackOrNak));
                        continue;
                    }
                    //Console.WriteLine("Firmware: ackornack bytes were {0}", printMessage(ackOrNak));
                    if (ackOrNak.SequenceEqual(new byte[1] { 0xA5 }))
                    {
                        //Console.WriteLine("Firmware: Received ack");
                        bool checksum = true;
                        var paramSize = int.Parse(header[1].ToString());
                        var paramBytes = new byte[paramSize];
                        //Console.WriteLine("Firmware: Reading parameter data");
                        if (read(paramBytes, paramSize))
                        {
                            //Console.WriteLine("Firmware: Received param bytes of {0}", printMessage(paramBytes));
                            //Console.WriteLine("Firmware: Validating checksum");
                            checksum = HelperFunctions.validateChecksum(header, paramBytes);
                            if (checksum == true)
                            {
                                //Console.WriteLine("Firmware: Checksum was good, sending version confirmation");
                                // Run command header[0]
                                executeCommand(header[0], paramBytes);
                                var message = Encoding.UTF8.GetBytes("VERSION:" + firmwareVersion + char.MinValue);
                                //Console.WriteLine("Firmware: Writing confirmation of bytes {0}", printMessage(message));
                                printer.WriteSerialToHost(message, message.Length);
                            }
                            else
                            {
                                //Console.WriteLine("Firmware: Checksum was bad, sending checksum error");
                                var message = Encoding.UTF8.GetBytes("CHECKSUM" + char.MinValue);
                                //Console.WriteLine("Firmware: Writing bad cheksum of bytes {0}", printMessage(message));
                                printer.WriteSerialToHost(message, message.Length);
                            }
                        }
                    }
                    else
                    {
                        //Console.WriteLine("Firmware: Received nak");
                        continue;
                    }
                }
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
                    zRail.resetStepperToBuildPlate();
                    break;
                case 0x02:
                    float direction = BitConverter.ToSingle(param, 0);
                    if (direction == 1)
                    {
                        //Console.WriteLine("Step up");
                        bool result = printer.StepStepper(PrinterControl.StepperDir.STEP_UP);
                    }
                    else if (direction == 0)
                    {
                        //Console.WriteLine("Step down");
                        bool result = printer.StepStepper(PrinterControl.StepperDir.STEP_DOWN);
                    }
                    break;
                case 0x03:
                    float value = BitConverter.ToSingle(param, 0);
                    if (value == 1)
                    {
                        printer.SetLaser(true);
                       // Console.WriteLine("Turned on laser");
                    }
                    else if (value == 0)
                    {
                        printer.SetLaser(false);
                       // Console.WriteLine("Turned laser off");
                    }
                    else
                    {
                        Console.WriteLine("Invalid laser value for on/off");
                    }
                    break;
                case 0x04:
                    float x = BitConverter.ToSingle(param, 0);
                    float y = BitConverter.ToSingle(param, 4);
                    printer.MoveGalvos(x, y);
                    //Console.WriteLine("Moved galvo to ({0}, {1})", x, y);
                    break;
                case 0x05:
                    float distance = BitConverter.ToSingle(param, 0);
                    zRail.moveStepper(distance);
                    break;
                default:
                    Console.WriteLine("Bad command");
                    break;
            }
        }
    }
}
