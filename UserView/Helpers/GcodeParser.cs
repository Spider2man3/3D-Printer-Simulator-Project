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
        public List<List<string>> ParseGcode(Stream file)
        {
            List<List<string>> listOfG = new List<List<string>>();

            var g1 = "G1";
            var g92 = "G92";
            var lineCounter = 0;
            using (var sr = new StreamReader(file))
            {

                while (!sr.EndOfStream)
                {
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
                        List<string> G1Line = new List<string>();
                        string counterString = lineCounter.ToString();
                        G1Line.Add(counterString);
                        if (xString != null && xString.Length != 0)
                        {
                            G1Line.Add(string.Join("", xString));
                        }
                        if (yString != null && yString.Length != 0)
                        {
                            G1Line.Add(string.Join("", yString));
                        }
                        if (zString != null && zString.Length != 0)
                        {
                            G1Line.Add(string.Join("", zString));
                        }
                        if (eString != null && eString.Length != 0)
                        {
                            G1Line.Add(string.Join("", eString));
                        }
                        
                        listOfG.Add(G1Line);
                    }
                    if (line.IndexOf(g92, StringComparison.CurrentCultureIgnoreCase) >= 0)
                    {
                        var eachLine = line.Split(' ');
                        var eString = Array.FindAll(eachLine, x => x.StartsWith("E"));
                        List<string> G92Line = new List<string>();
                        if (eString != null && eString.Length != 0)
                        {
                            G92Line.Add(string.Join("", eString));
                        }
                        listOfG.Add(G92Line);
                    }

                }
                return listOfG;
            }


        }
     
    }
}