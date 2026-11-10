// NixOS 不可变系统适配：加速器 systemd 服务式运行辅助（独立文件）
namespace BD.WTTS;

public static partial class StartupNixOS
{
    internal const string ENV_FILE_PATH = "/tmp/steampp-accel.env";
    internal const string ENV_PIPE_KEY = "STEAMPP_PIPE";
    internal const string ENV_PID_KEY = "STEAMPP_PID";
    internal const string ENV_MODEL_KEY = "STEAMPP_MODEL";
    internal const string SERVICE_NAME = "steampp-accelerator";
    internal const string SYSCTL_NAME = "systemctl";

    public static void StartAcceleratorService(string pipeName, int pid, string model)
    {
        try
        {
            var envFileContent = string.Join(Environment.NewLine,
                $"{ENV_PIPE_KEY}={pipeName}",
                $"{ENV_PID_KEY}={pid}",
                $"{ENV_MODEL_KEY}={model}") + Environment.NewLine;
            File.WriteAllText(ENV_FILE_PATH, envFileContent);
            var psi = new ProcessStartInfo(SYSCTL_NAME)
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };
            psi.ArgumentList.Add("start");
            psi.ArgumentList.Add(SERVICE_NAME);
            using var process = Process.Start(psi);
            if (process == null)
            {
                Log.Error(nameof(StartupNixOS), $"无法启动进程 {SYSCTL_NAME}");
                return;
            }
            var stdout = process.StandardOutput.ReadToEnd();
            var stderr = process.StandardError.ReadToEnd();
            process.WaitForExit();
            if (process.ExitCode != 0)
            {
                Log.Error(nameof(StartupNixOS),
                    $"{SYSCTL_NAME} start {SERVICE_NAME} 退出码 {process.ExitCode}，stdout: {stdout}，stderr: {stderr}");
            }
        }
        catch (Exception ex)
        {
            Log.Error(nameof(StartupNixOS), ex, $"{SYSCTL_NAME} start {SERVICE_NAME} 失败");
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
        }
        catch
        {
        }
    }
}
