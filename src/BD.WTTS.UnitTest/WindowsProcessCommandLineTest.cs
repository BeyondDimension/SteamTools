#if WINDOWS
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace BD.WTTS.UnitTest;

public sealed class WindowsProcessCommandLineTest
{
    [Test]
    public void CurrentProcessCommandLineMatchesTheOperatingSystemValue()
        => Assert.That(WindowsProcessCommandLine.Read(Environment.ProcessId), Is.EqualTo(Marshal.PtrToStringUni(GetCommandLine())));

    [TestCase(16)]
    [TestCase(2048)]
    public async Task OwnedProcessCommandLineSupportsStackAndAllocatedBuffers(int markerLength)
    {
        var marker = "wtts-native-command-" + new string('x', markerLength);
        using var process = StartOwnedChild(marker);
        try
        {
            var commandLine = WindowsProcessCommandLine.Read(process.Id);
            Assert.Multiple(() =>
            {
                Assert.That(commandLine, Does.Contain(marker));
                Assert.That(commandLine, Does.Contain("cmd.exe").IgnoreCase);
            });
            await process.StandardInput.WriteLineAsync("finish");
            await process.StandardInput.FlushAsync();
            await process.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(10));
        }
        finally
        {
            if (!process.HasExited)
            {
                process.Kill();
                await process.WaitForExitAsync();
            }
        }
    }

    [Test]
    public void InvalidProcessPreservesTheNativeFailure()
    {
        var error = Assert.Throws<Win32Exception>(() => WindowsProcessCommandLine.Read(0));
        Assert.That(error!.NativeErrorCode, Is.EqualTo(87));
    }

    [Test]
    public async Task ExitedOwnedProcessDoesNotReturnInventedCommandLineMetadata()
    {
        var process = StartOwnedChild("wtts-exited-command");
        var processId = process.Id;
        try
        {
            await process.StandardInput.WriteLineAsync("finish");
            await process.StandardInput.FlushAsync();
            await process.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(10));
            Assert.Throws<Win32Exception>(() => WindowsProcessCommandLine.Read(processId));
        }
        finally
        {
            if (!process.HasExited)
            {
                process.Kill();
                await process.WaitForExitAsync();
            }
            process.Dispose();
        }
    }

    static Process StartOwnedChild(string marker)
    {
        var start = new ProcessStartInfo(Path.Combine(Environment.SystemDirectory, "cmd.exe"))
        {
            UseShellExecute = false,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
        };
        start.ArgumentList.Add("/d");
        start.ArgumentList.Add("/q");
        start.ArgumentList.Add("/c");
        start.ArgumentList.Add("set /p finish=" + marker);
        return Process.Start(start) ?? throw new InvalidOperationException("Owned command-query child did not start.");
    }

    [DllImport("kernel32.dll", EntryPoint = "GetCommandLineW")]
    static extern nint GetCommandLine();
}
#endif
