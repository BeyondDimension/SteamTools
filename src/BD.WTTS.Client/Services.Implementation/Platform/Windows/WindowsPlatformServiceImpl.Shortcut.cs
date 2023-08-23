#if WINDOWS
using IWshRuntimeLibrary;
using SkiaSharp;
using System.CommandLine;
using Res = BD.WTTS.Properties.Resources;

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

partial class WindowsPlatformServiceImpl
{
    /// <summary>
    /// 创建快捷方式
    /// </summary>
    /// <param name="pathLink">快捷方式的保存路径</param>
    /// <param name="targetPath">快捷方式的目标路径</param>
    /// <param name="arguments">参数</param>
    /// <param name="description">快捷键描述</param>
    /// <param name="hotkey">设置快捷键</param>
    /// <param name="iconLocation">快捷键方式图标路径</param>
    /// <param name="workingDirectory">应用程序工作目录</param>
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

    void IPlatformService.CreateShortcut(
        string pathLink,
        string targetPath,
        string? arguments,
        string? description,
        string? hotkey,
        string? iconLocation,
        string? workingDirectory)
    {
        CreateShortcut(pathLink, targetPath, arguments, description, hotkey, iconLocation, workingDirectory);
    }
}
#endif