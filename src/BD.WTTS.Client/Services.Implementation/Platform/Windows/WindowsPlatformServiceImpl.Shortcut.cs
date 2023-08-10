#if WINDOWS
using IWshRuntimeLibrary;

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

partial class WindowsPlatformServiceImpl
{
    /// <summary>
    /// 创建一个快捷方式
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void CreateShortcut(
        string pathLink,
        string targetPath,
        string? arguments = null,
        string? description = null,
        string? hotkey = null,
        string? iconLocation = null,
        string? workingDirectory = null)
    {
        WshShell shell = new();
        var shortcut = (IWshShortcut)shell.CreateShortcut(pathLink);
        shortcut.TargetPath = targetPath;

        if (!string.IsNullOrEmpty(description))
            shortcut.Description = description;

        if (!string.IsNullOrEmpty(arguments))
            shortcut.Arguments = arguments;

        if (!string.IsNullOrEmpty(hotkey))
            shortcut.Hotkey = hotkey;

        if (!string.IsNullOrEmpty(iconLocation))
            shortcut.IconLocation = iconLocation;

        if (!string.IsNullOrEmpty(workingDirectory))
            shortcut.WorkingDirectory = workingDirectory;

        shortcut.Save();
    }
}
#endif