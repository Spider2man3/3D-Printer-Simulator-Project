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
        Dictionary<String, int> commandsExecuted = new Dictionary<string, int>();
        public FirmwareHandler(PrinterControl printer, string firmwareVersion)
        {
            this.firmwareVersion = firmwareVersion;
            this.printer = printer;
            commandsExecuted.Add("ResetStepper", 0);
            commandsExecuted.Add("StepStepperUp", 0);
            commandsExecuted.Add("StepStepperDown", 0);
            commandsExecuted.Add("MoveGalvonometer", 0);
        }
        public int getExecutions(String command)
        {
            return commandsExecuted[command];
        }
        public bool read(byte[] buffer, int expectedBytes)
        {
            if (expectedBytes == 0)
            {
                return true;
            }
            int timeout = 100;
            int readResult = 0;
            while (timeout > 0 && readResult == 0)
            {
                readResult = printer.ReadSerialFromHost(buffer, expectedBytes);
                if (readResult != expectedBytes)
                {
                    timeout--;
                    Thread.Sleep(20);
                }
            }
            if (timeout == 0)
            {
                Console.WriteLine("Firmware: Read timeout");
                var message = Encoding.UTF8.GetBytes("TIMEOUT" + char.MinValue);
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
                if (printer.ReadSerialFromHost(header, 4) != 4)
                {
                    Thread.Sleep(5);
                    continue; // skip rest of loop 
                }
                Console.WriteLine("Firmware: Receieved command from host");
                Console.WriteLine("Firmware: Sending header back to host");
                printer.WriteSerialToHost(header, 4);
                var ackOrNak = new byte[1];
                if (!read(ackOrNak, 1))
                {
                    continue;
                }
                if (ackOrNak.SequenceEqual(new byte[1] { 0xA5 }))
                {
                    Console.WriteLine("Firmware: Received ack");
                    bool checksum = true;
                    var paramSize = int.Parse(header[1].ToString());
                    var paramBytes = new byte[paramSize];
                    Console.WriteLine("Firmware: Reading parameter data");
                    if (read(paramBytes, paramSize))
                    {
                        Console.WriteLine("Firmware: Validating checksum");
                        checksum = HelperFunctions.validateChecksum(header, paramBytes);
                        if (checksum == true)
                        {
                            Console.WriteLine("Firmware: Checksum was good, sending version confirmation");
                            // Run command header[0]
                            executeCommand(header[0], paramBytes);
                            var message = Encoding.UTF8.GetBytes("VERSION:" + firmwareVersion + char.MinValue);
                            printer.WriteSerialToHost(message, message.Length);
                        }
                        else
                        {
                            Console.WriteLine("Firmware: Checksum was bad, sending checksum error");
                            var message = Encoding.UTF8.GetBytes("CHECKSUM" + char.MinValue);
                            printer.WriteSerialToHost(message, message.Length);
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Firmware: Received nak");
                    continue;
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
                    commandsExecuted["ResetStepper"] += 1;
                    printer.ResetStepper();
                    moveStepperToTop();
                    moveStepperFromTopToBuildPlate();
                    break;
                case 0x02:
                    float direction = BitConverter.ToSingle(param, 0);
                    if (direction == 1)
                    {
                        Console.WriteLine("Step up");
                        commandsExecuted["StepStepperUp"] += 1;
                        bool result = printer.StepStepper(PrinterControl.StepperDir.STEP_UP);
                    }
                    else if (direction == 0)
                    {
                        Console.WriteLine("Step down");
                        commandsExecuted["StepStepperDown"] += 1;
                        bool result = printer.StepStepper(PrinterControl.StepperDir.STEP_DOWN);
                    }
                    break;
                case 0x03:
                    float value = BitConverter.ToSingle(param, 0);
                    if (value == 1)
                    {
                        printer.SetLaser(true);
                        Console.WriteLine("Turned on laser");
                    }
                    else if (value == 0)
                    {
                        printer.SetLaser(false);
                        Console.WriteLine("Turned laser off");
                    }
                    else
                    {
                        Console.WriteLine("Invalid laser value for on/off");
                    }
                    break;
                case 0x04:
                    commandsExecuted["MoveGalvonometer"] += 1;
                    float x = BitConverter.ToSingle(param, 0);
                    float y = BitConverter.ToSingle(param, 4);
                    printer.MoveGalvos(x, y);
                    Console.WriteLine("Moved galvo");
                    break;
                default:
                    Console.WriteLine("Bad command");
                    break;
            }
        }

        public void moveStepperToTop()
        {
            double currentMicrosecondWait = 630;
            double currentWait = 0;
            while (printer.LimitSwitchPressed() == false)
            {
                bool printerStepped = printer.StepStepper(PrinterControl.StepperDir.STEP_UP);
                if (!printerStepped && !printer.LimitSwitchPressed())
                {
                    throw new Exception("printer step failed, speed is wrong?");
                }
                printer.WaitMicroseconds((long)currentMicrosecondWait);
                currentWait += currentMicrosecondWait;
                if (currentWait > 1000 && currentMicrosecondWait > 69)
                {
                    currentMicrosecondWait -= .06;
                    currentWait = 0;
                }
               
            }
            Console.WriteLine("Limit switch pressed?");
        }

        public void moveStepperFromTopToBuildPlate()
        {
            double currentMicrosecondWait = 630;
            double currentWait = 0;
            int height = 40000;
            while (height > 0)
            {
                bool printerStepped = printer.StepStepper(PrinterControl.StepperDir.STEP_DOWN);
                height -= 1;
                if (!printerStepped && height > 0)
                {
                    throw new Exception("printer step failed, speed is wrong?");
                }
                printer.WaitMicroseconds((long)currentMicrosecondWait);
                currentWait += currentMicrosecondWait;
                if (currentWait > 1000 && currentMicrosecondWait > 69)
                {
                    currentMicrosecondWait -= .055;
                    currentWait = 0;
                }
            }
            Console.WriteLine("Limit switch pressed?");
        }
    }
}
