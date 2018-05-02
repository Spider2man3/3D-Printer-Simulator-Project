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
        moveStepper,
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
                case Command.moveStepper:
                    byteMessage = new byte[4] { 0x05, 0x04, 0x00, 0x00 };
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
        private String printMessage(byte[] message)
        {
            string result = "";
            foreach (byte b in message)
            {
                result += b.ToString() + ", ";
            }
            return result;
        }

        private void handleMessage(Command cmd, byte[] byteMessage, byte[] paramBytes, float[] floatParam)
        {
            var fullMessage = getFullMessage(byteMessage, paramBytes);
            var checksum = HelperFunctions.ParseCmdPacket(fullMessage);
            byteMessage[2] = checksum[0];
            byteMessage[3] = checksum[1];
            bool done = false;
            var finalResponse = "";
            while (!done)
            {
                //Console.WriteLine("Host: Writing command {0} to firmware", cmd);
                //Console.Write("Host: Params in command: ");
                //foreach (float p in floatParam)
                //{
                //    Console.Write("{0}, ", p);
                //}
                //Console.Write("\n");
                //Console.WriteLine("Host: Checksum verification returned {0}", HelperFunctions.validateChecksum(byteMessage, paramBytes));
                //Console.WriteLine("Host: Writing bytes {0}", printMessage(byteMessage));
                var fill = new byte[byteMessage.Length];
                Array.Copy(byteMessage, fill, byteMessage.Length);
                printer.WriteSerialToFirmware(fill, byteMessage.Length);
                finalResponse = "";
                var headerResponse = new byte[4];
                //Console.WriteLine("Host: Reading header back from firmware");
                if (!read(headerResponse, 4))
                {
                    continue;
                }
                //Console.WriteLine("Host: Received header response of {0}", printMessage(headerResponse));
                //Console.WriteLine("Host: Comparing message: {0} to response {1} returned {2}", printMessage(fill), printMessage(headerResponse), byteMessage.SequenceEqual(headerResponse));
                bool isRight = byteMessage.SequenceEqual(headerResponse);
                if (isRight)
                {
                    //Console.WriteLine("Host: Header was correct, sending parameter data");
                    printer.WriteSerialToFirmware(new byte[1] { 0xA5 }, 1);
                    var fill2 = new byte[paramBytes.Length];
                    Array.Copy(paramBytes, fill2, paramBytes.Length);
                    printer.WriteSerialToFirmware(fill2, fill2.Length);
                    var ch = new byte[1] { 0xFF };
                    //Console.WriteLine("Host: Waiting for success string");
                    while (!ch.SequenceEqual(new byte[1] { 0x00 }))
                    {
                        var readResult = read(ch, 1);
                        //Console.WriteLine("Host: Received character of {0}", printMessage(ch));
                        if (ch[0] == 0x00)
                        {
                            break;
                        }
                        finalResponse += System.Text.Encoding.UTF8.GetString(ch);
                    }
                    //Console.WriteLine("Host: Received response string of {0}", finalResponse);
                    if (finalResponse.Split(':')[0] == "VERSION")
                    {
                        //Console.WriteLine("Host: received version confirmation\n");
                        this.firmwareVersion = "FIRMWARE VERSION: " + finalResponse.Split(':')[1].Replace("\0", string.Empty);
                        done = true;
                        return;
                    }
                    else
                    {
                        //Console.WriteLine("Host: Did not receive version confirmation\n");
                        continue;
                    }
                }
                else
                {
                    //Console.WriteLine("Host: Header was wrong, resending command\n");
                    printer.WriteSerialToFirmware(new byte[1] { 0xFF }, 1);
                    continue;
                }
            }

        }
        private bool read(byte[] buffer, int expectedBytes)
        {
           // int timeout = 100;
            while (printer.ReadSerialFromFirmware(buffer, expectedBytes) != expectedBytes)
            {
                //System.Threading.Thread.Sleep(2);
                //timeout--;
                //if (timeout == 0)
                //{
                //    Console.WriteLine("Host: Read timeout");
                //    System.Threading.Thread.Sleep(5);
                //    return false;
                //}
            }
            return true;
        }
    }
}
