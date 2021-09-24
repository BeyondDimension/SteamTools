using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using System.Logging;

namespace System
{
    [SetUpFixture]
    public class SetupFixture
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

        static void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(l => l.AddProvider(NUnitLoggerProvider.Instance));
            services.Configure<LoggerFilterOptions>(o =>
            {
                o.MinLevel = LogLevel.Trace;
            });
            services.AddPinyin();
        }
    }
}