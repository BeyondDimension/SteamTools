using NUnit.Framework;
//using static System.Application.Program;

namespace System.Application
{
    [SetUpFixture]
    public class SetupFixture
    {
        bool isFirstOneTimeSetUp;

        //[OneTimeSetUp]
        //public void OneTimeSetUp()
        //{
        //    // TODO: Add code here that is run before
        //    //  all tests in the assembly are run
        //    if (!isFirstOneTimeSetUp)
        //    {
        //        isFirstOneTimeSetUp = true;
        //        var logDirPath = InitLogDir("_test");
        //        TestContext.WriteLine($"logDirPath: {logDirPath}");
        //    }
        //}
    }
}