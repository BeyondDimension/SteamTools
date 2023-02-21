#if WINDOWS

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

partial class WindowsPlatformServiceImpl
{
    public const string OpenDesktopIconsSettingsCommandArguments = "shell32.dll,Control_RunDLL desk.cpl,,0";
    public const string OpenGameControllersCommandArguments = "shell32.dll,Control_RunDLL joy.cpl";

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static Process? Rundll32(string arguments) => Process.Start(new ProcessStartInfo
    {
        FileName = "rundll32.exe",
        Arguments = arguments,
        UseShellExecute = true,
    });

    public void OpenDesktopIconsSettings()
        => Rundll32(OpenDesktopIconsSettingsCommandArguments);

    public void OpenGameControllers()
        => Rundll32(OpenGameControllersCommandArguments);
}
#endif