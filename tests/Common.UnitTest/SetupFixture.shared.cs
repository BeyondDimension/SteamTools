using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Logging;

namespace System
{
    partial class SetupFixture
    {
        public static void ConfigureServices(IServiceCollection services)
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
