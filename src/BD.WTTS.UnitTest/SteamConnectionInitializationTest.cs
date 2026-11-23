using BD.SteamClient.Models;
using BD.SteamClient.Services;
using BD.WTTS.Services;
using Moq;
using ReactiveUI.Reactive.Builder;
using System.ComponentModel;
using System.Reactive.Concurrency;

namespace BD.WTTS.UnitTest;

[NonParallelizable]
public sealed class SteamConnectionInitializationTest
{
    const long SteamId = 76561198000000001;

    [OneTimeSetUp]
    public void InitializeReactiveUI() => RxAppBuilder.CreateReactiveUIBuilder()
        .WithMainThreadScheduler(ImmediateScheduler.Instance).BuildApp();

    [TestCase("access")]
    [TestCase("exited")]
    [TestCase("unavailable")]
#if WINDOWS
    [TestCase("legacy-wmi")]
#endif
    public void OptionalLauncherFailureCompletesConnectionOnlyOnce(string failure)
    {
        var steam = CreateSteam();
        var local = CreateLocal();
        var web = CreateWeb();
        Exception error = failure switch
        {
            "access" => new Win32Exception(5),
            "exited" => new InvalidOperationException("Owned process exited."),
#if WINDOWS
            "legacy-wmi" => new System.Management.ManagementException("Invalid class"),
#endif
            _ => new EntryPointNotFoundException("Optional process metadata is unavailable."),
        };
        steam.Setup(x => x.IsSteamChinaLauncher()).Throws(error);
        using var service = new SteamConnectService(steam.Object, local.Object, web.Object);
        var observedProcessId = 100;

        Assert.That(service.TryInitializeSteamConnection(ref observedProcessId, out var connection), Is.True);
        Assert.Multiple(() =>
        {
            Assert.That(service.IsConnectToSteam, Is.True);
            Assert.That(service.IsSteamChinaLauncher, Is.False);
            Assert.That(service.IsDisposedClient, Is.True);
            Assert.That(service.CurrentSteamUser, Is.SameAs(connection.User));
            Assert.That(connection.User.SteamId64, Is.EqualTo(SteamId));
            Assert.That(connection.Country, Is.EqualTo("CN"));
            Assert.That(connection.User.IPCountry, Is.EqualTo("CN"));
        });
        for (var poll = 0; poll < 3; poll++)
            Assert.That(service.TryInitializeSteamConnection(ref observedProcessId, out _), Is.False,
                "The monitor must not repeat launch notifications or automatic idle for optional metadata failures.");

        local.Verify(x => x.Initialize(), Times.Once);
        local.Verify(x => x.DisposeSteamClient(), Times.Once);
        steam.Verify(x => x.IsSteamChinaLauncher(), Times.Once);
        web.Verify(x => x.GetUserInfo(SteamId), Times.Once);
    }

    [TestCase(false)]
    [TestCase(true)]
    public void SuccessfulClassificationKeepsItsBooleanMeaning(bool chinaLauncher)
    {
        var steam = CreateSteam();
        steam.Setup(x => x.IsSteamChinaLauncher()).Returns(chinaLauncher);
        using var service = new SteamConnectService(steam.Object, CreateLocal().Object, CreateWeb().Object);
        var observedProcessId = 100;

        Assert.That(service.TryInitializeSteamConnection(ref observedProcessId, out _), Is.True);
        Assert.That(service.IsSteamChinaLauncher, Is.EqualTo(chinaLauncher));
    }

    [Test]
    public void NativeInitializationFailureIsNotConvertedIntoAConnectedSession()
    {
        var steam = CreateSteam();
        var local = CreateLocal();
        local.Setup(x => x.Initialize()).Returns(false);
        using var service = new SteamConnectService(steam.Object, local.Object, CreateWeb().Object);
        var observedProcessId = 100;

        Assert.That(service.TryInitializeSteamConnection(ref observedProcessId, out _), Is.False);
        Assert.Multiple(() =>
        {
            Assert.That(service.IsConnectToSteam, Is.False);
            Assert.That(service.CurrentSteamUser, Is.Null);
        });
        local.Verify(x => x.DisposeSteamClient(), Times.Once);
        steam.Verify(x => x.IsSteamChinaLauncher(), Times.Never);
    }

    [Test]
    public void NativeSessionFailurePropagatesAndReleasesTheGateForRecovery()
    {
        var steam = CreateSteam();
        var local = CreateLocal();
        var error = new InvalidOperationException("Native client cannot read the account.");
        local.Setup(x => x.GetSteamId64()).Throws(error);
        using var service = new SteamConnectService(steam.Object, local.Object, CreateWeb().Object);
        var observedProcessId = 100;

        Assert.That(Assert.Throws<InvalidOperationException>(() =>
            service.TryInitializeSteamConnection(ref observedProcessId, out _)), Is.SameAs(error));
        Assert.That(service.IsConnectToSteam, Is.False);
        steam.Verify(x => x.IsSteamChinaLauncher(), Times.Never);

        local.Setup(x => x.GetSteamId64()).Returns(SteamId);
        Assert.That(service.TryInitializeSteamConnection(ref observedProcessId, out _), Is.True);
        local.Verify(x => x.DisposeSteamClient(), Times.Exactly(2));
    }

    [Test]
    public void ExitingAndRestartingSteamClassifiesTheNewSessionOnce()
    {
        var processId = 100;
        var steam = CreateSteam();
        steam.Setup(x => x.GetSteamProcessPid()).Returns(() => processId);
        steam.Setup(x => x.IsSteamChinaLauncher()).Returns(true);
        var local = CreateLocal();
        using var service = new SteamConnectService(steam.Object, local.Object, CreateWeb().Object);
        var observedProcessId = processId;
        Assert.That(service.TryInitializeSteamConnection(ref observedProcessId, out _), Is.True);

        processId = 0;
        Assert.That(service.TryInitializeSteamConnection(ref observedProcessId, out _), Is.False);
        Assert.Multiple(() =>
        {
            Assert.That(service.IsConnectToSteam, Is.False);
            Assert.That(service.IsRunningSteamProcess, Is.False);
            Assert.That(service.IsSteamChinaLauncher, Is.False);
            Assert.That(service.CurrentSteamUser, Is.Null);
        });

        processId = 200;
        steam.Setup(x => x.IsSteamChinaLauncher()).Throws(new Win32Exception(5));
        Assert.That(service.TryInitializeSteamConnection(ref observedProcessId, out _), Is.True);
        Assert.That(service.TryInitializeSteamConnection(ref observedProcessId, out _), Is.False);
        Assert.That(service.IsSteamChinaLauncher, Is.False, "Do not retain the previous process's region.");
        local.Verify(x => x.Initialize(), Times.Exactly(2));
        steam.Verify(x => x.IsSteamChinaLauncher(), Times.Exactly(2));
    }

    [Test]
    public void RestartDuringOptionalClassificationDoesNotPublishTheOldAccount()
    {
        var processId = 100;
        var steam = CreateSteam();
        steam.Setup(x => x.GetSteamProcessPid()).Returns(() => processId);
        steam.Setup(x => x.IsSteamChinaLauncher()).Returns(() =>
        {
            processId = 200;
            return true;
        });
        using var service = new SteamConnectService(steam.Object, CreateLocal().Object, CreateWeb().Object);
        var observedProcessId = processId;

        Assert.That(service.TryInitializeSteamConnection(ref observedProcessId, out _), Is.False);
        Assert.That(service.IsConnectToSteam, Is.False);
        Assert.That(service.CurrentSteamUser, Is.Null);

        steam.Setup(x => x.IsSteamChinaLauncher()).Returns(false);
        Assert.That(service.TryInitializeSteamConnection(ref observedProcessId, out _), Is.True);
    }

    [Test]
    public async Task LibraryDetectedAccountChangeStillInitializesTheReplacementAccount()
    {
        var steamId = SteamId;
        var steam = CreateSteam();
        var local = CreateLocal();
        local.Setup(x => x.GetSteamId64()).Returns(() => steamId);
        local.Setup(x => x.OwnsApps(It.IsAny<IEnumerable<SteamApp>>()))
            .Returns<IEnumerable<SteamApp>>(apps => apps);
        steam.Setup(x => x.GetAppInfoEdits()).Returns(new Dictionary<uint, SteamAppEdit>());
        steam.Setup(x => x.GetDownloadingAppList()).Returns(new List<SteamApp>());
        steam.Setup(x => x.ReadAppInfosInBatchesAsync(It.IsAny<Action<IReadOnlyList<SteamApp>>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        using var service = new SteamConnectService(steam.Object, local.Object, CreateWeb().Object);
        var observedProcessId = 100;
        Assert.That(service.TryInitializeSteamConnection(ref observedProcessId, out _), Is.True);

        steamId++;
        await service.RefreshGamesListAsync(showToast: false);
        Assert.That(service.IsConnectToSteam, Is.False);
        Assert.That(service.TryInitializeSteamConnection(ref observedProcessId, out var connection), Is.True);
        Assert.That(connection.User.SteamId64, Is.EqualTo(steamId));
        Assert.That(service.TryInitializeSteamConnection(ref observedProcessId, out _), Is.False);
        steam.Verify(x => x.IsSteamChinaLauncher(), Times.Exactly(2));
    }

    [Test]
    public async Task ConcurrentConnectionPollsShareOneNativeInitializationAndLaunchSignal()
    {
        var steam = CreateSteam();
        var local = CreateLocal();
        var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        using var release = new ManualResetEventSlim();
        local.Setup(x => x.Initialize()).Returns(() =>
        {
            entered.TrySetResult();
            if (!release.Wait(TimeSpan.FromSeconds(10)))
                throw new TimeoutException("Concurrent initialization was not released.");
            return true;
        });
        using var service = new SteamConnectService(steam.Object, local.Object, CreateWeb().Object);
        var first = Task.Run(() =>
        {
            var observedProcessId = 100;
            return service.TryInitializeSteamConnection(ref observedProcessId, out _);
        });
        try
        {
            await entered.Task.WaitAsync(TimeSpan.FromSeconds(10));
            var observedProcessId = 100;
            Assert.That(service.TryInitializeSteamConnection(ref observedProcessId, out _), Is.False);
        }
        finally
        {
            release.Set();
        }

        Assert.That(await first.WaitAsync(TimeSpan.FromSeconds(10)), Is.True);
        var observed = 100;
        Assert.That(service.TryInitializeSteamConnection(ref observed, out _), Is.False);
        local.Verify(x => x.Initialize(), Times.Once);
        steam.Verify(x => x.IsSteamChinaLauncher(), Times.Once);
    }

    static Mock<ISteamService> CreateSteam()
    {
        var steam = new Mock<ISteamService>();
        steam.SetupGet(x => x.IsRunningSteamProcess).Returns(true);
        steam.Setup(x => x.GetSteamProcessPid()).Returns(100);
        return steam;
    }

    static Mock<ISteamworksLocalApiService> CreateLocal()
    {
        var local = new Mock<ISteamworksLocalApiService>();
        local.Setup(x => x.Initialize()).Returns(true);
        local.Setup(x => x.GetSteamId64()).Returns(SteamId);
        local.Setup(x => x.GetIPCountry()).Returns("CN");
        local.Setup(x => x.GetServerRealTime()).Returns(1700000000U);
        return local;
    }

    static Mock<ISteamworksWebApiService> CreateWeb()
    {
        var web = new Mock<ISteamworksWebApiService>();
        web.Setup(x => x.GetUserInfo(It.IsAny<long>())).ReturnsAsync((long id) => new SteamUser { SteamId64 = id });
        return web;
    }
}
