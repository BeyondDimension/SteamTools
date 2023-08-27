// ReSharper disable once CheckNamespace
namespace BD.WTTS;

partial class Startup // RunSelfProcess
{
    public static Process? RunSelfProcess(string arguments)
    {
        var psi = new ProcessStartInfo
        {
            Arguments = arguments,
#if LINUX
            FileName = Path.Combine(AppContext.BaseDirectory, "Steam++.sh"),
            UseShellExecute = true,
#else
            FileName = Environment.ProcessPath!,
            UseShellExecute = false,
#endif
        };
        var process = Process.Start(psi);
        return process;
    }
}