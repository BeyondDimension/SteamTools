#if MACOS || MACCATALYST || IOS
// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

partial class MacCatalystPlatformServiceImpl
{
    async void IPlatformService.SystemShutdown(int waitSecond)
    {
        await Task.Delay(waitSecond);
        RunOsaScript(nameof(IPlatformService.SystemSleep),
            "tell application \"Finder\" to shut down");
    }

    async void IPlatformService.SystemSleep(int waitSecond)
    {
        await Task.Delay(waitSecond);
        RunOsaScript(nameof(IPlatformService.SystemSleep),
            "tell application \"Finder\" to sleep");
    }

    static string RunOsaScript(string shellCallName, string shell)
    {
        try
        {
            using var p = new Process();
            p.StartInfo.FileName = "osascript";
            p.StartInfo.Arguments = $" -e '{shell}";
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;
            p.Start();
            string result = p.StandardOutput.ReadToEnd();
            p.WaitForExit();
            p.Close();
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