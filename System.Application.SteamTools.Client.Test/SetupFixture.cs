using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using System.IO;
using System.Logging;

namespace System.Application
{
    [SetUpFixture]
    public class SetupFixture
    {
        public static bool DIInit { get; set; }

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            // TODO: Add code here that is run before
            //  all tests in the assembly are run
            if (!DIInit)
            {
                DI.Init(ConfigureServices);

                var path = AppContext.BaseDirectory;
                var path1 = Path.Combine(path, "AppData");
                IOPath.DirCreateByNotExists(path1);
                var path2 = Path.Combine(path, "Cache");
                IOPath.DirCreateByNotExists(path2);
                string GetAppDataDirectory() => path1;
                string GetCacheDirectory() => path2;
                IOPath.InitFileSystem(GetAppDataDirectory, GetCacheDirectory);
            }
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            // TODO: Add code here that is run after
            //  all tests in the assembly have been run
        }

        static void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(l => l.AddProvider(NUnitLoggerProvider.Instance));
            services.Configure<LoggerFilterOptions>(o =>
            {
                o.MinLevel = LogLevel.Trace;
            });
            services.TryAddHttpPlatformHelper();
            services.AddHttpService();
        }
    }
}