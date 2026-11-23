using BD.SteamClient.Models;
using BD.SteamClient.Services;
using BD.WTTS.Services;
using Moq;
using ReactiveUI.Reactive.Builder;
using System.Diagnostics;
using System.Reactive.Concurrency;
using System.Runtime.InteropServices;

namespace BD.WTTS.UnitTest;

[NonParallelizable]
public sealed class SteamIdleProcessTest
{
    [OneTimeSetUp]
    public void InitializeReactiveUI() => RxAppBuilder.CreateReactiveUIBuilder()
        .WithMainThreadScheduler(ImmediateScheduler.Instance).BuildApp();

    static SteamConnectService CreateService(Func<SteamApp, Process?> start) => new(
        Mock.Of<ISteamService>(), Mock.Of<ISteamworksLocalApiService>(), Mock.Of<ISteamworksWebApiService>(), start);

    [Test]
    public void AutomaticIdleDoesNotStartOtherTrackedGameIds()
    {
        var launched = new List<uint>();
        using var service = CreateService(app =>
        {
            launched.Add(app.AppId);
            return StartOwnedChild();
        });
        service.RuningSteamApps[1] = new SteamApp(1);
        var configured = new[] { new KeyValuePair<uint, string?>(480, "test game") };
        var first = service.StartConfiguredIdleApps(configured);
        var second = service.StartConfiguredIdleApps(configured);
        Assert.Multiple(() =>
        {
            Assert.That(first, Is.EqualTo(1));
            Assert.That(second, Is.Zero);
            Assert.That(launched, Is.EqualTo(new uint[] { 480 }));
            Assert.That(service.RuningSteamApps[1].Process, Is.Null);
            Assert.That(service.RuningSteamApps[480].Process!.HasExited, Is.False);
        });
    }

    [Test]
    public async Task ConcurrentStartsOfDifferentModelsShareOneOwnedChild()
    {
        var starts = 0;
        using var service = CreateService(_ =>
        {
            Interlocked.Increment(ref starts);
            return StartOwnedChild();
        });
        var requests = Enumerable.Range(0, 24).Select(_ => Task.Run(() => service.StartIdleApp(new SteamApp(480)))).ToArray();
        var results = await Task.WhenAll(requests).WaitAsync(TimeSpan.FromSeconds(15));
        var process = results[0].Process!;
        Assert.Multiple(() =>
        {
            Assert.That(starts, Is.EqualTo(1));
            Assert.That(results.All(app => ReferenceEquals(app, results[0])), Is.True);
            Assert.That(results.All(app => app.Process!.Id == process.Id), Is.True);
            Assert.That(process.HasExited, Is.False);
        });
        using var observer = Process.GetProcessById(process.Id);
        await Task.WhenAll(Enumerable.Range(0, 12).Select(_ => Task.Run(() => service.StopIdleApp(480))));
        Assert.Multiple(() =>
        {
            Assert.That(observer.WaitForExit(5000), Is.True);
            Assert.That(service.RuningSteamApps.ContainsKey(480), Is.False);
            Assert.That(results[0].Process, Is.Null);
        });
    }

    [Test]
    public void ExitedWorkerCanRestartAndToggleStopsOnlyThatGame()
    {
        var starts = 0;
        using var service = CreateService(_ => { starts++; return StartOwnedChild(); });
        var game = service.StartIdleApp(new SteamApp(480));
        var first = game.Process!;
        var firstPid = first.Id;
        first.StandardInput.WriteLine();
        first.StandardInput.Flush();
        Assert.That(first.WaitForExit(5000), Is.True);
        var restarted = service.StartIdleApp(new SteamApp(480));
        var replacement = restarted.Process!;
        using var observer = Process.GetProcessById(replacement.Id);
        Assert.Multiple(() =>
        {
            Assert.That(starts, Is.EqualTo(2));
            Assert.That(restarted, Is.SameAs(game));
            Assert.That(replacement.Id, Is.Not.EqualTo(firstPid));
            Assert.That(replacement.HasExited, Is.False);
        });
        service.ToggleIdleApp(new SteamApp(480));
        Assert.That(observer.WaitForExit(5000), Is.True);
        Assert.That(game.Process, Is.Null);
    }

    [Test]
    public void FailedLaunchDoesNotLeaveAnUnstartableTrackedEntry()
    {
        using var service = CreateService(_ => throw new InvalidOperationException("isolated launch failure"));
        Assert.Throws<InvalidOperationException>(() => service.StartIdleApp(new SteamApp(480)));
        Assert.That(service.RuningSteamApps.ContainsKey(480), Is.False);
    }

    [TestCase("0")]
    [TestCase("-1")]
    [TestCase("not-an-id")]
    public void InvalidIdleIdsExitWithoutStartingApplication(string value)
    {
        var handled = Startup.TryRunHeadlessIdle(["-clt", "app", "-silence", "-id", value], out var code);
        Assert.Multiple(() =>
        {
            Assert.That(handled, Is.True);
            Assert.That(code, Is.Not.Zero);
        });
    }

    [TestCase("-achievement")]
    [TestCase("-cloudmanager")]
    public void GuiGameModesRemainOnNormalApplicationPath(string mode)
        => Assert.That(Startup.TryRunHeadlessIdle(["-clt", "app", mode, "-id", "480"], out _), Is.False);

    [TestCase(Architecture.Arm64, true, true)]
    [TestCase(Architecture.Arm64, false, false)]
    [TestCase(Architecture.X64, true, true)]
    [TestCase(Architecture.X64, false, true)]
    public void NativeIdleWorkerSupportsAppleSilicon(Architecture architecture, bool isMacOS, bool expected)
        => Assert.That(HeadlessSteamIdleWorker.SupportsProcessArchitecture(architecture, isMacOS), Is.EqualTo(expected));

    [Test]
    public async Task ExistingPerAppWorkerIsRejectedBeforeNativeInitialization()
    {
        using var ready = new ManualResetEventSlim();
        using var release = new ManualResetEventSlim();
        var owner = Task.Run(() =>
        {
            using var mutex = new Mutex(false, "SteamTools.Idle.App.2000000001",
                new NamedWaitHandleOptions { CurrentUserOnly = true, CurrentSessionOnly = false });
            Assert.That(mutex.WaitOne(0), Is.True);
            try { ready.Set(); release.Wait(TimeSpan.FromSeconds(15)); }
            finally { mutex.ReleaseMutex(); }
        });
        try
        {
            Assert.That(ready.Wait(TimeSpan.FromSeconds(5)), Is.True);
            Assert.That(Startup.TryRunHeadlessIdle(["-clt", "app", "-silence", "-id", "2000000001"], out var code), Is.True);
            Assert.That(code, Is.Not.Zero);
        }
        finally { release.Set(); await owner; }
    }

    static Process StartOwnedChild()
    {
        var start = new ProcessStartInfo
        {
            UseShellExecute = false,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
        };
        if (OperatingSystem.IsWindows())
        {
            start.FileName = Path.Combine(Environment.SystemDirectory, "cmd.exe");
            start.ArgumentList.Add("/d");
            start.ArgumentList.Add("/q");
            start.ArgumentList.Add("/c");
            start.ArgumentList.Add("set /p finish=");
        }
        else
        {
            start.FileName = "/bin/sh";
            start.ArgumentList.Add("-c");
            start.ArgumentList.Add("read finish");
        }
        return Process.Start(start) ?? throw new InvalidOperationException("Owned shell did not start.");
    }
}
