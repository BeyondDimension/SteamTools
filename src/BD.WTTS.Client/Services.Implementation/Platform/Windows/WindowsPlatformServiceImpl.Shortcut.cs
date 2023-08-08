#if WINDOWS
using IWshRuntimeLibrary;

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

partial class WindowsPlatformServiceImpl
{
    /// <summary>
    /// 创建一个快捷方式
    /// </summary>
    /// <param name="pathLink"></param>
    /// <param name="targetPath"></param>
    /// <param name="arguments"></param>
    /// <param name="workingDirectory"></param>
    public static void CreateShortcut(
        string pathLink,
        string targetPath,
        string? arguments = null,
        string? workingDirectory = null)
    {
        WshShell shell = new();
        IWshShortcut shortcut = shell.CreateShortcut(pathLink);
        shortcut.TargetPath = targetPath;
        if (!string.IsNullOrEmpty(arguments))
            shortcut.Arguments = arguments;
        if (!string.IsNullOrEmpty(workingDirectory))
            shortcut.WorkingDirectory = workingDirectory;
        shortcut.Save();
    }
}
#endif