using Hardware;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserView
{
    public class HostController
    {
        PrinterControl controller;

        public HostController(PrinterThread pThread)
        {
            this.controller = pThread.GetPrinterSim();
        }

        

    }
}
