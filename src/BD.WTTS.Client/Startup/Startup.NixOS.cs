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
            };
            psi.ArgumentList.Add("start");
            psi.ArgumentList.Add(SERVICE_NAME);
            Process.Start(psi);
        }
        catch
        {
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
