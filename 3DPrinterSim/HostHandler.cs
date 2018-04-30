using Hardware;
using PrinterSimulator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace PrinterSimulator
{
    public class HostHandler
    {
        private PrinterControl printer;
        public HostHandler(PrinterThread thread)
        {
            this.printer = thread.GetPrinterSim();
        }
        public void execute(Command cmd, List<Object> param)
        {
            switch (cmd)
            {
                case Command.GetFirmwareVersion:
                    var byteMessage = new byte[4] { 0x00, 0x00, 0x00, 0x00 };
                    break;
                case Command.ResetStepper:
                    byteMessage = new byte[4] { 0x01, 0x00, 0x00, 0x00 };
                    printer.WriteSerialToFirmware(byteMessage, 4);// send command
                    handleResponse(cmd, byteMessage, param);
                    break;
                case Command.StepStepper:
                    byteMessage = new byte[4] { 0x02, 0x00, 0x00, 0x00 };
                    // send command with param[0] (up or down)
                    break;
                case Command.SetLaser:
                    byteMessage = new byte[4] { 0x03, 0x00, 0x00, 0x00 };
                    // send command param[0] (on or off)
                    break;
                case Command.MoveGalvonometer:
                    byteMessage = new byte[4] { 0x04, 0x00, 0x00, 0x00 };
                    // send commandwith param[0] x and param[1] y
                    break;
                default:
                    break;
            }
        }
        private void handleResponse(Command cmd, byte[] byteMessage, List<Object> param)
        {
            var finalResponse = "";
            var headerResponse = new byte[1];
            printer.ReadSerialFromFirmware(headerResponse, 1);
            if (headerResponse[0] == byteMessage[0])
            {
                printer.WriteSerialToFirmware(new byte[1] { 0xA5 }, 1);
                byte[] finalMessage = new byte[byteMessage.Length - 3];
                Array.Copy(byteMessage, 4, finalMessage, 0, byteMessage.Length - 3);
                printer.WriteSerialToFirmware(finalMessage, byteMessage.Length - 3);
                var ch = new byte[2] { 0xFF, 0xFF };
                while (ch != new byte[2] { 0x00, 0x00 })
                {
                    printer.ReadSerialFromFirmware(ch, 2);
                    finalResponse += System.Text.Encoding.UTF8.GetString(ch);
                }
                if (finalResponse != "SUCCESS")
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
    }
}
