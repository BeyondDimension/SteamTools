using System;
using System.Diagnostics;
using System.IO;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using static System.Application.Services.IDesktopPlatformService;

static string? OnActivated(IActivatedEventArgs args)
{
    if (args.Kind == ActivationKind.StartupTask)
    {
        // 静默启动（不弹窗口）
        return SystemBootRunArguments;
    }
    return default;
}

var activatedArgs = AppInstance.GetActivatedEventArgs();
try
{
    Process.Start(new ProcessStartInfo()
    {
        FileName = Path.Combine(AppContext.BaseDirectory, "..", "Steam++.exe"),
        Arguments = (activatedArgs == default ? default : OnActivated(activatedArgs)) ?? string.Join(" ", args),
        UseShellExecute = default,
    });
}
catch (FileNotFoundException)
{
}