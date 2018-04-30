using Hardware;
using PrinterSimulator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace PrinterSimulator
{
    public enum Command
    {
        GetFirmwareVersion,
        ResetStepper,
        StepStepper,
        SetLaser,
        MoveGalvonometer,
        WaitMicroseconds,
        RemoveModelFromPrinter
    }
    public class HostHandler
    {
        private PrinterControl printer;
        public string firmwareVersion = "FIRMWARE VERSION: ";
        public HostHandler(PrinterThread thread)
        {
            this.printer = thread.GetPrinterSim();
        }
        public void execute(Command cmd, float[] param)
        {
            try
            {
                var paramBytes = getParamAsBytes(param);
                var byteMessage = new byte[4];
                switch (cmd)
                {
                    case Command.GetFirmwareVersion:
                        byteMessage = new byte[4] { 0x00, 0x00, 0x00, 0x00 };
                        break;
                    case Command.ResetStepper:
                        byteMessage = new byte[4] { 0x01, 0x00, 0x00, 0x00 };
                        break;
                    case Command.StepStepper:
                        byteMessage = new byte[4] { 0x02, 0x04, 0x00, 0x00 };
                        // send command with param[0] (up or down)
                        break;
                    case Command.SetLaser:
                        byteMessage = new byte[4] { 0x03, 0x04, 0x00, 0x00 };
                        // send command param[0] (on or off)
                        break;
                    case Command.MoveGalvonometer:
                        byteMessage = new byte[4] { 0x04, 0x08, 0x00, 0x00 };
                        // send commandwith param[0] x and param[1] y
                        break;
                    case Command.RemoveModelFromPrinter:
                        break;
                    case Command.WaitMicroseconds:
                        break;
                    default:
                        Console.WriteLine("Not a valid command");
                        break;
                }
                handleMessage(cmd, byteMessage, paramBytes, param);
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e.StackTrace);
            }
        }

        private byte[] getFullMessage(byte[] header, byte[] param)
        {
            var fullMessage = new byte[header.Length + param.Length];
            for (int i = 0; i < header.Length; i++)
            {
                fullMessage[i] = header[i];
            }
            for (int i = 0; i < param.Length; i++)
            {
                fullMessage[i + header.Length] = param[i];
            }
            return fullMessage;
        }

        private byte[] getParamAsBytes(float[] param)
        {
            byte[] paramReturn = new byte[param.Length * 4];
            Buffer.BlockCopy(param, 0, paramReturn, 0, paramReturn.Length);
            return paramReturn;
        }

        private void handleMessage(Command cmd, byte[] byteMessage, byte[] paramBytes, float[] floatParam)
        {
            var fullMessage = getFullMessage(byteMessage, paramBytes);
            var checksum = HelperFunctions.ParseCmdPacket(fullMessage);
            byteMessage[2] = checksum[0];
            byteMessage[3] = checksum[1];
            bool done = false;
            while (!done)
            {
                if (byteMessage[1] > 0x08)
                {
                    var breakp = 0;
                }
                Console.WriteLine("Host: Writing command {0} to firmware", cmd);
                Console.Write("Host: Params in command: ");
                foreach (float p in floatParam)
                {
                    Console.Write("{0}, ", p);
                }
                Console.Write("\n");
                Console.WriteLine("Host: Checksum verification returned {0}", HelperFunctions.validateChecksum(byteMessage, paramBytes));
                printer.WriteSerialToFirmware(byteMessage, byteMessage.Length);
                var finalResponse = "";
                var headerResponse = new byte[4];
                Console.WriteLine("Host: Reading header back from firmware");
                if (!read(headerResponse, 4))
                {
                    continue;
                }
                bool isRight = byteMessage.SequenceEqual(headerResponse);
                if (isRight)
                {
                    Console.WriteLine("Host: Header was correct, sending parameter data");
                    printer.WriteSerialToFirmware(new byte[1] { 0xA5 }, 1);
                    printer.WriteSerialToFirmware(paramBytes, paramBytes.Length);
                    var ch = new byte[1] { 0xFF };
                    Console.WriteLine("Host: Waiting for success string");
                    while (!ch.SequenceEqual(new byte[1] { 0x00 }))
                    {
                        var readResult = read(ch, 1);
                        if (!readResult)
                        {
                            ch[0] = 0x00;
                        }
                        finalResponse += System.Text.Encoding.UTF8.GetString(ch);
                    }
                    if (finalResponse.Split(':')[0] == "VERSION")
                    {
                        Console.WriteLine("Host: received version confirmation\n");
                        this.firmwareVersion = "FIRMWARE VERSION: " + finalResponse.Split(':')[1].Replace("\0", string.Empty);
                        done = true;
                        return;
                    }
                    else
                    {
                        Console.WriteLine("Host: Did not receive version confirmation\n");
                        continue;
                    }
                }
                else
                {
                    Console.WriteLine("Host: Header was wrong, resending command\n");
                    printer.WriteSerialToFirmware(new byte[1] { 0xFF }, 1);
                    continue;
                }
            }
        }
        private bool read(byte[] buffer, int expectedBytes)
        {
            int timeout = 100;
            while (printer.ReadSerialFromFirmware(buffer, expectedBytes) != expectedBytes && timeout > 0)
            {
                System.Threading.Thread.Sleep(20);
                timeout--;
                if (timeout == 0)
                {
                    Console.WriteLine("Host: Read timeout");
                    System.Threading.Thread.Sleep(5);
                    return false;
                }
            }
            return true;
        }
    }
}
