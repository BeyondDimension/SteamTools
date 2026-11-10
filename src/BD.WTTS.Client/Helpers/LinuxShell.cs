#if LINUX
namespace BD.WTTS;

/// <summary>
/// Linux 下可用的 bash 路径。
/// NixOS 等不可变发行版的 <c>/bin</c> 中只有 <c>sh</c>（bash 在 /run/current-system/sw/bin 或用户 profile 中），
/// 直接使用 <c>/bin/bash</c> 会抛 Win32Exception(2)。
/// </summary>
public static class LinuxShell
{
    static string? bashPath;

    /// <summary>
    /// 解析可用的 bash：优先 /bin/bash，其次常见绝对路径，再按 PATH 查找，最后回退 /bin/sh。
    /// </summary>
    public static string BashPath => bashPath ??= ResolveBash();

    static string ResolveBash()
    {
        foreach (var path in new[]
        {
            "/bin/bash",
            "/usr/bin/bash",
            "/run/current-system/sw/bin/bash",
        })
        {
            try
            {
                if (File.Exists(path))
                    return path;
            }
            catch
            {
            }
        }

        try
        {
            var pathEnv = Environment.GetEnvironmentVariable("PATH");
            if (!string.IsNullOrWhiteSpace(pathEnv))
            {
                foreach (var dir in pathEnv.Split(':', StringSplitOptions.RemoveEmptyEntries))
                {
                    var candidate = Path.Combine(dir, "bash");
                    if (File.Exists(candidate))
                        return candidate;
                }
            }
        }
        catch
        {
        }

        // 让 Process.Start 走 PATH 查找；仍找不到时由调用方捕获异常
        return "bash";
    }
}
#endif
