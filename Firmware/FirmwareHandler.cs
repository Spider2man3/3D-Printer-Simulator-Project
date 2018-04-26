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
            try
            {
                if (expectedBytes == 0)
                {
                    return true;
                }
                int timeout = 10000;
                int readResult = 0;
                while (timeout > 0 && readResult == 0)
                {
                    readResult = printer.ReadSerialFromHost(buffer, expectedBytes);
                    if (readResult != expectedBytes)
                    {
                        timeout--;
                        Thread.Sleep(10);
                    }
                }
                if (timeout == 0)
                {
                    var message = Encoding.UTF8.GetBytes("TIMEOUT" + char.MinValue);
                    printer.WriteSerialToHost(message, message.Length);
                    return false;
                }
                return true;
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e.StackTrace);
                return false;
            }
        }

        public void Process()
        {
            try
            {
                while (!fDone)
                {
                    var header = new byte[4];
                    if (printer.ReadSerialFromHost(header, 4) == 0)
                    {
                        Thread.Sleep(5);
                        continue; // skip rest of loop 
                    }
                    printer.WriteSerialToHost(header, 4);
                    var ackOrNak = new byte[1];
                    if (!read(ackOrNak, 1))
                    {
                        continue;
                    }
                    if (ackOrNak.SequenceEqual(new byte[1] { 0xA5 }))
                    {
                        bool checksum = true;
                        var paramSize = int.Parse(header[1].ToString());
                        var paramBytes = new byte[paramSize];
                        if (read(paramBytes, paramSize))
                        {
                            checksum = HelperFunctions.validateChecksum(header, paramBytes);
                            if (checksum == true)
                            {
                                // Run command header[0]
                                executeCommand(header[0], paramBytes);
                                var message = Encoding.UTF8.GetBytes("VERSION:" + firmwareVersion + char.MinValue);
                                printer.WriteSerialToHost(message, message.Length);
                            }
                            else
                            {
                                var message = Encoding.UTF8.GetBytes("CHECKSUM" + char.MinValue);
                                printer.WriteSerialToHost(message, message.Length);
                            }
                        }
                    }
                    else
                        continue;
                }
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e.StackTrace);
            }
        }

        public void executeCommand(byte command, byte[] param)
        {
            try
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
            catch (Exception e)
            {
                System.Console.WriteLine(e.StackTrace);
            }
        }

        public void moveStepperToTop()
        {
            try
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
            catch (Exception e)
            {
                System.Console.WriteLine(e.StackTrace);
            }
        }

        public void moveStepperFromTopToBuildPlate()
        {
            try
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
            catch (Exception e)
            {
                System.Console.WriteLine(e.StackTrace);
            }
        }
    }
}
