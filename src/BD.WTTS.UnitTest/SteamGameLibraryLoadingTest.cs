using BD.SteamClient.Helpers;
using BD.SteamClient.Models;
using BD.SteamClient.Services;
using BD.WTTS.Services;
using DynamicData;
using Moq;
using ReactiveUI.Reactive.Builder;
using System.Reactive.Concurrency;

namespace BD.WTTS.UnitTest;

public sealed class SteamGameLibraryLoadingTest
{
    [OneTimeSetUp]
    public void InitializeReactiveUI() => RxAppBuilder.CreateReactiveUIBuilder()
        .WithMainThreadScheduler(ImmediateScheduler.Instance).BuildApp();

    [TestCase(false)]
    [TestCase(true)]
    public async Task StartingSteamWithoutAnAccountKeepsAndRefreshesCachedGames(bool emptyCache)
    {
        var steam = new Mock<ISteamService>();
        var local = new Mock<ISteamworksLocalApiService>();
        var web = new Mock<ISteamworksWebApiService>();
        var refreshStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var newBatch = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var processId = 0;
        steam.Setup(x => x.GetSteamProcessPid()).Returns(() => Volatile.Read(ref processId));
        steam.Setup(x => x.GetAppInfoEdits()).Returns(new Dictionary<uint, SteamAppEdit>());
        steam.Setup(x => x.GetDownloadingAppList()).Returns([]);
        steam.Setup(x => x.ReadAppInfosInBatchesAsync(It.IsAny<Action<IReadOnlyList<SteamApp>>>(), It.IsAny<CancellationToken>()))
            .Returns<Action<IReadOnlyList<SteamApp>>, CancellationToken>(async (publish, token) =>
            {
                if (Volatile.Read(ref processId) == 0)
                {
                    publish([new SteamApp(10) { Name = "Cached game" }]);
                    return;
                }

                refreshStarted.TrySetResult();
                await newBatch.Task.WaitAsync(token);
                if (!emptyCache)
                    publish([new SteamApp(20) { Name = "Refreshed cached game" }]);
            });
        local.Setup(x => x.Initialize()).Returns(false);
        using var service = new SteamConnectService(steam.Object, local.Object, web.Object);
        await service.RefreshGamesListAsync(showToast: false);

        var observedProcessId = 0;
        Volatile.Write(ref processId, 100);
        Assert.That(service.TryInitializeSteamConnection(ref observedProcessId, out _), Is.False);
        Assert.That(service.SteamApps.Items.Select(app => app.AppId), Is.EqualTo(new uint[] { 10 }));
        await refreshStarted.Task.WaitAsync(TimeSpan.FromSeconds(10));
        newBatch.SetResult();
        Assert.That(() => service.IsLoadingGameList, Is.False.After(2000, 20));
        Assert.That(service.SteamApps.Items.Select(app => app.AppId), Is.EqualTo(new uint[] { emptyCache ? 10u : 20u }));
    }

    [Test]
    public async Task SwitchingAccountCancelsOldLoadAndShowsNewBatches()
    {
        var steam = new Mock<ISteamService>();
        var local = new Mock<ISteamworksLocalApiService>();
        var web = new Mock<ISteamworksWebApiService>();
        var firstBatch = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var blocked = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var processId = 100;
        long steamId = 76561198000000001;
        var readCount = 0;

        steam.SetupGet(x => x.IsRunningSteamProcess).Returns(true);
        steam.Setup(x => x.GetSteamProcessPid()).Returns(() => Volatile.Read(ref processId));
        steam.Setup(x => x.GetAppInfoEdits()).Returns(new Dictionary<uint, SteamAppEdit>());
        steam.Setup(x => x.GetDownloadingAppList()).Returns(new List<SteamApp>());
        steam.Setup(x => x.ReadAppInfosInBatchesAsync(It.IsAny<Action<IReadOnlyList<SteamApp>>>(), It.IsAny<CancellationToken>()))
            .Returns<Action<IReadOnlyList<SteamApp>>, CancellationToken>(async (publish, token) =>
            {
                if (Interlocked.Increment(ref readCount) == 1)
                {
                    publish([new SteamApp(10) { Name = "First account" }]);
                    firstBatch.TrySetResult();
                    await blocked.Task.WaitAsync(token);
                    publish([new SteamApp(11) { Name = "Stale result" }]);
                }
                else
                {
                    publish([new SteamApp(20) { Name = "Second account" }]);
                }
            });
        local.Setup(x => x.Initialize()).Returns(true);
        local.Setup(x => x.GetSteamId64()).Returns(() => Volatile.Read(ref steamId));
        local.Setup(x => x.OwnsApps(It.IsAny<IEnumerable<SteamApp>>())).Returns<IEnumerable<SteamApp>>(apps => apps);
        using var service = new SteamConnectService(steam.Object, local.Object, web.Object) { IsDisposedClient = false };

        var oldRefresh = service.RefreshGamesListAsync(showToast: false);
        await firstBatch.Task.WaitAsync(TimeSpan.FromSeconds(10));
        Assert.Multiple(() =>
        {
            Assert.That(service.IsLoadingGameList, Is.True);
            Assert.That(service.SteamApps.Items.Select(x => x.AppId), Is.EquivalentTo(new uint[] { 10 }));
            Assert.That(oldRefresh.IsCompleted, Is.False);
        });

        Volatile.Write(ref processId, 200);
        Volatile.Write(ref steamId, 76561198000000002);
        var newRefresh = service.RefreshGamesListAsync(showToast: false);
        await Task.WhenAll(oldRefresh, newRefresh).WaitAsync(TimeSpan.FromSeconds(10));
        Assert.Multiple(() =>
        {
            Assert.That(service.SteamApps.Items.Select(x => x.AppId), Is.EquivalentTo(new uint[] { 20 }));
            Assert.That(service.IsLoadingGameList, Is.False);
            Assert.That(service.IsDisposedClient, Is.True);
        });
        local.Verify(x => x.Initialize(), Times.Exactly(2));
    }

    [Test]
    public async Task RefreshUsesCurrentAccountAndOfflineCache()
    {
        var steam = new Mock<ISteamService>();
        var local = new Mock<ISteamworksLocalApiService>();
        var web = new Mock<ISteamworksWebApiService>();
        var processId = 200;
        long steamId = 76561198000000002;
        var readCount = 0;
        steam.SetupGet(x => x.IsRunningSteamProcess).Returns(() => processId != 0);
        steam.Setup(x => x.GetSteamProcessPid()).Returns(() => processId);
        steam.Setup(x => x.GetAppInfoEdits()).Returns(new Dictionary<uint, SteamAppEdit>());
        steam.Setup(x => x.GetDownloadingAppList()).Returns(new List<SteamApp>());
        steam.Setup(x => x.ReadAppInfosInBatchesAsync(It.IsAny<Action<IReadOnlyList<SteamApp>>>(), It.IsAny<CancellationToken>()))
            .Returns<Action<IReadOnlyList<SteamApp>>, CancellationToken>((publish, _) =>
            {
                if (++readCount == 1) publish([new SteamApp(20) { Name = "Account B" }]);
                else if (readCount == 2) publish([new SteamApp(30) { Name = "Account C" }]);
                else if (readCount == 3)
                {
                    // The Steam parser reuses and clears its batch after the callback returns.
                    var batch = new List<SteamApp> { new(40) { Name = "Offline" } };
                    publish(batch);
                    batch.Clear();
                }
                else if (readCount == 6)
                    publish([new SteamApp(50) { Name = "Recovered offline cache" }]);
                return Task.CompletedTask;
            });
        local.Setup(x => x.Initialize()).Returns(true);
        local.Setup(x => x.GetSteamId64()).Returns(() => steamId);
        local.Setup(x => x.OwnsApps(It.IsAny<IEnumerable<SteamApp>>())).Returns<IEnumerable<SteamApp>>(apps => apps);
        using var service = new SteamConnectService(steam.Object, local.Object, web.Object);

        await service.RefreshGamesListAsync(showToast: false);
        service.CurrentSteamUser = new SteamUser { SteamId64 = steamId };
        service.IsConnectToSteam = true;
        steamId = 76561198000000003;
        await service.RefreshGamesListAsync(showToast: false);
        Assert.Multiple(() =>
        {
            Assert.That(service.SteamApps.Items.Select(x => x.AppId), Is.EquivalentTo(new uint[] { 30 }));
            Assert.That(service.CurrentSteamUser, Is.Null);
            Assert.That(service.IsConnectToSteam, Is.False);
        });

        processId = 0;
        await service.RefreshGamesListAsync(showToast: false);
        Assert.That(service.SteamApps.Items.Select(x => x.AppId), Is.EquivalentTo(new uint[] { 40 }));
        for (var attempt = 0; attempt < 2; attempt++)
        {
            await service.RefreshGamesListAsync(showToast: false);
            Assert.Multiple(() =>
            {
                Assert.That(service.SteamApps.Items.Select(x => x.AppId), Is.EqualTo(new uint[] { 40 }));
                Assert.That(service.IsLoadingGameList, Is.False);
            });
        }

        await service.RefreshGamesListAsync(showToast: false);
        Assert.That(service.SteamApps.Items.Select(x => x.AppId), Is.EqualTo(new uint[] { 50 }));
        local.Verify(x => x.Initialize(), Times.Exactly(2));
        local.Verify(x => x.OwnsApps(It.IsAny<IEnumerable<SteamApp>>()), Times.Exactly(2));
    }

    [Test]
    public async Task ExitingSteamDuringInventoryLoadReloadsFromCache()
    {
        var steam = new Mock<ISteamService>();
        var local = new Mock<ISteamworksLocalApiService>();
        var web = new Mock<ISteamworksWebApiService>();
        var accountReadStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var processId = 100;
        steam.SetupGet(x => x.IsRunningSteamProcess).Returns(() => Volatile.Read(ref processId) != 0);
        steam.Setup(x => x.GetSteamProcessPid()).Returns(() => Volatile.Read(ref processId));
        steam.Setup(x => x.GetAppInfoEdits()).Returns(new Dictionary<uint, SteamAppEdit>());
        steam.Setup(x => x.GetDownloadingAppList()).Returns(new List<SteamApp>());
        steam.Setup(x => x.ReadAppInfosInBatchesAsync(It.IsAny<Action<IReadOnlyList<SteamApp>>>(), It.IsAny<CancellationToken>()))
            .Returns<Action<IReadOnlyList<SteamApp>>, CancellationToken>(async (publish, token) =>
            {
                if (Volatile.Read(ref processId) != 0)
                {
                    accountReadStarted.TrySetResult();
                    await Task.Delay(Timeout.Infinite, token);
                }
                else
                {
                    publish([new SteamApp(20) { Name = "Cached game" }]);
                }
            });
        local.Setup(x => x.Initialize()).Returns(true);
        local.Setup(x => x.GetSteamId64()).Returns(76561198000000001);
        using var service = new SteamConnectService(steam.Object, local.Object, web.Object);

        var refresh = service.RefreshGamesListAsync(showToast: false);
        await accountReadStarted.Task.WaitAsync(TimeSpan.FromSeconds(10));
        var observedProcessId = 100;
        Volatile.Write(ref processId, 0);
        Assert.That(service.TryInitializeSteamConnection(ref observedProcessId, out _), Is.False);
        await refresh.WaitAsync(TimeSpan.FromSeconds(10));

        Assert.That(() => service.SteamApps.Items.Select(app => app.AppId).ToArray(), Is.EqualTo(new uint[] { 20 }).After(2000, 20));
        local.Verify(x => x.Initialize(), Times.Once);
        local.Verify(x => x.OwnsApps(It.IsAny<IEnumerable<SteamApp>>()), Times.Never);
    }

    [TestCase(false, 0L)]
    [TestCase(true, 0L)]
    [TestCase(true, SteamIdConvert.UndefinedId)]
    public async Task UnavailableSteamSessionLoadsCachedGames(bool initialized, long steamId)
    {
        var steam = new Mock<ISteamService>();
        var local = new Mock<ISteamworksLocalApiService>();
        var web = new Mock<ISteamworksWebApiService>();
        steam.SetupGet(x => x.IsRunningSteamProcess).Returns(true);
        steam.Setup(x => x.GetSteamProcessPid()).Returns(100);
        steam.Setup(x => x.GetAppInfoEdits()).Returns(new Dictionary<uint, SteamAppEdit>());
        steam.Setup(x => x.GetDownloadingAppList()).Returns(new List<SteamApp>());
        steam.Setup(x => x.ReadAppInfosInBatchesAsync(It.IsAny<Action<IReadOnlyList<SteamApp>>>(), It.IsAny<CancellationToken>()))
            .Returns<Action<IReadOnlyList<SteamApp>>, CancellationToken>((publish, _) =>
            {
                publish([new SteamApp(20) { Name = "Cached game" }]);
                return Task.CompletedTask;
            });
        local.Setup(x => x.Initialize()).Returns(initialized);
        local.Setup(x => x.GetSteamId64()).Returns(steamId);
        using var service = new SteamConnectService(steam.Object, local.Object, web.Object)
        {
            CurrentSteamUser = new SteamUser { SteamId64 = 76561198000000001 },
            IsConnectToSteam = true,
        };

        await service.RefreshGamesListAsync(showToast: false);

        Assert.Multiple(() =>
        {
            Assert.That(service.SteamApps.Items.Select(app => app.AppId), Is.EqualTo(new uint[] { 20 }));
            Assert.That(service.CurrentSteamUser, Is.Null);
            Assert.That(service.IsConnectToSteam, Is.False);
            Assert.That(service.IsDisposedClient, Is.True);
        });
        local.Verify(x => x.DisposeSteamClient(), Times.Once);
        local.Verify(x => x.OwnsApps(It.IsAny<IEnumerable<SteamApp>>()), Times.Never);
    }

    [Test]
    public async Task ChangingSteamPathCancelsOldInventoryAndRefreshesNewFiles()
    {
        var steam = new Mock<ISteamService>();
        var local = new Mock<ISteamworksLocalApiService>();
        var web = new Mock<ISteamworksWebApiService>();
        var firstBatch = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var newBatch = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var blocked = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var readCount = 0;
        steam.SetupGet(x => x.IsRunningSteamProcess).Returns(false);
        steam.Setup(x => x.GetSteamProcessPid()).Returns(0);
        steam.Setup(x => x.GetAppInfoEdits()).Returns(new Dictionary<uint, SteamAppEdit>());
        steam.Setup(x => x.GetDownloadingAppList()).Returns(new List<SteamApp>());
        steam.Setup(x => x.ReadAppInfosInBatchesAsync(It.IsAny<Action<IReadOnlyList<SteamApp>>>(), It.IsAny<CancellationToken>()))
            .Returns<Action<IReadOnlyList<SteamApp>>, CancellationToken>(async (publish, token) =>
            {
                if (Interlocked.Increment(ref readCount) == 1)
                {
                    publish([new SteamApp(10) { Name = "Old path" }]);
                    firstBatch.TrySetResult();
                    await blocked.Task.WaitAsync(token);
                    publish([new SteamApp(11) { Name = "Stale path" }]);
                }
                else
                {
                    publish([new SteamApp(20) { Name = "New path" }]);
                    newBatch.TrySetResult();
                }
            });
        using var service = new SteamConnectService(steam.Object, local.Object, web.Object);
        // Linux starts the platform password prompt when enabling the download watcher.
        var testWatcher = !OperatingSystem.IsLinux();
        if (testWatcher) service.IsWatchSteamDownloading = true;
        service.WatchDownloadingSteamAppIds.Add(10);
        service.SteamUsers.AddOrUpdate(new SteamUser { SteamId64 = 76561198000000001 });
        service.CurrentSteamUser = new SteamUser { SteamId64 = 76561198000000001 };
        service.IsConnectToSteam = true;

        var oldRefresh = service.RefreshGamesListAsync(showToast: false);
        await firstBatch.Task.WaitAsync(TimeSpan.FromSeconds(10));
        service.OnSteamProgramPathChanged();
        await Task.WhenAll(oldRefresh, newBatch.Task).WaitAsync(TimeSpan.FromSeconds(10));
        Assert.That(SpinWait.SpinUntil(() => !service.IsLoadingGameList, TimeSpan.FromSeconds(10)), Is.True);
        Assert.Multiple(() =>
        {
            Assert.That(service.SteamApps.Items.Select(x => x.AppId), Is.EquivalentTo(new uint[] { 20 }));
            Assert.That(service.SteamUsers.Count, Is.Zero);
            Assert.That(service.CurrentSteamUser, Is.Null);
            Assert.That(service.WatchDownloadingSteamAppIds, Is.Empty);
            Assert.That(service.IsConnectToSteam, Is.False);
        });
        if (testWatcher)
        {
            steam.Verify(x => x.StopWatchSteamDownloading(), Times.Once);
            steam.Verify(x => x.StartWatchSteamDownloading(It.IsAny<Action<SteamApp>>(), It.IsAny<Action<uint>>()), Times.Exactly(2));
        }
    }
}
