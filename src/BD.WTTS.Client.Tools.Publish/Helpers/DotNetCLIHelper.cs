namespace BD.WTTS.Client.Tools.Publish.Helpers;

static partial class DotNetCLIHelper
{
    const string dotnet = "dotnet";

    public static void CleanProjDir(string workingDirectory)
    {
        // The previous restore may have targeted a different architecture.
        // Clear the generated directories before restoring for the new publish.
        foreach (var directory in new[] { "bin", "obj" })
        {
            var path = Path.Combine(workingDirectory, directory);
            if (Directory.Exists(path))
                Directory.Delete(path, recursive: true);
        }
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
