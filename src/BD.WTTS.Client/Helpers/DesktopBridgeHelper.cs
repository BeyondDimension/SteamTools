#if WINDOWS

using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;

// ReSharper disable once CheckNamespace
namespace BD.WTTS;

sealed class DesktopBridgeHelper : DesktopBridge
{
    DesktopBridgeHelper() => throw new NotSupportedException();

    public static bool Init(int min_os_build = 17763)
    {
        if (!OperatingSystem2.IsWindowsVersionAtLeast(10, 0, min_os_build)) return false;
        try
        {
            if (string.IsNullOrWhiteSpace(Package.Current.Id.Name)) return false;
        }
        catch
        {
            return false;
        }
        IsRunningAsUwp = true;
        WindowsRuntimeFileSystem.InitFileSystem();
        return true;
    }

    [Obsolete]
    public static void OnActivated(ref string[] args)
    {
        var activatedArgs = AppInstance.GetActivatedEventArgs();
        if (activatedArgs != null) OnActivated(ref args, activatedArgs);
    }

    [Obsolete]
    static void OnActivated(ref string[] main_args, IActivatedEventArgs args)
    {
        if (args.Kind == ActivationKind.StartupTask)
        {
            // 静默启动（不弹窗口）
            main_args = IPlatformService.SystemBootRunArguments.Split(' ');
        }
    }
}
#endif