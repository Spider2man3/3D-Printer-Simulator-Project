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
        public enum Commands
        {
            ResetStepper,
            StepStepper,
            MoveGalvonometer,
            SetLaser
        }
        PrinterControl controller;

        public HostController(PrinterThread pThread)
        {
            this.controller = pThread.GetPrinterSim();
        }
        private bool execute()
        {
            return true;
        }

        public async void SendCommand(Commands cmd, List<Object> param) //TODO: should be an async task?
        {
            var result = false;
            switch (cmd)
            {
                case Commands.ResetStepper:
                    result = true;// send command
                    break;
                case Commands.SetLaser:
                    result = true;// send command
                    break;
                case Commands.StepStepper:
                    result = true;// send command with param[0] (up or down)
                    break;
                case Commands.MoveGalvonometer:
                    result = true;// send commandwith param[0] x and param[1] y
                    break;
                default:
                    result = false;
                    break;
            }
        }



    }
}
