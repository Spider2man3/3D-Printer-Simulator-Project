using Hardware;
using PrinterSimulator;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Host
{
    public class GcodeParser
    {
        public void ParseGcode(Stream file, HostHandler handler)
        {
            //List<List<string>> listOfG = new List<List<string>>();

            var g1 = "G1";
            var g92 = "G92";
            var lineCounter = -1;
            using (var sr = new StreamReader(file))
            {
                while (!sr.EndOfStream)
                {
                    List<string> GLine = new List<string>();
                    lineCounter++;
                    var line = sr.ReadLine();
                    if (String.IsNullOrEmpty(line)) continue;
                    if (line.IndexOf(g1, StringComparison.CurrentCultureIgnoreCase) >= 0)
                    {
                        var eachLine = line.Split(' ');

                        var xString = Array.FindAll(eachLine, x => x.StartsWith("X"));
                        var yString = Array.FindAll(eachLine, x => x.StartsWith("Y"));
                        var eString = Array.FindAll(eachLine, x => x.StartsWith("E"));
                        var zString = Array.FindAll(eachLine, x => x.StartsWith("Z"));

                        if ((xString != null && xString.Length != 0) && (yString != null && yString.Length != 0))
                        {
                            var xFloat = float.Parse(xString.First().Substring(1));
                            var yFloat = float.Parse(yString.First().Substring(1));
                            handler.execute(Command.MoveGalvonometer, new float[] { xFloat, yFloat });
                        }
                        if (zString != null && zString.Length != 0)
                        {
                            handler.execute(Command.StepStepper, new float[] { 1f });
                        }
                        if (eString != null && eString.Length != 0)
                        {
                            var eFloat = float.Parse(eString.First().Substring(1));
                            if (eFloat == 0f)
                            {
                                handler.execute(Command.SetLaser, new float[] { 0f });
                            }
                            else
                            {
                                handler.execute(Command.SetLaser, new float[] { 1f });
                            }
                        }
                    }
                    else if (line.IndexOf(g92, StringComparison.CurrentCultureIgnoreCase) >= 0)
                    {
                        handler.execute(Command.SetLaser, new float[] { 0f });
                    }
                }
            }
        }
     
    }
}