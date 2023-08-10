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

    public static void StartProcessAndWaitForExit(string workingDirectory, string? arguments = null)
    {
        var psi = GetProcessStartInfo(workingDirectory);
        if (arguments != null) psi.Arguments = arguments;
        ProcessHelper.StartAndWaitForExit(psi);
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
