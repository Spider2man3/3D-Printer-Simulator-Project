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
            switch (cmd)
            {
                case Command.GetFirmwareVersion:
                    var byteMessage = new byte[4] { 0x00, 0x00, 0x00, 0x00 };
                    printer.WriteSerialToFirmware(byteMessage, 4);
                    handleResponse(cmd, byteMessage, param);
                    break;
                case Command.ResetStepper:
                    byteMessage = new byte[4] { 0x01, 0x00, 0x00, 0x00 };
                    printer.WriteSerialToFirmware(byteMessage, 4);// send command
                    handleResponse(cmd, byteMessage, param);
                    break;
                case Command.StepStepper:
                    byteMessage = new byte[4] { 0x02, 0x04, 0x00, 0x00 };
                    printer.WriteSerialToFirmware(byteMessage, 4);
                    handleResponse(cmd, byteMessage, param);
                    // send command with param[0] (up or down)
                    break;
                case Command.SetLaser:
                    byteMessage = new byte[4] { 0x03, 0x04, 0x00, 0x00 };
                    printer.WriteSerialToFirmware(byteMessage, 4);
                    Console.WriteLine("set laser param was");
                    foreach (var par in param)
                    {
                        Console.WriteLine(par);
                    }
                    handleResponse(cmd, byteMessage, param);
                    // send command param[0] (on or off)
                    break;
                case Command.MoveGalvonometer:
                    byteMessage = new byte[4] { 0x04, 0x08, 0x00, 0x00 };
                    printer.WriteSerialToFirmware(byteMessage, 4);
                    handleResponse(cmd, byteMessage, param);
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
        }
        private void handleResponse(Command cmd, byte[] byteMessage, float[] param)
        {
            var finalResponse = "";
            var headerResponse = new byte[4];
            read(headerResponse, 4);
            bool isRight = byteMessage.SequenceEqual(headerResponse);
            if (byteMessage.SequenceEqual(headerResponse))
            {
                printer.WriteSerialToFirmware(new byte[1] { 0xA5 }, 1);
                byte[] finalMessage = new byte[param.Length * 4];
                Buffer.BlockCopy(param, 0, finalMessage, 0, finalMessage.Length);
                printer.WriteSerialToFirmware(finalMessage, finalMessage.Length);
                var ch = new byte[1] { 0xFF};
                while (!ch.SequenceEqual(new byte[1] { 0x00 }))
                {
                    read(ch, 1);
                    finalResponse += System.Text.Encoding.UTF8.GetString(ch);
                }
                if (finalResponse.Split(':')[0] == "VERSION")
                {
                    this.firmwareVersion = "FIRMWARE VERSION: " + finalResponse.Split(':')[1].Replace("\0", string.Empty);
                }
                else
                {
                    execute(cmd, param);
                }
            }
            else
            {
                printer.WriteSerialToFirmware(new byte[1] { 0xFF }, 1);
                execute(cmd, param);
            }

        }
        public bool read(byte[] buffer, int expectedBytes)
        {
            int timeoutCount = 0;
            int readResult = 0;
            while (readResult == 0 && timeoutCount < 100)
            {
                timeoutCount += 1;
                readResult = printer.ReadSerialFromFirmware(buffer, expectedBytes);
                if (readResult == 0)
                {
                    System.Threading.Thread.Sleep(10);
                }
            }
            return true;
        }
    }
}
