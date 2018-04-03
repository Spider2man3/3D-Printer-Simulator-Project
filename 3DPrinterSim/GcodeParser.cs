using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3DPrinterSim
{
    public class GcodeParser
    {
        public List<string> ParseGcode(StreamReader file)
        {
            List<List<string>> listOfG = new List<List<string>>();
           
            var g1 = "G1";
            var g92 = "G92"; 
            using (var sr = new StreamReader(""))
            {

                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine();
                    if (String.IsNullOrEmpty(line)) continue;
                    if (line.IndexOf(g1, StringComparison.CurrentCultureIgnoreCase) >= 0)
                    {
                        //I don't know what I'm doing here anymore...
                        var eachLine = line.Split(' ');
                        var xString = Array.FindAll(eachLine, x => x.Equals("X"));
                        var yString = Array.FindAll(eachLine, x => x.Equals("Y"));
                        var eString = Array.FindAll(eachLine, x => x.Equals("E"));
                        var zString = Array.FindAll(eachLine, x => x.Equals("Z"));
                        List<string> G1Line = new List<string>();
                        G1Line.Add(string.Join("", xString));
                        G1Line.Add(string.Join("", yString));
                        G1Line.Add(string.Join("", zString));
                        G1Line.Add(string.Join("", eString));
                        listOfG.Add(G1Line);    
                    }
                    if (line.IndexOf(g92, StringComparison.CurrentCultureIgnoreCase) >= 0)
                    {
                        var eachLine = line.Split(' ');
                        var eString = Array.FindAll(eachLine, x => x.Equals("E"));
                        List<string> G92Line = new List<string>();
                        G92Line.Add(string.Join("", eString));
                        listOfG.Add(G92Line);
                    }                
                }
            }
            return null; 
      
        }
        public static string getString(string strSource, string strStart)
        {
            int Start;
            if (strSource.Contains(strStart))
            {
                Start = strSource.IndexOf(strStart, 0) + strStart.Length;
                return strSource.Substring(Start);
            }
            else
            {
                return "";
            }
        }
    }
}
