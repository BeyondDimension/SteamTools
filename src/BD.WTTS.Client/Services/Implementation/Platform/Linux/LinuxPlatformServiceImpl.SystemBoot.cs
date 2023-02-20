#if LINUX
// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

partial class LinuxPlatformServiceImpl
{
    async void IPlatformService.SystemShutdown(int waitSecond)
    {
        await Task.Delay(waitSecond);
        RunShell(nameof(IPlatformService.SystemShutdown),
            $"echo \"{SystemUserPassword}\" | sudo shutdown -h now");
    }

    async void IPlatformService.SystemSleep(int waitSecond)
    {
        await Task.Delay(waitSecond);
        RunShell(nameof(IPlatformService.SystemSleep),
            $"echo \"{SystemUserPassword}\" | sudo sh -c \" echo mem > /sys/pwoer/state\"");
    }

    async void IPlatformService.SystemHibernate(int waitSecond)
    {
        await Task.Delay(waitSecond);
        RunShell(nameof(IPlatformService.SystemHibernate),
            $"echo \"{SystemUserPassword}\" | sudo sh -c \" echo disk > /sys/pwoer/state\"");
    }

    static string RunShell(string shellCallName, string shell)
    {
        try
        {
            using var p = new Process();
            p.StartInfo.FileName = Process2.BinBash;
            p.StartInfo.Arguments = "";
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.UseShellExecute = false;
            p.Start();
            p.StandardInput.WriteLine(shell);
            //p.StandardInput.WriteLine(SystemUserPassword);
            p.StandardInput.Close();
            string result = p.StandardOutput.ReadToEnd();
            p.StandardOutput.Close();
            p.WaitForExit();
            p.Close();
            p.Dispose();
            return result;
        }
        catch (Exception e)
        {
            e.LogAndShowT(TAG, msg: $"{shellCallName} fail.");
        }
        return string.Empty;
    }
}
#endif