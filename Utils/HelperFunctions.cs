using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrinterSimulator
{
    public class HelperFunctions
    {
        public static byte[] ParseCmdPacket(byte[] cmdPacket)
        {
            var byteSum = 0;
            for (int i = 0; i <= cmdPacket[1] + 4; i++)
            {
                byteSum += i;
            }
            ushort checkSum = (ushort)((12 * byteSum) / 16);
            cmdPacket[2] = (byte)(checkSum >> 8);
            cmdPacket[3] = (byte)(checkSum & 0xff);

            return cmdPacket;
        }
    }
}
