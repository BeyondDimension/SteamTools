#if WINDOWS_NT
using NUnit.Framework;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace SevenZip.Tests
{
    [SetUpFixture]
    public class SetupFixture
    {
        bool isFirstOneTimeSetUp;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            // TODO: Add code here that is run before
            //  all tests in the assembly are run
            if (!isFirstOneTimeSetUp)
            {
                isFirstOneTimeSetUp = true;

                if (OperatingSystem.IsWindows())
                {
                    var sevenZipLibraryPath = Path.Combine(AppContext.BaseDirectory, "7z.dll");
                    if (!File.Exists(sevenZipLibraryPath))
                    {
                        sevenZipLibraryPath = Path.Combine(AppContext.BaseDirectory, "runtimes", RuntimeInformation.ProcessArchitecture switch
                        {
                            Architecture.X86 => "win-x86",
                            Architecture.X64 => "win-x64",
                            Architecture.Arm => "win-arm",
                            Architecture.Arm64 => "win-arm64",
                            _ => throw new PlatformNotSupportedException(),
                        }, "native", "7z.dll");
                    }
                    SevenZipBase.SetLibraryPath(sevenZipLibraryPath);
                }
            }
        }
    }
}
#endif