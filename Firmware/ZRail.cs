using Hardware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Firmware
{
    class ZRail
    {
        private PrinterControl printer;
        private double plateHeight = 0;
        List<Tuple<double, double>> moves = new List<Tuple<double, double>>();
        public ZRail(PrinterControl printer)
        {
            this.printer = printer;
        }
        
        public void resetStepperToBuildPlate()
        {
            moveStepperToTop();
            moveStepperFromTopToBuildPlate();
        }
        public void moveStepperToTop()
        {
            double currentMicrosecondWait = 635;
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
                    currentMicrosecondWait -= .050;
                    currentWait = 0;
                }

            }
            plateHeight = 100;
        }

        public void moveStepperFromTopToBuildPlate()
        {
            double currentMicrosecondWait = 635;
            double currentWait = 0;
            int height = 40000;
            while (plateHeight > 0)
            {
                bool printerStepped = printer.StepStepper(PrinterControl.StepperDir.STEP_DOWN);
                plateHeight -= 1.0 / 400.0;
                height -= 1;
                if (!printerStepped && height > 0)
                {
                    throw new Exception("printer step failed, speed is wrong?");
                }
                printer.WaitMicroseconds((long)currentMicrosecondWait);
                currentWait += currentMicrosecondWait;
                if (currentWait > 1000 && currentMicrosecondWait > 69)
                {
                    currentMicrosecondWait -= .050;
                    currentWait = 0;
                }
            }
            Console.WriteLine("Limit switch pressed?");
        }

        public void moveStepper(float newZ)
        {
            double currentMicrosecondWait = 635;
            double currentWait = 0;
            moves.Add(new Tuple<double, double>(plateHeight, newZ));
            int distanceLeft = (int)Math.Round(400 * (newZ - plateHeight));
            if (distanceLeft > 0)
            {
                while (distanceLeft > 0 && plateHeight < 100)
                {
                    bool printerStepped = printer.StepStepper(PrinterControl.StepperDir.STEP_UP);
                    if (!printerStepped)
                    {
                        throw new Exception("Printer step up failed");
                    }
                    distanceLeft -= 1;
                    plateHeight += 1.0 / 400.0;
                    printer.WaitMicroseconds((long)currentMicrosecondWait);
                    currentWait += currentMicrosecondWait;
                    if (currentWait > 1000 && currentMicrosecondWait > 69)
                    {
                        currentMicrosecondWait -= .050;
                        currentWait = 0;
                    }
                }
            }
            else if (distanceLeft < 0)
            {
                while (distanceLeft < 0 && plateHeight > 0)
                {
                    bool printerStepped = printer.StepStepper(PrinterControl.StepperDir.STEP_DOWN);
                    if (!printerStepped)
                    {
                        throw new Exception("Printer step down failed");
                    }
                    distanceLeft += 1;
                    plateHeight -= 1.0 / 400.0;
                    printer.WaitMicroseconds((long)currentMicrosecondWait);
                    currentWait += currentMicrosecondWait;
                    if (currentWait > 1000 && currentMicrosecondWait > 69)
                    {
                        currentMicrosecondWait -= .051;
                        currentWait = 0;
                    }
                } 
            }
        }
    }
}
