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
    class FirmwareHandler
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
            int timeout = 10;
            int readResult = 0;
            while (timeout > 0 && readResult == 0)
            {
                readResult = printer.ReadSerialFromHost(buffer, expectedBytes);
                if (readResult == 0)
                {
                    timeout--;
                    Thread.Sleep(5);
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
                        commandsExecuted["StepStepperUp"] += 1;
                        bool result = printer.StepStepper(PrinterControl.StepperDir.STEP_UP);
                    }
                    else if (direction == 0)
                    {
                        commandsExecuted["StepStepperDown"] += 1;
                        bool result = printer.StepStepper(PrinterControl.StepperDir.STEP_DOWN);
                    }
                    break;
                case 0x04:
                    commandsExecuted["MoveGalvonometer"] += 1;
                    float x = BitConverter.ToSingle(param, 0);
                    float y = BitConverter.ToSingle(param, 4);
                    printer.MoveGalvos(x, y);
                    break;
                default:
                    Console.WriteLine("Bad command");
                    break;
            }
        }

        public void moveStepperToTop()
        {
            double stepsPerMicro = .0016;
            long microPerStep = 625;
            while (printer.LimitSwitchPressed() == false)
            {
                bool printerStepped = printer.StepStepper(PrinterControl.StepperDir.STEP_UP);
                if (!printerStepped && !printer.LimitSwitchPressed())
                {
                    throw new Exception("printer step failed, speed is wrong?");
                }
                printer.WaitMicroseconds(microPerStep);
                microPerStep = (long)(1 / stepsPerMicro);
                if (microPerStep > 64)
                {
                    stepsPerMicro += .0016;
                }
                else
                {
                    microPerStep = 64;
                }
            }
            Console.WriteLine("Limit switch pressed?");
        }

        public void moveStepperFromTopToBuildPlate()
        {
            double stepsPerMicro = .0016;
            long microPerStep = 625;
            int height = 40000;
            while (height > 0)
            {
                bool printerStepped = printer.StepStepper(PrinterControl.StepperDir.STEP_DOWN);
                height -= 1;
                if (!printerStepped && height > 0)
                {
                    throw new Exception("printer step failed, speed is wrong?");
                }
                printer.WaitMicroseconds(microPerStep);
                microPerStep = (long)(1 / stepsPerMicro);
                if (microPerStep > 63)
                {
                    stepsPerMicro += .0016;
                }
                else
                {
                    microPerStep = 63;
                }
            }
            Console.WriteLine("Limit switch pressed?");
        }
    }
}
