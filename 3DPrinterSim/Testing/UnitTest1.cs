using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrinterSimulator;

namespace Testing
{
    [TestClass]
    public class Checksum
    {
        [TestMethod]
        public void TestValidate()
        {
            byte[] header = new byte[4] { 0x02, 0x04, 0x00, 0xc5 };
            byte[] param = new byte[] { 0x00, 0x00, 0x80, 0x3f};
            bool valid = HelperFunctions.validateChecksum(header, param);
            Assert.IsTrue(valid);
        }
        [TestMethod]
        public void TestValidateMoveGalvonometer()
        {
            byte[] header = new byte[] { 0x04, 0x08, 0x03, 0x60 };
            byte[] param = new byte[] { 0x27, 0xa0, 0x09, 0xbc, 0x9b, 0x42, 0xad, 0x3e };
            bool valid = HelperFunctions.validateChecksum(header, param);
            Assert.IsTrue(valid);
        }
        [TestMethod]
        public void TestGenerateChecksum()
        {
            byte[] message = new byte[] { 0x03, 0x04, 0x00, 0x00, 0x00, 0x00, 0x80, 0x3f };
            byte[] checksum = HelperFunctions.ParseCmdPacket(message);
            var expected = new byte[] { 0x00, 0xc6 };
            Assert.AreEqual(checksum[0], expected[0]);
            Assert.AreEqual(checksum[1], expected[1]);
        }

        public void TestGenerateChecksum1()
        {
            byte[] message = new byte[] { 0x04, 0xbd };
        }
    }
}
