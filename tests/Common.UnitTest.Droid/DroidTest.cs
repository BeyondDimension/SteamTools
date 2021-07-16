using NUnit.Framework;
using System.Application;

namespace System.UnitTest
{
    [TestFixture]
    public class DroidTest
    {
        [Test]
        public void EssentialsSupported()
        {
            Assert.IsTrue(XamarinEssentials.IsSupported);
        }
    }
}
