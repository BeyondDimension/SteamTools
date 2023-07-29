namespace BD.WTTS.Client.Tools.Publish.Helpers;

static partial class DotNetCLIHelper
{
    const string dotnet = "dotnet";

    public static void CleanProjDir(string workingDirectory)
    {
        StartProcessAndWaitForExit(workingDirectory, "clean");

        var binPath = Path.Combine(workingDirectory, "bin");
        IOPath.DirTryDelete(binPath);

        var objPath = Path.Combine(workingDirectory, "obj");
        IOPath.DirTryDelete(objPath);
    }

    public static void StartProcessAndWaitForExit(ProcessStartInfo psi)
    {
        var process = Process.Start(psi);
        process.ThrowIsNull();
        process.WaitForExit();
        var exitCode = process.ExitCode;
        if (exitCode != default)
        {
            throw new ArgumentOutOfRangeException(nameof(exitCode), exitCode, null);
        }
    }

    public static void StartProcessAndWaitForExit(string workingDirectory, string? arguments = null)
    {
        var psi = GetProcessStartInfo(workingDirectory);
        if (arguments != null) psi.Arguments = arguments;
        StartProcessAndWaitForExit(psi);
    }

    public static ProcessStartInfo GetProcessStartInfo(string workingDirectory)
    {
        ProcessStartInfo psi = new()
        {
            FileName = dotnet,
            WorkingDirectory = workingDirectory,
#if !NETCOREAPP
            UseShellExecute = false,
#endif
        };
        return psi;
    }
}
