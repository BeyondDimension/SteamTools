namespace BD.WTTS.Client.Tools.Publish.Helpers;

static partial class ProcessHelper
{
    public static void StartAndWaitForExit(ProcessStartInfo psi, bool ignoreExitCode = false)
    {
        var process = Process.Start(psi);
        process.ThrowIsNull();
        process.WaitForExit();
        if (ignoreExitCode) return;
        var exitCode = process.ExitCode;
        if (exitCode != default)
        {
            var c = $"{psi.FileName} " +
                (string.IsNullOrWhiteSpace(psi.Arguments) ?
                    string.Join(" ", psi.ArgumentList) :
                    psi.Arguments);
            Exception[] innerExceptions =
                [
                    new Exception(c),
                    new ArgumentOutOfRangeException(nameof(exitCode), exitCode, null),
                ];
            throw new AggregateException(innerExceptions);
        }
    }
}
