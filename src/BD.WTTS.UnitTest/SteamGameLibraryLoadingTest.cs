using BD.SteamClient.Models;
using BD.SteamClient.Services;
using BD.WTTS.Services;
using DynamicData;
using Moq;
using ReactiveUI.Builder;
using System.Reactive.Concurrency;

namespace BD.WTTS.UnitTest;

public sealed class SteamGameLibraryLoadingTest
{
    [OneTimeSetUp]
    public void InitializeReactiveUI() => RxAppBuilder.CreateReactiveUIBuilder()
        .WithMainThreadScheduler(ImmediateScheduler.Instance).BuildApp();

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
        steam.Setup(x => x.GetModifiedApps()).Returns((List<ModifiedApp>?)null);
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
        steam.Setup(x => x.GetModifiedApps()).Returns((List<ModifiedApp>?)null);
        steam.Setup(x => x.GetDownloadingAppList()).Returns(new List<SteamApp>());
        steam.Setup(x => x.ReadAppInfosInBatchesAsync(It.IsAny<Action<IReadOnlyList<SteamApp>>>(), It.IsAny<CancellationToken>()))
            .Returns<Action<IReadOnlyList<SteamApp>>, CancellationToken>((publish, _) =>
            {
                if (++readCount == 1) publish([new SteamApp(20) { Name = "Account B" }]);
                else if (readCount == 2) publish([new SteamApp(30) { Name = "Account C" }]);
                else if (readCount == 3) publish([new SteamApp(40) { Name = "Offline" }]);
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
        await service.RefreshGamesListAsync(showToast: false);
        Assert.Multiple(() =>
        {
            Assert.That(service.SteamApps.Count, Is.Zero);
            Assert.That(service.IsLoadingGameList, Is.False);
        });
        local.Verify(x => x.OwnsApps(It.IsAny<IEnumerable<SteamApp>>()), Times.Exactly(2));
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
        steam.Setup(x => x.GetModifiedApps()).Returns((List<ModifiedApp>?)null);
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
        service.IsWatchSteamDownloading = true;
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
        steam.Verify(x => x.StopWatchSteamDownloading(), Times.Once);
        steam.Verify(x => x.StartWatchSteamDownloading(It.IsAny<Action<SteamApp>>(), It.IsAny<Action<uint>>()), Times.Exactly(2));
    }
}
