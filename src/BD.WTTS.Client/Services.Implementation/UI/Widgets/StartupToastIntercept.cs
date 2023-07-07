// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

public sealed class StartupToastIntercept : IToastIntercept
{
    readonly ILogger logger;

    public StartupToastIntercept(ILogger<StartupToastIntercept> logger)
    {
        this.logger = logger;
        if (!IApplication.IsDesktop())
        {
            IsStartuped = true;
        }
    }

    public bool IsStartuped { get; private set; }

    public void Log(string msg)
    {
        logger.LogCritical(msg);
    }

    public bool OnShowExecuting(ToastIcon icon, string text, int? duration = null)
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
        var startupToastIntercept = Ioc.Get_Nullable<StartupToastIntercept>();
        if (startupToastIntercept != null)
        {
            startupToastIntercept.IsStartuped = true;
        }
    }
}