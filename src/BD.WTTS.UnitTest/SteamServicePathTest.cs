using System.IO.Hashing;
using System.Text;
using BD.SteamClient.Models;
using BD.SteamClient.Services;
using BD.SteamClient.Services.Implementation;
using BD.SteamClient.Services.Mvvm;
using BD.WTTS.Services;
using BD.WTTS.Services.Implementation;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using ReactiveUI.Reactive.Builder;
using System.Reactive.Concurrency;

namespace BD.WTTS.UnitTest;

public sealed class SteamServicePathTest
{
    string directory = null!;
    string first = null!;
    string second = null!;

    [SetUp]
    public void SetUp()
    {
        directory = Path.Combine(Path.GetTempPath(), "WttsSteamPathTest-" + Guid.NewGuid());
        first = Path.Combine(directory, "first");
        second = Path.Combine(directory, "second");
        CreateSteam(first, "first-account", 1001, 129);
        CreateSteam(second, "second-account", 2001, 1);
    }

    [TearDown]
    public void TearDown() => Directory.Delete(directory, recursive: true);

    [Test]
    public async Task ConfiguredProgramChangesExecutableDirectoryAndSteamFilesWithoutRecreatingService()
    {
        var service = new TestSteamService();
        var automaticProgram = service.SteamProgramPath;
        var automaticDirectory = service.SteamDirPath;

        service.ConfiguredPath = Path.Combine(first, "steam.exe");
        Assert.That(service.SteamProgramPath, Is.EqualTo(service.ConfiguredPath));
        Assert.That(service.SteamDirPath, Is.EqualTo(first));
        Assert.That(service.GetRememberUserList().Select(user => user.AccountName), Is.EqualTo(new[] { "first-account" }));
        Assert.That((await service.GetAppInfos()).Select(app => app.AppId), Is.EqualTo(Enumerable.Range(1001, 129).Select(id => (uint)id)));

        service.ConfiguredPath = Path.Combine(second, "steam.exe");
        Assert.That(service.SteamProgramPath, Is.EqualTo(service.ConfiguredPath));
        Assert.That(service.SteamDirPath, Is.EqualTo(second));
        Assert.That(service.GetRememberUserList().Select(user => user.AccountName), Is.EqualTo(new[] { "second-account" }));
        Assert.That((await service.GetAppInfos()).Select(app => app.AppId), Is.EqualTo(new uint[] { 2001 }));

        service.ConfiguredPath = null;
        Assert.That(service.SteamProgramPath, Is.EqualTo(automaticProgram));
        Assert.That(service.SteamDirPath, Is.EqualTo(automaticDirectory));
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task OfflineRefreshKeepsLastGamesWhenLocalCacheTemporarilyYieldsNoEntries(bool sharded)
    {
        RxAppBuilder.CreateReactiveUIBuilder().WithMainThreadScheduler(ImmediateScheduler.Instance).BuildApp();
        var parsedSteam = new TestSteamService { ConfiguredPath = Path.Combine(second, "steam.exe") };
        var steam = new Mock<ISteamService>();
        steam.Setup(x => x.GetSteamProcessPid()).Returns(0);
        steam.Setup(x => x.GetAppInfoEdits()).Returns(new Dictionary<uint, SteamAppEdit>());
        steam.Setup(x => x.GetDownloadingAppList()).Returns([]);
        steam.Setup(x => x.ReadAppInfosInBatchesAsync(It.IsAny<Action<IReadOnlyList<SteamApp>>>(), It.IsAny<CancellationToken>()))
            .Returns<Action<IReadOnlyList<SteamApp>>, CancellationToken>(parsedSteam.ReadAppInfosInBatchesAsync);
        var local = new Mock<ISteamworksLocalApiService>(MockBehavior.Strict);
        var web = new Mock<ISteamworksWebApiService>(MockBehavior.Strict);
        using var connection = new SteamConnectService(steam.Object, local.Object, web.Object);

        if (sharded) WriteShard(second, 0, 2001, 1, metadataSize: 66);
        await connection.RefreshGamesListAsync(showToast: false);
        Assert.That(connection.SteamApps.Items.Select(app => app.AppId), Is.EqualTo(new uint[] { 2001 }));

        if (sharded) WriteShard(second, 0, 0, 0, metadataSize: 66);
        else File.WriteAllBytes(Path.Combine(second, "appcache", "appinfo.vdf"), []);
        for (var attempt = 0; attempt < 2; attempt++)
        {
            await connection.RefreshGamesListAsync(showToast: false);
            Assert.That(connection.SteamApps.Items.Select(app => app.AppId), Is.EqualTo(new uint[] { 2001 }));
            Assert.That(connection.IsLoadingGameList, Is.False);
        }

        if (sharded) WriteShard(second, 0, 2002, 1, metadataSize: 66);
        else CreateSteam(second, "second-account", 2002, 1);
        await connection.RefreshGamesListAsync(showToast: false);
        Assert.That(connection.SteamApps.Items.Select(app => app.AppId), Is.EqualTo(new uint[] { 2002 }));
        local.VerifyNoOtherCalls();
        web.VerifyNoOtherCalls();
    }

    [Test]
    public void SavedAccountReaderDistinguishesCorruptionFromAnEmptyList()
    {
        var service = new TestSteamService { ConfiguredPath = Path.Combine(first, "steam.exe") };
        var path = Path.Combine(first, "config", "loginusers.vdf");
        Assert.That(service.TryGetRememberUserList(out var saved), Is.True);
        Assert.That(saved.Select(user => user.AccountName), Is.EqualTo(new[] { "first-account" }));

        File.WriteAllText(path, "\"users\" { \"unclosed", Encoding.UTF8);
        Assert.That(service.TryGetRememberUserList(out _), Is.False);

        File.WriteAllText(path, "\"users\" { }", Encoding.UTF8);
        Assert.That(service.TryGetRememberUserList(out var empty), Is.True);
        Assert.That(empty, Is.Empty);

        File.Delete(path);
        Assert.That(service.TryGetRememberUserList(out var missing), Is.True);
        Assert.That(missing, Is.Empty);
    }

    [Test]
    public async Task ChangingPathDuringBatchReadDoesNotMixTheTwoInstallations()
    {
        var service = new TestSteamService { ConfiguredPath = Path.Combine(first, "steam.exe") };
        var firstBatch = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        using var resume = new ManualResetEventSlim();
        var ids = new List<uint>();
        var read = service.ReadAppInfosInBatchesAsync(batch =>
        {
            ids.AddRange(batch.Select(app => app.AppId));
            if (ids.Count == 128)
            {
                firstBatch.TrySetResult();
                if (!resume.Wait(TimeSpan.FromSeconds(10))) throw new TimeoutException("Second batch was not released.");
            }
        });

        try
        {
            await firstBatch.Task.WaitAsync(TimeSpan.FromSeconds(10));
            service.ConfiguredPath = Path.Combine(second, "steam.exe");
        }
        finally { resume.Set(); }
        await read.WaitAsync(TimeSpan.FromSeconds(10));
        Assert.That(ids, Is.EqualTo(Enumerable.Range(1001, 129).Select(id => (uint)id)));
        Assert.That((await service.GetAppInfos()).Select(app => app.AppId), Is.EqualTo(new uint[] { 2001 }));
    }

    [Test]
    public async Task SplitCacheTakesPrecedenceAndLoadsWithShardLocalKeys()
    {
        WriteShard(first, 0, 3001, 129, metadataSize: 66);
        WriteShard(first, 1, 4001, 1, metadataSize: 68);
        var service = new TestSteamService
        {
            ConfiguredPath = Path.Combine(first, "steam.exe"),
            HiddenApps = new() { [3002] = null },
        };
        var apps = new List<SteamApp>();
        var batchSizes = new List<int>();
        await service.ReadAppInfosInBatchesAsync(batch =>
        {
            apps.AddRange(batch);
            batchSizes.Add(batch.Count);
        });
        var expected = Enumerable.Range(3001, 129).Select(id => (uint)id).Where(id => id != 3002u).Append(4001u);
        Assert.That(batchSizes, Is.EqualTo(new[] { 128, 1 }));
        Assert.That(apps.Select(app => app.AppId), Is.EquivalentTo(expected));
        Assert.That(apps.Single(app => app.AppId == 4001).Name, Is.EqualTo("分片游戏 4001"));
        Assert.That(apps.All(app => app.ChangesData == null && app.OriginalData == null), Is.True);

        var editable = await service.GetAppInfos(isSaveProperties: true);
        var hidden = editable.Single(app => app.AppId == 3002);
        var properties = hidden.ChangesData!.GetPropertyValue<SteamAppPropertyTable>("appinfo");
        Assert.That(properties.GetPropertyValue<SteamAppPropertyTable>("common").GetPropertyValue<string>("name"), Is.EqualTo("分片游戏 3002"));
    }

    [Test]
    public async Task SplitCacheDoesNotReadAnUnterminatedRecordIntoTheNextApp()
    {
        var path = WriteShard(first, 0, 3001, 2, metadataSize: 64);
        using (var file = File.Open(path, FileMode.Open, FileAccess.ReadWrite))
        using (var reader = new BinaryReader(file, Encoding.UTF8, leaveOpen: true))
        using (var writer = new BinaryWriter(file, Encoding.UTF8, leaveOpen: true))
        {
            const int firstRecordSizeOffset = 44 + 8;
            file.Position = firstRecordSizeOffset;
            var size = reader.ReadUInt32();
            file.Position = firstRecordSizeOffset;
            writer.Write(size - 1);
        }

        var service = new TestSteamService { ConfiguredPath = Path.Combine(first, "steam.exe") };
        await Assert.ThrowsAsync<InvalidDataException>(() => service.ReadAppInfosInBatchesAsync(_ => Assert.Fail("Invalid data must not be emitted.")));
    }

    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    [TestCase(4)]
    public async Task ApplyingAndResettingEditsPreservesLocalAppInfo(int version)
    {
        var split = version == 4;
        var appId = split ? 3001u : 1001u;
        var original = split ? "分片游戏 3001" : "Game 0";
        var unrelated = split ? WriteShard(first, 4, 4001, 1, metadataSize: 68) : Path.Combine(second, "appcache", "appinfo.vdf");
        var untouched = File.ReadAllBytes(unrelated);
        if (split)
            WriteShard(first, 0, appId, 2, metadataSize: 66);
        else if (version != 1)
            CreateSteam(first, "first-account", appId, 2, version);
        var service = new TestSteamService { ConfiguredPath = Path.Combine(first, "steam.exe") };
        var edits = new Dictionary<uint, SteamAppEdit>
        {
            [appId] = new()
            {
                Name = "编辑后的名字",
                SortAs = string.Empty,
                Developer = "A studio",
                LaunchItems = [new SteamLaunchEdit(string.Empty, "fixture", string.Empty, null, "osx")],
            },
        };

        Assert.That(TestSteamService.Rewrite(first, edits, () => false), Is.False);
        Assert.That((await service.GetAppInfos()).Single(x => x.AppId == appId).Name, Is.EqualTo(original));
        Assert.That(TestSteamService.Rewrite(first, edits, () => true), Is.True);
        var changed = (await service.GetAppInfos()).Single(x => x.AppId == appId);
        Assert.That(changed.Name, Is.EqualTo("编辑后的名字"));
        Assert.That(changed.Developer, Is.EqualTo("A studio"));
        Assert.That(changed.GetOriginalEditValues().Name, Is.EqualTo(original));

        Assert.That(TestSteamService.Rewrite(first, edits, () => true), Is.False, "A repeated apply does not replace the cache.");
        var persisted = await service.GetAppInfos(isSaveProperties: true);
        Assert.That(SteamServiceImpl2.AreAppEditsAlreadyApplied(edits, persisted), Is.True);
        Assert.That(SteamServiceImpl2.AreAppEditsAlreadyApplied(new Dictionary<uint, SteamAppEdit>
        {
            [appId] = new() { Name = "A different edit" },
        }, persisted), Is.False);
        Assert.That(SteamServiceImpl2.AreAppEditsAlreadyApplied(edits, persisted.Where(x => x.AppId != appId)), Is.False);

        edits[appId] = new();
        Assert.That(SteamServiceImpl2.AreAppEditsAlreadyApplied(edits, persisted), Is.False);
        Assert.That(TestSteamService.Rewrite(first, edits, () => true), Is.True);
        var restored = (await service.GetAppInfos()).Single(x => x.AppId == appId);
        Assert.That(restored.Name, Is.EqualTo(original));
        Assert.That(restored.Developer, Is.Empty);
        Assert.That(restored.IsEdited, Is.False);
        Assert.That(SteamServiceImpl2.AreAppEditsAlreadyApplied(edits, await service.GetAppInfos(isSaveProperties: true)), Is.True);
        Assert.That(File.ReadAllBytes(unrelated), Is.EqualTo(untouched));
    }

    [Test]
    public void BackupRoundTripKeepsExplicitEmptyValuesAndReportsNoUnchangedFields()
    {
        var original = new SteamApp(10) { Name = "Fixture", Developer = "Studio", LaunchItems = new() { new() { Executable = "fixture" } } };
        var current = SteamAppEdit.Capture(original) with { Developer = string.Empty, LaunchItems = [] };
        var baseline = SteamAppEdit.Capture(original);
        var edits = new Dictionary<uint, SteamAppEdit> { [10] = current.Diff(baseline) };
        using var backup = new MemoryStream();
        SteamAppEditBackup.Write(backup, edits);
        backup.Position = 0;
        var restored = SteamAppEditBackup.Read(backup)[10];
        Assert.That(restored.Name, Is.Null);
        Assert.That(restored.Developer, Is.Empty);
        Assert.That(restored.LaunchItems, Is.Empty);
        original.ApplyAppInfoEdit(restored);
        Assert.That(original.Developer, Is.Empty);
        Assert.That(original.LaunchItems, Is.Null);
        original.ApplyAppInfoEdit(new());
        Assert.That(original.Developer, Is.EqualTo("Studio"));
        Assert.That(original.LaunchItems!.Single().Executable, Is.EqualTo("fixture"));
    }

    [Test]
    public async Task LegacyBackupRestoresOnlySupportedCustomizations()
    {
        var service = new TestSteamService { ConfiguredPath = Path.Combine(second, "steam.exe") };
        var source = (await service.GetAppInfos(isSaveProperties: true)).Single();
        source.ChangesData!.SetPropertyValue(BD.SteamClient.Enums.SteamAppPropertyType.String, "Legacy rename", "appinfo", "common", "name");
        source.ChangesData.SetPropertyValue(BD.SteamClient.Enums.SteamAppPropertyType.String, "Game 0", "appinfo", "steam_edit", "base_name");
        using var backup = new MemoryStream(Serializable.SMP(new List<ModifiedApp> { new(source) }));

        var target = new SteamApp(source.AppId) { Name = "Updated fixture", State = 4, CloudQuota = 100_000 };
        target.ApplyAppInfoEdit(SteamAppEditBackup.Read(backup)[source.AppId]);
        Assert.That(target.Name, Is.EqualTo("Legacy rename"));
        Assert.That(target.IsInstalled, Is.True);
        Assert.That(target.CloudQuota, Is.EqualTo(100_000));
        target.ApplyAppInfoEdit(new());
        Assert.That(target.Name, Is.EqualTo("Updated fixture"));
    }

    static string WriteShard(string root, uint shard, uint firstId, int count, int metadataSize)
    {
        string[] keys = ["appinfo", "common", "name", "type"];
        if (shard != 0)
            Array.Reverse(keys);
        using var index = new MemoryStream();
        using var data = new MemoryStream();
        using var indexWriter = new BinaryWriter(index);
        using var dataWriter = new BinaryWriter(data);
        var metadata = new byte[metadataSize];
        var sha1 = new byte[20];
        for (var id = firstId; id < firstId + count; id++)
        {
            var offset = (uint)data.Position;
            dataWriter.Write(sha1);
            Key(0, "appinfo");
            Key(0, "common");
            Key(1, "name");
            dataWriter.WriteAppInfoString($"分片游戏 {id}");
            Key(1, "type");
            dataWriter.WriteAppInfoString("game");
            dataWriter.Write(new byte[] { 8, 8, 8 });
            indexWriter.Write(id);
            indexWriter.Write(offset);
            indexWriter.Write((uint)data.Position - offset);
            indexWriter.Write(metadataSize);
            indexWriter.Write(metadata);
        }

        var folder = Directory.CreateDirectory(Path.Combine(root, "appcache", "appinfo"));
        var path = Path.Combine(folder.FullName, $"appinfo_{shard:x2}.vdf");
        using var body = new MemoryStream();
        index.WriteTo(body);
        using (var keysWriter = new BinaryWriter(body, Encoding.UTF8, leaveOpen: true))
            foreach (var key in keys) keysWriter.WriteAppInfoString(key);
        using var header = new MemoryStream(44);
        using var writer = new BinaryWriter(header);
        writer.Write(0x0756442Au);
        writer.Write(1u); // Universe
        writer.Write(shard);
        writer.Write(256u);
        writer.Write(count);
        writer.Write((int)index.Length);
        writer.Write(keys.Length);
        writer.Write(keys.Sum(key => Encoding.UTF8.GetByteCount(key) + 1));
        writer.Write((int)data.Length);
        writer.Write(Crc32.HashToUInt32(body.GetBuffer().AsSpan(0, (int)body.Length)));
        writer.Write(Crc32.HashToUInt32(header.GetBuffer().AsSpan(0, (int)header.Length)));
        using var file = File.Create(path);
        header.WriteTo(file);
        body.WriteTo(file);
        data.WriteTo(file);
        return path;

        void Key(byte kind, string key)
        {
            dataWriter.Write(kind);
            dataWriter.Write(Array.IndexOf(keys, key));
        }
    }

    static void CreateSteam(string root, string account, uint firstId, int count, int version = 1)
    {
        Directory.CreateDirectory(Path.Combine(root, "config"));
        Directory.CreateDirectory(Path.Combine(root, "appcache"));
        Directory.CreateDirectory(Path.Combine(root, "steamapps"));
        File.WriteAllBytes(Path.Combine(root, "steam.exe"), Array.Empty<byte>());
        File.WriteAllText(Path.Combine(root, "config", "loginusers.vdf"),
            $"\"users\"\n{{\n\"76561198000000001\"\n{{\n\"AccountName\" \"{account}\"\n\"PersonaName\" \"{account}\"\n\"Timestamp\" \"1700000000\"\n\"WantsOfflineMode\" \"0\"\n}}\n}}", Encoding.UTF8);
        using var writer = new BinaryWriter(File.Create(Path.Combine(root, "appcache", "appinfo.vdf")), Encoding.UTF8);
        writer.Write(0x07564426u + (uint)version);
        writer.Write(1u);
        string[] keys = ["appinfo", "common", "name", "type"];
        if (version == 3) writer.Write(0L);
        for (var i = 0; i < count; i++)
        {
            using var payload = new MemoryStream();
            using (var data = new BinaryWriter(payload, Encoding.UTF8, true))
            {
                data.Write(new byte[16]);
                data.Write(new byte[20]);
                data.Write(1u);
                if (version >= 2) data.Write(new byte[20]);
                Key(0, 0);
                Key(0, 1);
                Key(1, 2); data.WriteAppInfoString($"Game {i}");
                Key(1, 3); data.WriteAppInfoString("game");
                data.Write(new byte[] { 8, 8, 8 });

                void Key(byte type, int index)
                {
                    data.Write(type);
                    if (version == 3) data.Write(index);
                    else data.WriteAppInfoString(keys[index]);
                }
            }
            writer.Write(firstId + (uint)i);
            writer.Write((int)payload.Length);
            writer.Write(payload.GetBuffer(), 0, (int)payload.Length);
        }
        writer.Write(0u);
        if (version == 3)
        {
            var offset = writer.BaseStream.Position;
            writer.Write(keys.Length);
            foreach (var key in keys) writer.WriteAppInfoString(key);
            writer.BaseStream.Position = 8;
            writer.Write(offset);
        }
    }

    sealed class TestSteamService : SteamServiceImpl
    {
        public TestSteamService() : base(NullLoggerFactory.Instance) { }
        public static bool Rewrite(string steam, IReadOnlyDictionary<uint, SteamAppEdit> edits, Func<bool> canWrite) =>
            RewriteAppInfoCache(steam, edits, canWrite);
        public string? ConfiguredPath { get; set; }
        public Dictionary<uint, string?>? HiddenApps { get; init; }
        protected override string? ConfiguredSteamProgramPath => ConfiguredPath;
        public override ISteamConnectService Conn => throw new NotSupportedException();
        protected override string? StratSteamDefaultParameter => null;
        protected override bool IsRunSteamAdministrator => false;
        protected override Dictionary<uint, string?>? HideGameList => HiddenApps;
        protected override string? GetString(string name) => null;
    }
}
