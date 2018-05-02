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
        FirmwareHandler handler;
        bool fInitialized = false;

        public FirmwareController(PrinterControl printer)
        {
            this.printer = printer;
            handler = new FirmwareHandler(printer, firmwareVersion);
        }

        public void Start()
        {
            fInitialized = true;

            handler.Process(); // this is a blocking call
        }

        public void Stop()
        {
            handler.fDone = true;
        }

        public void WaitForInit()
        {
            while (!fInitialized)
                Thread.Sleep(100);
        }
    }
}
