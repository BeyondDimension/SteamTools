#if WINDOWS
// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

partial class WindowsPlatformServiceImpl : IRegistryService
{
    static readonly Lazy<string> _regedit_exe = new(() =>
    {
        var regedit_exe = Path.Combine(_windir!.Value, "regedit.exe");
        return regedit_exe;
    });

    /// <summary>
    /// %windir%\regedit.exe
    /// </summary>
    public static string Regedit => _regedit_exe.Value;

    /// <summary>
    /// 带参数(可选/null)启动 %windir%\regedit.exe
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    [Obsolete("use StartProcessRegedit(string path, string content, int millisecondsDelay)", true)]
    public static Process? StartProcessRegedit(string? args)
        => Process2.Start(Regedit, args, workingDirectory: _windir.Value);

    /// <summary>
    /// 带参数(可选/null)启动 %windir%\regedit.exe 并等待退出后删除文件
    /// </summary>
    /// <param name="path"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void StartProcessRegedit(
        string path,
        string contents,
        int millisecondsDelay = 3700)
    {
        File.WriteAllText(path, contents, Encoding.UTF8);
        if (IsPrivilegedProcess)
        {
            // 管理员权限则直接执行
            StartProcessRegeditCore(path, millisecondsDelay);
        }
        else
        {
            // 通过 IPC 调用管理员服务进程执行
            var ipcPlatformService = IPlatformService.IPCRoot.Instance.GetAwaiter().GetResult();
            ipcPlatformService.StartProcessRegeditCoreIPC(path, millisecondsDelay);
        }
    }

    void IRegistryService.StartProcessRegedit(
        string path,
        string contents,
        int millisecondsDelay) => StartProcessRegedit(path, contents, millisecondsDelay);

    /// <inheritdoc cref="StartProcessRegedit(string, string, int)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void StartProcessRegeditCore(
        string path,
        int millisecondsDelay = 3700)
    {
        var args = $"/s \"{path}\"";
        var p = Process2.Start(Regedit, args, workingDirectory: _windir.Value);
        IOPath.TryDeleteInDelay(p, path, millisecondsDelay, millisecondsDelay);
    }

    #region Registry2

    /// <inheritdoc cref="Registry2.ReadRegistryKey(string, RegistryView)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string? ReadRegistryKeyCore(string encodedPath, RegistryView view = Registry2.DefaultRegistryView)
    {
        var result = Registry2.ReadRegistryKey(encodedPath, view);
        var str = result?.ToString();
        return str;
    }

    /// <inheritdoc cref="Registry2.ReadRegistryKey(string, RegistryView)"/>
    public string? ReadRegistryKey(string encodedPath, RegistryView view)
    {
        if (IsPrivilegedProcess)
        {
            // 管理员权限则直接执行
            var str = ReadRegistryKeyCore(encodedPath, view);
            return str;
        }
        else
        {
            // 通过 IPC 调用管理员服务进程执行
            var ipcPlatformService = IPlatformService.IPCRoot.Instance.GetAwaiter().GetResult();
            var str = ipcPlatformService.ReadRegistryKey(encodedPath, view);
            return str;
        }
    }

    /// <inheritdoc cref="Registry2.SetRegistryKey(string, RegistryView, string?)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool SetRegistryKeyCore(string encodedPath, RegistryView view, string? value = null)
    {
        var result = Registry2.SetRegistryKey(encodedPath, view, value);
        return result;
    }

    public bool SetRegistryKey(string encodedPath, RegistryView view, string? value)
    {
        if (IsPrivilegedProcess)
        {
            // 管理员权限则直接执行
            var result = SetRegistryKeyCore(encodedPath, view, value);
            return result;
        }
        else
        {
            // 通过 IPC 调用管理员服务进程执行
            var ipcPlatformService = IPlatformService.IPCRoot.Instance.GetAwaiter().GetResult();
            var result = ipcPlatformService.SetRegistryKey(encodedPath, view, value);
            return result;
        }
    }

    /// <inheritdoc cref="Registry2.DeleteRegistryKeyCore(string, RegistryView)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool DeleteRegistryKeyCore(string encodedPath, RegistryView view = Registry2.DefaultRegistryView)
    {
        var result = Registry2.DeleteRegistryKey(encodedPath, view);
        return result;
    }

    public bool DeleteRegistryKey(string encodedPath, RegistryView view)
    {
        if (IsPrivilegedProcess)
        {
            // 管理员权限则直接执行
            var result = DeleteRegistryKeyCore(encodedPath, view);
            return result;
        }
        else
        {
            // 通过 IPC 调用管理员服务进程执行
            var ipcPlatformService = IPlatformService.IPCRoot.Instance.GetAwaiter().GetResult();
            var result = ipcPlatformService.DeleteRegistryKey(encodedPath, view);
            return result;
        }
    }

    #endregion
}
#endif