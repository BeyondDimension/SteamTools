using NUnit.Framework;

namespace System
{
    [TestFixture]
    public class ByteArrayTest
    {
        [Test]
        public void ByteArrayTestMethod()
        {
            var values = new byte[] { 0xff };
            var sbytes = values.ToSByteArray();
            var bytes = sbytes.ToByteArray();
            Assert.IsTrue(bytes[0] == values[0]);
        }
    }
}