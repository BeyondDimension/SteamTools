using NUnit.Framework;

namespace System
{
    [SetUpFixture]
    public partial class SetupFixture
    {
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            // TODO: Add code here that is run before
            //  all tests in the assembly are run
            if (!DI.IsInit)
            {
                DI.Init(ConfigureServices);
            }
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            // TODO: Add code here that is run after
            //  all tests in the assembly have been run
        }
    }
}