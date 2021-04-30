using Microsoft.Extensions.Logging;

namespace System.Application.Services.Implementation
{
    public sealed class StartupToastIntercept : IToastIntercept
    {
        readonly ILogger logger;
        public StartupToastIntercept(ILogger<StartupToastIntercept> logger)
        {
            this.logger = logger;
        }

        public bool IsStartuped { get; set; }

        public void Log(string msg)
        {
            logger.LogCritical(msg);
        }

        public bool OnShowExecuting(string text)
        {
            if (IsStartuped)
            {
                return false;
            }

            Log(text);
            return true;
        }
    }
}