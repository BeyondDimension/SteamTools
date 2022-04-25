using Microsoft.Extensions.Logging;

namespace System.Application.Services.Implementation
{
    public sealed class StartupToastIntercept : IToastIntercept
    {
        readonly ILogger logger;

        public StartupToastIntercept(ILogger<StartupToastIntercept> logger)
        {
            this.logger = logger;
            if (!OperatingSystem2.IsDesktop)
            {
                IsStartuped = true;
            }
        }

        public bool IsStartuped { get; private set; }

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

        public static void OnStartuped()
        {
            var startupToastIntercept = DI.Get_Nullable<StartupToastIntercept>();
            if (startupToastIntercept != null)
            {
                startupToastIntercept.IsStartuped = true;
            }
        }
    }
}