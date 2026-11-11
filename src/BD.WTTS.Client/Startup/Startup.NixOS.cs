// NixOS 不可变系统适配：加速器 systemd 服务式运行辅助（独立文件）
namespace BD.WTTS;

public static partial class StartupNixOS
{
    /// <summary>
    /// 加速器启动参数状态文件。
    /// 由 module.nix 的 systemd.tmpfiles.rules 在 /run 下创建专用目录（属主为服务用户，
    /// 权限 0755），应用写入参数，systemd 读取后启动加速器。
    /// 不用 /tmp：/tmp 下同名文件无法保证归属与生命周期，且重启后残留会被当作有效参数。
    /// </summary>
    internal const string ENV_FILE_PATH = "/run/watt-toolkit/accelerator.env";

    internal const string ENV_FILE_DIR = "/run/watt-toolkit";
    internal const string ENV_PIPE_KEY = "STEAMPP_PIPE";
    internal const string ENV_PID_KEY = "STEAMPP_PID";
    internal const string ENV_MODEL_KEY = "STEAMPP_MODEL";
    internal const string SERVICE_NAME = "steampp-accelerator";
    internal const string SYSCTL_NAME = "systemctl";
    internal const string JOURNALCTL_NAME = "journalctl";

    /// <summary>
    /// 最近一次启动失败的原因（成功时为 null）。供 UI 在服务等待页展示，
    /// 避免只显示笼统的「无法连接加速服务」。
    /// </summary>
    public static volatile string? LastStartFailureDetail;

    public static bool StartAcceleratorService(string pipeName, int pid, string model)
    {
        LastStartFailureDetail = null;
        try
        {
            if (!Directory.Exists(ENV_FILE_DIR))
            {
                LastStartFailureDetail =
                    $"加速参数目录 {ENV_FILE_DIR} 不存在：未应用 module.nix 的 tmpfiles 规则（请重新 nixos-rebuild switch）";
                Log.Error(nameof(StartupNixOS), LastStartFailureDetail);
                return false;
            }

            // 先写临时文件再原子替换，避免 systemd 读到写了一半的参数
            var envLines = new List<string>
            {
                $"{ENV_PIPE_KEY}={pipeName}",
                $"{ENV_PID_KEY}={pid}",
                $"{ENV_MODEL_KEY}={model}",
            };
            // 主程序经 wrapper 启动时会设置 XDG_DATA_HOME（AppData 根目录为 $XDG_DATA_HOME/Steam++）。
            // 加速器子进程的 systemd 单元没有这个变量，会退回到 $HOME/.local/share/Steam++，
            // 导致它与主程序不是同一个 AppData：证书在一边生成、另一边读不到，被判为「未安装」。
            // 这里把主程序的 XDG_* 原值一并写进参数文件（EnvironmentFile 优先于单元 Environment），
            // 保证两个进程看到同一个 AppData / Cache。
            foreach (var name in new[] { "XDG_DATA_HOME", "XDG_CACHE_HOME" })
            {
                var value = Environment.GetEnvironmentVariable(name);
                if (!string.IsNullOrWhiteSpace(value))
                    envLines.Add($"{name}={value}");
            }
            var envFileContent = string.Join('\n', envLines) + "\n";
            var tmpPath = ENV_FILE_PATH + ".tmp";
            File.WriteAllText(tmpPath, envFileContent);
            File.Move(tmpPath, ENV_FILE_PATH, true);

            var (exitCode, stdout, stderr) = RunSystemctl("start", SERVICE_NAME);
            if (exitCode != 0)
            {
                LastStartFailureDetail = BuildFailureDetail(exitCode, stdout, stderr, pid, pipeName);
                Log.Error(nameof(StartupNixOS), LastStartFailureDetail);
                return false;
            }

            // systemctl start 只表示启动请求被接受，单元可能立刻退出（例如参数过期）
            var (stateCode, state, _) = RunSystemctl("is-active", SERVICE_NAME);
            if (stateCode != 0 || state.Trim() != "active")
            {
                LastStartFailureDetail = BuildFailureDetail(stateCode, state, "", pid, pipeName);
                Log.Error(nameof(StartupNixOS), LastStartFailureDetail);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            LastStartFailureDetail = $"{SYSCTL_NAME} start {SERVICE_NAME} 失败: {ex.Message}";
            Log.Error(nameof(StartupNixOS), ex, LastStartFailureDetail);
            return false;
        }
    }

    static string BuildFailureDetail(int exitCode, string stdout, string stderr, int pid, string pipeName)
    {
        var mainProcessAlive = false;
        try
        {
            mainProcessAlive = Process.GetProcessById(pid) != null;
        }
        catch
        {
        }

        var unitProps = RunSystemctl("show", SERVICE_NAME, "-p", "Result", "-p", "ExecMainStatus", "-p", "ExecMainCode").stdout.Trim();
        var journal = RunJournalTail(6);

        var sb = new System.Text.StringBuilder();
        sb.Append($"加速服务启动失败（pipe={pipeName}, pid={pid}, 主进程存活={mainProcessAlive}, 退出码={exitCode}）");
        if (!string.IsNullOrWhiteSpace(stdout)) sb.Append($"，输出: {stdout.Trim()}");
        if (!string.IsNullOrWhiteSpace(stderr)) sb.Append($"，错误: {stderr.Trim()}");
        if (!string.IsNullOrWhiteSpace(unitProps)) sb.Append($"；单元状态: {unitProps.Replace('\n', ' ')}");
        if (!string.IsNullOrWhiteSpace(journal)) sb.Append($"；日志: {journal.Trim().Replace('\n', ' ')}");
        return sb.ToString();
    }

    static (int ExitCode, string stdout, string stderr) RunSystemctl(params string[] arguments)
    {
        var psi = new ProcessStartInfo(SYSCTL_NAME)
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
        foreach (var arg in arguments) psi.ArgumentList.Add(arg);
        using var process = Process.Start(psi);
        if (process == null) return (-1, string.Empty, "无法启动 systemctl 进程");
        var stdout = process.StandardOutput.ReadToEnd();
        var stderr = process.StandardError.ReadToEnd();
        process.WaitForExit();
        return (process.ExitCode, stdout, stderr);
    }

    static string RunJournalTail(int lines)
    {
        try
        {
            var psi = new ProcessStartInfo(JOURNALCTL_NAME)
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };
            psi.ArgumentList.Add("-u");
            psi.ArgumentList.Add(SERVICE_NAME);
            psi.ArgumentList.Add("-n");
            psi.ArgumentList.Add(lines.ToString());
            psi.ArgumentList.Add("--no-pager");
            psi.ArgumentList.Add("-o");
            psi.ArgumentList.Add("cat");
            using var process = Process.Start(psi);
            if (process == null) return string.Empty;
            var stdout = process.StandardOutput.ReadToEnd();
            process.StandardError.ReadToEnd();
            process.WaitForExit();
            return stdout;
        }
        catch
        {
            return string.Empty;
        }
    }

    public static void StopAcceleratorService()
    {
        try
        {
            var psi = new ProcessStartInfo(SYSCTL_NAME)
            {
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            psi.ArgumentList.Add("stop");
            psi.ArgumentList.Add(SERVICE_NAME);
            Process.Start(psi);
            try
            {
                File.Delete(ENV_FILE_PATH);
            }
            catch
            {
            }
        }
        catch
        {
        }
    }
}
