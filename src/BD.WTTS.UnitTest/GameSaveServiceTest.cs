using BD.SteamClient.Models;
using BD.WTTS.GameList.Saves;

namespace BD.WTTS.UnitTest;

[TestFixture]
public sealed class GameSaveServiceTest
{
    const uint appId = 1234;
    const uint accountId = 2468;
    const long steamId = 76561197960268196;
    static readonly DateTime originalTime = new(2025, 1, 2, 3, 4, 5, DateTimeKind.Utc);
    static readonly CancellationToken ct = CancellationToken.None;

    DirectoryInfo temp = null!;
    GameSaveEnvironment environment = null!;
    GameSaveScanner scanner = null!;
    GameSaveBackupService backups = null!;
    SteamApp game = null!;

    [SetUp]
    public void SetUp()
    {
        temp = Directory.CreateTempSubdirectory("wtts-game-saves-");
        environment = new()
        {
            Platform = OSPlatform.Windows,
            UserProfile = InTemp("profile"),
            ApplicationData = InTemp("roaming"),
            LocalApplicationData = InTemp("local"),
            LocalApplicationDataLow = InTemp("low"),
            MyDocuments = InTemp("documents"),
            SavedGames = InTemp("saved-games"),
            CommonDocuments = InTemp("public-documents"),
            SteamDirPath = InTemp("steam"),
            SteamId64 = steamId,
            SteamId32 = accountId,
        };
        scanner = new(environment);
        backups = new();
        game = new(appId) { Name = "Save:Game", InstalledDir = InTemp("installed"), State = 4, SaveFiles = [] };
    }

    [TearDown]
    public void TearDown() => temp.Delete(recursive: true);

    [Test]
    public void UsesAccountNameWhenPersonaNameIsEmpty()
    {
        var user = new SteamUser { SteamId64 = steamId, PersonaName = "", AccountName = "remembered-login" };

        var account = GameSaveScanner.DiscoverAccounts(environment.SteamDirPath, null, [], [user], ct).Single();

        Assert.That(account.DisplayName, Is.EqualTo("remembered-login"));
    }

    [Test]
    public void PrefersSteamNicknameAndAppendsDistinctRemark()
    {
        var user = new SteamUser
        {
            SteamId64 = steamId, SteamID = "  Nickname  ", PersonaName = "Persona",
            AccountName = "Login", Remark = "  Main account  ",
        };

        var account = GameSaveScanner.DiscoverAccounts(environment.SteamDirPath, user, [], [], ct).Single();

        Assert.That(account.DisplayName, Is.EqualTo("Nickname (Main account)"));
    }

    [Test]
    public void MergesBetterNameAndRemarkIntoCurrentAccount()
    {
        var current = new SteamUser { SteamId64 = steamId, SteamID = steamId.ToString(), PersonaName = " " };
        var cached = new SteamUser { SteamId64 = steamId, Remark = "Main account" };
        var remembered = new SteamUser { SteamId64 = steamId, PersonaName = "Fresh persona", AccountName = "Login" };

        var account = GameSaveScanner.DiscoverAccounts(environment.SteamDirPath, current, [cached], [remembered], ct).Single();

        Assert.That(account.SteamId32, Is.EqualTo(accountId));
        Assert.That(account.IsCurrent, Is.True);
        Assert.That(account.DisplayName, Is.EqualTo("Fresh persona (Main account)"));
    }

    [Test]
    public void PrefersLoadedNicknameOverDuplicatePersonaNameInSteamIdField()
    {
        var current = new SteamUser { SteamId64 = steamId, SteamID = "Persona", PersonaName = "Persona" };
        var cached = new SteamUser { SteamId64 = steamId, SteamID = "Loaded nickname", PersonaName = "Persona" };

        var account = GameSaveScanner.DiscoverAccounts(environment.SteamDirPath, current, [cached], [], ct).Single();

        Assert.That(account.DisplayName, Is.EqualTo("Loaded nickname"));
    }

    [TestCase(""" "friends" { "PersonaName" "Offline persona" } """)]
    [TestCase(""" "friends" { "2468" { "name" "Offline persona" } } """)]
    [TestCase(""" "System" { "PersonaName" "Offline persona" } """)]
    public void ResolvesOfflineAccountNameFromUserdata(string localConfigSection)
    {
        var path = InTemp("steam", "userdata", accountId.ToString(), "config", "localconfig.vdf");
        Write(path, System.Text.Encoding.UTF8.GetBytes($$"""
            "UserLocalConfigStore"
            {
                {{localConfigSection}}
            }
            """));

        var account = GameSaveScanner.DiscoverAccounts(environment.SteamDirPath, null, [], [], ct).Single();

        Assert.That(account.DisplayName, Is.EqualTo("Offline persona"));
    }

    [Test]
    public void FallsBackToFullSteamIdWhenUserdataConfigHasNoUsableName()
    {
        Write(InTemp("steam", "userdata", accountId.ToString(), "config", "localconfig.vdf"),
            System.Text.Encoding.UTF8.GetBytes(""" "UserLocalConfigStore" { "friends" { "PersonaName" " " } } """));

        var account = GameSaveScanner.DiscoverAccounts(environment.SteamDirPath, null, [], [], ct).Single();

        Assert.That(account.DisplayName, Is.EqualTo(steamId.ToString()));
    }

    [TestCase("WinAppDataRoaming", "roaming")]
    [TestCase("WinAppDataLocal", "local")]
    [TestCase("WinAppDataLocalLow", "low")]
    [TestCase("WinMyDocuments", "documents")]
    [TestCase("WinSavedGames", "saved-games")]
    [TestCase("WinPublicDocuments", "public-documents")]
    [TestCase("PublicDocuments", "public-documents")]
    [TestCase("gameinstall", "installed")]
    [TestCase("SteamInstall", "steam")]
    public void ResolvesWindowsAndSteamRoots(string root, string folder)
    {
        var location = new GameSavePathResolver(environment).ResolveRule(game, Rule(root, @"vendor\game", null));
        Assert.That(location.ResolvedDirectory, Is.EqualTo(InTemp(folder, "vendor", "game")));
        Assert.That(location.Status, Is.EqualTo(GameSaveLocationStatus.NotFound));
        Assert.That(location.Pattern, Is.EqualTo("*"));
    }

    [TestCase("SteamUserData", "slot")]
    [TestCase("SteamUserData", @"1234\slot")]
    [TestCase("SteamUserData", "1234/remote/slot")]
    [TestCase("SteamCloudDocuments", "remote/slot")]
    public void ResolvesUserdataPathsRelativeToRemote(string root, string path)
    {
        var location = new GameSavePathResolver(environment).ResolveRule(game, Rule(root, path));
        Assert.That(location.ResolvedDirectory, Is.EqualTo(Path.Combine(Remote, "slot")));
    }

    [Test]
    public void ResolvesWindowsKnownFolderFallbacksAndSteamTokens()
    {
        var resolver = new GameSavePathResolver(environment with { LocalApplicationDataLow = null, SavedGames = null });
        Assert.That(resolver.ResolveRule(game, Rule("WinAppDataLocalLow", "")).ResolvedDirectory,
            Is.EqualTo(InTemp("profile", "AppData", "LocalLow")));
        Assert.That(resolver.ResolveRule(game, Rule("WinSavedGames", "")).ResolvedDirectory,
            Is.EqualTo(InTemp("profile", "Saved Games")));

        var tokens = resolver.ResolveRule(game, Rule("WinAppDataLocal", "{64bitsteamid}/{STEAM3ACCOUNTID}/{SteamID}",
            "{steam3accountid}.sav"));
        Assert.That(tokens.ResolvedDirectory, Is.EqualTo(InTemp("local", steamId.ToString(), accountId.ToString(), steamId.ToString())));
        Assert.That(tokens.Pattern, Is.EqualTo($"{accountId}.sav"));
        Assert.That(new GameSavePathResolver(environment with { SteamId64 = 0 }).ResolveRule(game,
            Rule("WinAppDataLocal", "{64BitSteamID}")).Status, Is.EqualTo(GameSaveLocationStatus.Unresolved));
    }

    [TestCase("Linux", "LinuxHome", "profile")]
    [TestCase("Linux", "LinuxXdgDataHome", "profile/.local/share")]
    [TestCase("Linux", "LinuxXdgConfigHome", "profile/.config")]
    [TestCase("OSX", "MacHome", "profile")]
    [TestCase("OSX", "MacAppSupport", "profile/Library/Application Support")]
    [TestCase("OSX", "MacDocuments", "profile/Documents")]
    public void ResolvesSupportedUnixRootsOnlyOnTheirPlatform(string platform, string root, string folder)
    {
        var rule = Rule(root, "saves");
        var resolver = new GameSavePathResolver(environment with { Platform = OSPlatform.Create(platform.ToUpperInvariant()) });
        Assert.That(resolver.ResolveRule(game, rule).ResolvedDirectory, Is.EqualTo(InTemp([.. folder.Split('/'), "saves"])));
        Assert.That(new GameSavePathResolver(environment).ResolveRule(game, rule).Status,
            Is.EqualTo(GameSaveLocationStatus.Unresolved));
    }

    [TestCase("../outside")]
    [TestCase(@"one\..\..\outside")]
    [TestCase("../roaming/saves")]
    [TestCase("/absolute")]
    [TestCase(@"C:\")]
    [TestCase(@"C:\temp\saves")]
    [TestCase(@"\\?\C:\saves")]
    [TestCase(@"\\.\C:\saves")]
    public void RejectsTraversalAndAbsoluteUfsPaths(string path)
    {
        var location = new GameSavePathResolver(environment).ResolveRule(game, Rule("WinAppDataRoaming", path));
        Assert.That(location.Status, Is.EqualTo(GameSaveLocationStatus.Unresolved));
        Assert.That(location.ResolvedDirectory, Is.Null);
    }

    [Test]
    public void RejectsVolumeAndDeviceRootsEvenInTheSuppliedEnvironment()
    {
        foreach (var root in new[] { Path.GetPathRoot(temp.FullName), @"\\?\C:\saves", @"\\.\C:\saves" })
        {
            var resolver = new GameSavePathResolver(environment with { ApplicationData = root });
            Assert.That(resolver.ResolveRule(game, Rule("WinAppDataRoaming", "")).Status,
                Is.EqualTo(GameSaveLocationStatus.Unresolved));
        }
    }

    [Test]
    public async Task ScansPatternsAndDeduplicatesOverlappingFiles()
    {
        game.SaveFiles!.Add(Rule("WinAppDataRoaming", "saves", "*.sav"));
        game.SaveFiles.Add(Rule("WinAppDataRoaming", "saves", "slot*", recursive: true));
        game.SaveFiles.Add(Rule("WinAppDataRoaming", "saves/nested", "*.sav"));
        game.SaveFiles.Add(Rule("SteamUserData", "1234/remote", "*", recursive: true));
        Write(InTemp("roaming", "saves", "slot.sav"), [1, 2, 3]);
        Write(InTemp("roaming", "saves", "slot.txt"), [4, 5]);
        Write(InTemp("roaming", "saves", "nested", "slot.sav"), [6, 7, 8, 9], originalTime.AddDays(1));
        Write(InTemp("roaming", "saves", "nested", "other.sav"), [10]);
        Write(Path.Combine(Remote, "zero.dat"), []);

        var progress = new InlineProgress<GameSaveScanProgress>();
        var result = (await scanner.ScanAsync([game], progress, ct)).Single();

        Assert.That(result.IsInstalled, Is.True);
        Assert.That(result.HasLocalSaves, Is.True);
        Assert.That(result.HasUserdataRemote, Is.True);
        Assert.That(result.Locations.Select(x => x.TotalBytes), Is.EqualTo(new long[] { 0, 3, 9, 5 }));
        Assert.That(result.Locations.Select(x => x.FileCount), Is.EqualTo(new[] { 1, 1, 3, 2 }));
        Assert.That(result.TotalBytes, Is.EqualTo(10));
        Assert.That(result.TotalFileCount, Is.EqualTo(5));
        Assert.That(result.LastModifiedUtc, Is.EqualTo(originalTime.AddDays(1)));
        Assert.That(result.Locations[2].Files.Select(file => file.RelativePath), Is.EquivalentTo(new[]
        {
            "slot.sav", "slot.txt", Path.Combine("nested", "slot.sav"),
        }));
        Assert.That(progress.Updates, Is.EqualTo(new GameSaveScanProgress[] { new(0, 1, null), new(1, 1, game.Name) }));

        var backup = await backups.BackupGameAsync(result, InTemp("backup"), ct: ct);
        var snapshot = RequireSuccess(backup);
        Assert.That(backup.FilesProcessed, Is.EqualTo(result.TotalFileCount));
        Assert.That(backup.BytesProcessed, Is.EqualTo(result.TotalBytes));
        var restored = await backups.RestoreFromSnapshotAsync(snapshot, new(false), ct);
        Assert.That(restored.Success, Is.True, restored.ErrorMessage);
        Assert.That(restored.FilesProcessed, Is.EqualTo(result.TotalFileCount));
    }

    [Test]
    public async Task DiscoversUninstalledScreenshotOnlyGamesAndCountsThumbnailsOnlyInDiskUsage()
    {
        Write(Path.Combine(Screenshots, "cover.JPG"), [1, 2]);
        Write(Path.Combine(Screenshots, "photo.jpeg"), [3]);
        Write(Path.Combine(Screenshots, "photo.png"), [4]);
        Write(Path.Combine(Screenshots, "photo.bmp"), []);
        Write(Path.Combine(Screenshots, "photo.webp"), [5, 6, 7]);
        Write(Path.Combine(Screenshots, "thumbnails", "cover.jpg"), [8, 9]);
        Write(Path.Combine(Screenshots, "thumbnails", "cache", "index.dat"), [10], originalTime.AddDays(1));
        Write(Path.Combine(Screenshots, "metadata.txt"), [11, 12]);
        Write(Path.Combine(Screenshots, "unrelated", "image.png"), [13, 14]);
        Write(InTemp("steam", "userdata", "8642", "760", "remote", "2345", "screenshots", "other.png"), [15]);

        var entry = (await scanner.ScanAsync(ct: ct)).Single();
        Assert.That(entry.AppId, Is.EqualTo(appId));
        Assert.That(entry.IsInstalled, Is.False);
        Assert.That(entry.HasLocalSaves, Is.False);
        Assert.That(entry.HasScreenshots, Is.True);
        Assert.That(entry.ScreenshotCount, Is.EqualTo(5));
        Assert.That(entry.ScreenshotBytes, Is.EqualTo(10));
        Assert.That(entry.TotalOccupiedBytes, Is.EqualTo(10));
        var screenshots = entry.Screenshots!;
        Assert.That(screenshots.DirectoryPath, Is.EqualTo(Screenshots));
        Assert.That(screenshots.Status, Is.EqualTo(GameSaveLocationStatus.Found));
        Assert.That(screenshots.LastModifiedUtc, Is.EqualTo(originalTime.AddDays(1)));
        Assert.That(screenshots.Files.Select(file => file.Name), Is.EquivalentTo(new[]
        {
            "cover.JPG", "photo.jpeg", "photo.png", "photo.bmp", "photo.webp",
        }));
        Assert.That(screenshots.Files.Single(file => file.Name == "cover.JPG"),
            Is.EqualTo(new GameSaveFileItem("cover.JPG", "cover.JPG", Path.Combine(Screenshots, "cover.JPG"), 2, originalTime)));

        var save = Path.Combine(Remote, "slot.sav");
        Write(save, [16, 17]);
        var rescanned = await scanner.RescanGameAsync(entry.App, ct: ct);
        Assert.That(rescanned.ScreenshotCount, Is.EqualTo(5));
        Assert.That(rescanned.TotalOccupiedBytes, Is.EqualTo(12));
        Assert.That(rescanned.Locations.Single().Files.Single(),
            Is.EqualTo(new GameSaveFileItem("slot.sav", "slot.sav", save, 2, originalTime)));
    }

    [Test]
    public async Task IncludesThumbnailOnlyStorageWithoutCountingScreenshots()
    {
        Write(Path.Combine(Screenshots, "thumbnails", "cover.jpg"), [1, 2]);

        var entry = (await scanner.ScanAsync(ct: ct)).Single();
        Assert.That(entry.HasScreenshots, Is.True);
        Assert.That(entry.ScreenshotCount, Is.Zero);
        Assert.That(entry.ScreenshotBytes, Is.EqualTo(2));
        Assert.That(entry.Screenshots!.Status, Is.EqualTo(GameSaveLocationStatus.Found));
        Assert.That(entry.Screenshots.Files, Is.Empty);
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task SkipsLinkedScreenshotStorage(bool linkScreenshotRoot)
    {
        var other = InTemp("outside");
        Write(Path.Combine(other, "linked.png"), [1, 2, 3]);
        var link = linkScreenshotRoot ? Screenshots : Path.Combine(Screenshots, "thumbnails");
        Directory.CreateDirectory(Path.GetDirectoryName(link)!);
        if (!linkScreenshotRoot)
            Write(Path.Combine(Screenshots, "real.png"), [4]);
        CreateDirectoryLink(link, other);
        try
        {
            var entry = (await scanner.ScanAsync(ct: ct)).Single();
            Assert.That(entry.ScreenshotBytes, Is.EqualTo(linkScreenshotRoot ? 0 : 1));
            Assert.That(entry.ScreenshotCount, Is.EqualTo(linkScreenshotRoot ? 0 : 1));
            Assert.That(entry.Screenshots!.ErrorMessage, Is.Not.Null);
            Assert.That(entry.Screenshots.Status, Is.EqualTo(linkScreenshotRoot
                ? GameSaveLocationStatus.AccessDenied : GameSaveLocationStatus.Found));
        }
        finally
        {
            Directory.Delete(link);
        }
    }

    [Test]
    public async Task BuildsSortedFileTreeAndAggregatesNestedDirectoriesWithoutReadingDisk()
    {
        var lastWrite = originalTime.AddDays(1);
        Write(Path.Combine(Remote, "aaa.sav"), [1]);
        Write(Path.Combine(Remote, "slot2", "manual.sav"), [2, 3]);
        Write(Path.Combine(Remote, "slot1", "manual.sav"), [4, 5, 6]);
        Write(Path.Combine(Remote, "slot1", "auto", "save.dat"), [7, 8, 9, 10], lastWrite);
        var location = (await scanner.ScanAsync(ct: ct)).Single().Locations.Single();
        Directory.Delete(Remote, recursive: true);

        var tree = GameSaveTreeBuilder.Build(location);
        Assert.That(tree.Select(node => node.Name), Is.EqualTo(new[] { "slot1", "slot2", "aaa.sav" }));
        var slot = tree[0];
        Assert.That(slot.IsDirectory, Is.True);
        Assert.That(slot.FullPath, Is.EqualTo(Path.Combine(Remote, "slot1")));
        Assert.That(slot.RelativePath, Is.EqualTo("slot1"));
        Assert.That(slot.SizeBytes, Is.EqualTo(7));
        Assert.That(slot.FileCount, Is.EqualTo(2));
        Assert.That(slot.LastWriteUtc, Is.EqualTo(lastWrite));
        Assert.That(slot.Children.Select(node => node.Name), Is.EqualTo(new[] { "auto", "manual.sav" }));
        var auto = slot.Children[0];
        Assert.That(auto.SizeBytes, Is.EqualTo(4));
        Assert.That(auto.FileCount, Is.EqualTo(1));
        var file = auto.Children.Single();
        Assert.That(file.IsDirectory, Is.False);
        Assert.That(file.RelativePath, Is.EqualTo(Path.Combine("slot1", "auto", "save.dat")));
        Assert.That(file.LastWriteUtc, Is.EqualTo(lastWrite));
        Assert.That(file.Children, Is.Empty);
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task PublishesAllInstalledGamesBeforeBackgroundBatchesAndHonorsIndependentCancellation(bool cancelBackground)
    {
        var secondInstalled = new SteamApp(2345) { Name = "Another installed game", State = 4 };
        var uninstalled = Enumerable.Range(3000, 13)
            .Select(id => new SteamApp((uint)id) { Name = $"Game {id}" }).ToArray();
        Write(Path.Combine(Screenshots, "installed.jpg"), [1]);
        Write(InTemp("steam", "userdata", accountId.ToString(), "760", "remote", "3000", "screenshots", "offline.png"), [2]);
        var phasedScanner = new GameSaveScanner(environment, [game, secondInstalled, .. uninstalled]);
        using var background = new CancellationTokenSource();
        IReadOnlyList<GameSaveEntry>? installed = null;
        var batches = new List<uint[]>();
        var progress = new InlineProgress<GameSavePhasedScanProgress>();
        var installedCallbacks = 0;

        await phasedScanner.ScanPhasedAsync(null, entries =>
        {
            installedCallbacks++;
            Assert.That(batches, Is.Empty);
            Assert.That(progress.Updates.Select(update => update.Phase), Is.All.EqualTo(GameSaveScanPhase.Installed));
            installed = entries;
        }, batch =>
        {
            Assert.That(installedCallbacks, Is.EqualTo(1));
            Assert.That(installed!.Select(entry => entry.AppId), Is.EquivalentTo(new[] { appId, secondInstalled.AppId }));
            Assert.That(batch.All(entry => !entry.IsInstalled), Is.True);
            batches.Add(batch.Select(entry => entry.AppId).ToArray());
            if (cancelBackground)
                background.Cancel();
        }, progress, installedCt: ct, backgroundCt: background.Token);

        Assert.That(installedCallbacks, Is.EqualTo(1));
        Assert.That(installed!.Single(entry => entry.AppId == appId).ScreenshotCount, Is.EqualTo(1));
        Assert.That(batches[0], Is.EqualTo(new[] { 3000u }));
        Assert.That(batches.SelectMany(batch => batch), Is.EqualTo((cancelBackground ? uninstalled.Take(1) : uninstalled)
            .Select(app => app.AppId)));
        Assert.That(progress.Updates.Last().Phase, Is.EqualTo(cancelBackground
            ? GameSaveScanPhase.Uninstalled : GameSaveScanPhase.Completed));
        var expectedCounts = Enumerable.Range(0, 3)
            .Concat(Enumerable.Range(2, cancelBackground ? 2 : uninstalled.Length + 1));
        if (!cancelBackground)
            expectedCounts = expectedCounts.Append(2 + uninstalled.Length);
        Assert.That(progress.Updates.Select(update => update.TotalDiscoveredGamesCount), Is.EqualTo(expectedCounts));
    }

    [Test]
    public async Task DiscoversBacksUpAndRestoresAnUninstalledGameFromOfflineUserdata()
    {
        var file = Path.Combine(Remote, "offline.sav");
        Write(file, [1]);
        var results = await scanner.ScanAsync(ct: ct);
        Assert.That(results, Has.Count.EqualTo(1));
        Assert.That(results[0].AppId, Is.EqualTo(appId));
        Assert.That(results[0].IsInstalled, Is.False);
        Assert.That(results[0].App.SaveFiles, Is.Null.Or.Empty);
        Assert.That(results[0].HasUserdataRemote, Is.True);
        Assert.That(results[0].TotalFileCount, Is.EqualTo(1));
        Assert.That(results[0].TotalBytes, Is.EqualTo(1));

        var root = InTemp("backup");
        var snapshot = RequireSuccess(await backups.BackupGameAsync(results[0], root, ct: ct));
        Write(file, [2, 3], originalTime.AddDays(1));
        var restored = await backups.RestoreFromSnapshotAsync(snapshot, new(PreRestoreBackupRoot: root), ct);
        Assert.That(restored.Success, Is.True, restored.ErrorMessage);
        Assert.That(File.ReadAllBytes(file), Is.EqualTo(new byte[] { 1 }));
        Assert.That(File.GetLastWriteTimeUtc(file), Is.EqualTo(originalTime));
        Assert.That((await scanner.RescanGameAsync(results[0].App, ct: ct)).TotalBytes, Is.EqualTo(1));
        Assert.That((await backups.ListBackupsAsync(root, appId, ct)).Single(x => x.IsAutoPreRestore).SnapshotDirectory,
            Is.EqualTo(restored.AutoBackupPath));
    }

    [Test]
    public async Task DistinguishesEmptyMissingUnsupportedAndFoundDirectoriesOnRescan()
    {
        game.SaveFiles!.Add(Rule("gameinstall", "saves"));
        game.SaveFiles.Add(Rule("LinuxHome", "not-this-platform"));
        var emptyDirectory = Directory.CreateDirectory(InTemp("installed", "saves")).FullName;
        var entry = await scanner.RescanGameAsync(game, ct: ct);
        Assert.That(entry.Locations.Select(x => x.Status), Is.EqualTo(new[]
        {
            GameSaveLocationStatus.NotFound, GameSaveLocationStatus.Empty, GameSaveLocationStatus.Unresolved,
        }));
        Write(Path.Combine(emptyDirectory, "zero.sav"), []);
        entry = await scanner.RescanGameAsync(game, ct: ct);
        Assert.That(entry.TotalBytes, Is.Zero);
        Assert.That(entry.TotalFileCount, Is.EqualTo(1));
        Assert.That(entry.Locations[1].Status, Is.EqualTo(GameSaveLocationStatus.Found));
    }

    [Test]
    public async Task CreatesSnapshotWithManifestHierarchyAndTimestamps()
    {
        var entry = await CreateLiveSaveAsync();
        var root = InTemp("backup");
        var result = await backups.BackupGameAsync(entry, root, "manual note", ct: ct);
        var snapshot = RequireSuccess(result);
        var archive = Path.Combine(snapshot, "files", "0", "chapter", "slot.sav");
        Assert.That(Path.GetFileName(snapshot), Does.Match(@"^\d{8}_\d{6}_[0-9a-f]{4}$"));
        Assert.That(Path.GetFileName(Path.GetDirectoryName(snapshot)), Is.EqualTo("1234_Save_Game"));
        Assert.That(File.ReadAllBytes(archive), Is.EqualTo(new byte[] { 1, 2, 3 }));
        Assert.That(File.GetLastWriteTimeUtc(archive), Is.EqualTo(originalTime));
        Assert.That(result.FilesProcessed, Is.EqualTo(2));
        Assert.That(result.BytesProcessed, Is.EqualTo(5));

        using var manifest = ReadManifest(snapshot);
        var json = manifest.RootElement;
        Assert.That(json.GetProperty("version").GetInt32(), Is.EqualTo(1));
        Assert.That(json.GetProperty("steamId64").GetInt64(), Is.EqualTo(steamId));
        Assert.That(json.GetProperty("steamId32").GetUInt32(), Is.EqualTo(accountId));
        Assert.That(json.GetProperty("locations")[0].GetProperty("originalDirectory").GetString(), Is.EqualTo(Live));
        Assert.That(json.GetProperty("note").GetString(), Is.EqualTo("manual note"));
    }

    [Test]
    public async Task RejectsBackupInsideSaveTreeAndCopiesBatchToAlternatePath()
    {
        var entry = await CreateLiveSaveAsync();
        var inside = Path.Combine(Live, "nested-backups");
        var blocked = await backups.BackupGameAsync(entry, inside, ct: ct);
        Assert.That(blocked.Success, Is.False);
        Assert.That(blocked.ErrorMessage, Does.Contain("inside"));
        Assert.That(Directory.Exists(inside), Is.False);

        var customRoot = InTemp("external", "custom");
        var progress = new InlineProgress<GameSaveScanProgress>();
        var result = (await backups.BackupGamesAsync([entry], customRoot, "custom copy", progress, ct)).Single();
        var snapshot = RequireSuccess(result);
        Assert.That(snapshot, Does.StartWith(customRoot + Path.DirectorySeparatorChar));
        Assert.That((await backups.ListBackupsAsync(customRoot, appId, ct)).Single().Note, Is.EqualTo("custom copy"));
        Assert.That(progress.Updates.Last().ProcessedGames, Is.EqualTo(1));
    }

    [Test]
    public async Task ListsDirectAndNestedSnapshotsNewestFirstAndFiltersAppIds()
    {
        var entry = await CreateLiveSaveAsync();
        var root = InTemp("backup");
        var first = RequireSuccess(await backups.BackupGameAsync(entry, root, "first", ct: ct));
        var second = RequireSuccess(await backups.BackupGameAsync(entry, root, "second", ct: ct));
        var direct = Path.Combine(root, Path.GetFileName(second));
        Directory.Move(second, direct);
        ReplaceManifestValue(first, "createdAtUtc", JsonValue.Create(originalTime));
        Directory.CreateDirectory(Path.Combine(root, "invalid"));
        File.WriteAllText(Path.Combine(root, "invalid", "manifest.json"), "{");

        var listed = await backups.ListBackupsAsync(root, appId, ct);
        Assert.That(listed.Select(x => x.Note), Is.EqualTo(new[] { "second", "first" }));
        Assert.That(listed[0].SnapshotDirectory, Is.EqualTo(direct));
        Assert.That(await backups.ListBackupsAsync(root, appId + 1, ct), Is.Empty);
        Assert.That((await backups.ListBackupsAsync(direct, ct: ct)).Single().BackupId, Is.EqualTo(listed[0].BackupId));
    }

    [Test]
    public async Task RestoresModifiedAndDeletedFilesAndKeepsAutomaticSafetySnapshot()
    {
        var entry = await CreateLiveSaveAsync();
        var root = InTemp("backup");
        var snapshot = RequireSuccess(await backups.BackupGameAsync(entry, root, ct: ct));
        var modified = Path.Combine(Live, "chapter", "slot.sav");
        var deleted = Path.Combine(Live, "profile.sav");
        Write(modified, [9, 8, 7, 6], originalTime.AddDays(2));
        File.Delete(deleted);
        var unrelated = Path.Combine(Live, "unrelated.txt");
        Write(unrelated, [6]);

        var result = await backups.RestoreFromSnapshotAsync(snapshot, new(), ct);
        Assert.That(result.Success, Is.True, result.ErrorMessage);
        Assert.That(result.FilesProcessed, Is.EqualTo(2));
        Assert.That(File.ReadAllBytes(modified), Is.EqualTo(new byte[] { 1, 2, 3 }));
        Assert.That(File.GetLastWriteTimeUtc(modified), Is.EqualTo(originalTime));
        Assert.That(File.ReadAllBytes(deleted), Is.EqualTo(new byte[] { 4, 5 }));
        Assert.That(File.ReadAllBytes(unrelated), Is.EqualTo(new byte[] { 6 }));
        Assert.That(result.AutoBackupPath, Does.StartWith(Path.Combine(Path.GetDirectoryName(snapshot)!, "_AutoPreRestore")));
        var safety = (await backups.ListBackupsAsync(result.AutoBackupPath!, appId, ct)).Single();
        Assert.That(safety.IsAutoPreRestore, Is.True);
        Assert.That(safety.FileCount, Is.EqualTo(1));
        Assert.That(File.ReadAllBytes(Path.Combine(safety.SnapshotDirectory, "files", "0", "chapter", "slot.sav")),
            Is.EqualTo(new byte[] { 9, 8, 7, 6 }));
    }

    [Test]
    public async Task RestoresExternalFolderAndStoresSafetySnapshotAtCustomRoot()
    {
        var entry = await CreateLiveSaveAsync();
        var target = entry.Locations.Single(x => x.Status == GameSaveLocationStatus.Found);
        var external = InTemp("external");
        Write(Path.Combine(external, "chapter", "slot.sav"), [11, 12]);
        Write(Path.Combine(external, "chapter", "new.sav"), [13]);
        var root = InTemp("safety");
        var result = await backups.RestoreRawFolderToLocationAsync(external, target, new(PreRestoreBackupRoot: root), entry, ct);
        Assert.That(result.Success, Is.True, result.ErrorMessage);
        Assert.That(result.FilesProcessed, Is.EqualTo(2));
        Assert.That(File.ReadAllBytes(Path.Combine(Live, "chapter", "slot.sav")), Is.EqualTo(new byte[] { 11, 12 }));
        Assert.That(File.ReadAllBytes(Path.Combine(Live, "chapter", "new.sav")), Is.EqualTo(new byte[] { 13 }));
        var safety = (await backups.ListBackupsAsync(root, appId, ct)).Single();
        Assert.That(safety.SnapshotDirectory, Is.EqualTo(result.AutoBackupPath));
        Assert.That(safety.IsAutoPreRestore, Is.True);
        Assert.That(safety.GameName, Is.EqualTo(entry.Name));
        Assert.That(File.ReadAllBytes(Path.Combine(safety.SnapshotDirectory, "files", "0", "chapter", "slot.sav")),
            Is.EqualTo(new byte[] { 1, 2, 3 }));
    }

    [Test]
    public async Task RestoresAnEmptyLiveTreeWithoutCreatingASafetySnapshot()
    {
        var entry = await CreateLiveSaveAsync();
        var snapshot = RequireSuccess(await backups.BackupGameAsync(entry, InTemp("backup"), ct: ct));
        Directory.Delete(Live, recursive: true);
        var result = await backups.RestoreFromSnapshotAsync(snapshot, new(), ct);
        Assert.That(result.Success, Is.True, result.ErrorMessage);
        Assert.That(result.AutoBackupPath, Is.Null);
        Assert.That(File.ReadAllBytes(Path.Combine(Live, "chapter", "slot.sav")), Is.EqualTo(new byte[] { 1, 2, 3 }));
    }

    [TestCase("relativePath", "../../outside.sav")]
    [TestCase("relativeArchiveDir", "../outside")]
    public async Task RejectsManifestTraversalBeforeWritingLiveFiles(string property, string path)
    {
        var entry = await CreateLiveSaveAsync();
        var snapshot = RequireSuccess(await backups.BackupGameAsync(entry, InTemp("backup"), ct: ct));
        var manifestPath = Path.Combine(snapshot, "manifest.json");
        var manifest = JsonNode.Parse(File.ReadAllText(manifestPath))!;
        var location = manifest["locations"]![0]!;
        var node = property == "relativePath" ? location["files"]![0]! : location;
        node[property] = path;
        File.WriteAllText(manifestPath, manifest.ToJsonString());
        var modified = Path.Combine(Live, "profile.sav");
        Write(modified, [40]);

        var result = await backups.RestoreFromSnapshotAsync(snapshot, new(false), ct);
        Assert.That(result.Success, Is.False);
        Assert.That(result.FilesProcessed, Is.Zero);
        Assert.That(File.ReadAllBytes(modified), Is.EqualTo(new byte[] { 40 }));
    }

    [Test]
    public async Task UsesTheApprovedSnapshotDestinationsWhenTheManifestChanges()
    {
        var entry = await CreateLiveSaveAsync();
        var snapshot = RequireSuccess(await backups.BackupGameAsync(entry, InTemp("backup"), ct: ct));
        var approved = await GameSaveBackupService.ReadSnapshotManifestAsync(snapshot);
        Assert.That(approved, Is.Not.Null);
        var manifestPath = Path.Combine(snapshot, "manifest.json");
        var manifest = JsonNode.Parse(File.ReadAllText(manifestPath))!;
        var other = InTemp("other");
        manifest["locations"]![0]!["originalDirectory"] = other;
        File.WriteAllText(manifestPath, manifest.ToJsonString());
        var liveFile = Path.Combine(Live, "profile.sav");
        Write(liveFile, [40]);

        var result = await backups.RestoreFromSnapshotAsync(snapshot, new(false), approved, ct);
        Assert.That(result.Success, Is.True, result.ErrorMessage);
        Assert.That(File.ReadAllBytes(liveFile), Is.EqualTo(new byte[] { 4, 5 }));
        Assert.That(Directory.Exists(other), Is.False);
    }

    [Test]
    public async Task ChecksEveryArchiveFileBeforeRestoringAny()
    {
        var entry = await CreateLiveSaveAsync();
        var snapshot = RequireSuccess(await backups.BackupGameAsync(entry, InTemp("backup"), ct: ct));
        File.Delete(Path.Combine(snapshot, "files", "0", "chapter", "slot.sav"));
        var modified = Path.Combine(Live, "profile.sav");
        Write(modified, [40]);

        var result = await backups.RestoreFromSnapshotAsync(snapshot, new(false), ct);
        Assert.That(result.Success, Is.False);
        Assert.That(result.FilesProcessed, Is.Zero);
        Assert.That(File.ReadAllBytes(modified), Is.EqualTo(new byte[] { 40 }));
    }

    [Test]
    public async Task DeletesOnlyVerifiedSnapshotUnderBackupRoot()
    {
        var entry = await CreateLiveSaveAsync();
        var root = InTemp("backup");
        var snapshot = RequireSuccess(await backups.BackupGameAsync(entry, root, ct: ct));
        var summary = (await backups.ListBackupsAsync(root, appId, ct)).Single();
        Assert.That((await backups.DeleteBackupAsync(summary, InTemp("unrelated"), ct)).Success, Is.False);
        Assert.That((await backups.DeleteBackupAsync(summary, snapshot, ct)).Success, Is.False);
        Assert.That((await backups.DeleteBackupAsync(summary with { AppId = appId + 1 }, root, ct)).Success, Is.False);
        var unverified = Directory.CreateDirectory(Path.Combine(root, summary.BackupId)).FullName;
        Write(Path.Combine(unverified, "keep.sav"), [5]);
        Assert.That((await backups.DeleteBackupAsync(summary with { SnapshotDirectory = unverified }, root, ct)).Success, Is.False);
        Assert.That(File.Exists(Path.Combine(unverified, "keep.sav")), Is.True);

        Assert.That((await backups.DeleteBackupAsync(summary, root, ct)).Success, Is.True);
        Assert.That(Directory.Exists(snapshot), Is.False);
        Assert.That(await backups.ListBackupsAsync(root, ct: ct), Is.Empty);
        Assert.That(File.Exists(Path.Combine(Live, "profile.sav")), Is.True);
    }

    [Test]
    public async Task RefusesLinksDuringScanRestoreAndDeletion()
    {
        var entry = await CreateLiveSaveAsync();
        var other = Directory.CreateDirectory(InTemp("outside")).FullName;
        Write(Path.Combine(other, "hidden.sav"), [9]);
        var link = Path.Combine(Live, "link");
        CreateDirectoryLink(link, other);
        try
        {
            game.SaveFiles!.Add(Rule("WinAppDataRoaming", "saves/link"));
            var scan = await scanner.RescanGameAsync(game, ct: ct);
            Assert.That(scan.TotalFileCount, Is.EqualTo(2));
            Assert.That(scan.Locations.Single(x => x.RawPath == "saves").ErrorMessage, Is.Not.Null);
            Assert.That(scan.Locations.Single(x => x.RawPath == "saves/link").Status, Is.EqualTo(GameSaveLocationStatus.AccessDenied));

            var external = Directory.CreateDirectory(InTemp("external")).FullName;
            Write(Path.Combine(external, "link", "hidden.sav"), [5]);
            Write(Path.Combine(external, "profile.sav"), [7]);
            var target = entry.Locations.Single(x => x.Status == GameSaveLocationStatus.Found);
            Assert.That((await backups.RestoreRawFolderToLocationAsync(external, target, new(false), ct)).Success, Is.False);
            Assert.That(File.ReadAllBytes(Path.Combine(other, "hidden.sav")), Is.EqualTo(new byte[] { 9 }));
            Assert.That(File.ReadAllBytes(Path.Combine(Live, "profile.sav")), Is.EqualTo(new byte[] { 4, 5 }));

            var root = InTemp("backup");
            var snapshot = RequireSuccess(await backups.BackupGameAsync(entry, root, ct: ct));
            var summary = (await backups.ListBackupsAsync(root, appId, ct)).Single();
            var snapshotLink = Path.Combine(snapshot, "link");
            CreateDirectoryLink(snapshotLink, other);
            try
            {
                Assert.That((await backups.DeleteBackupAsync(summary, root, ct)).Success, Is.False);
                Assert.That(File.Exists(Path.Combine(snapshot, "manifest.json")), Is.True);
                Assert.That(File.Exists(Path.Combine(other, "hidden.sav")), Is.True);
            }
            finally
            {
                Directory.Delete(snapshotLink);
            }
        }
        finally
        {
            Directory.Delete(link);
        }
    }

    [Test]
    public async Task PersistsCustomSettingsAndUsesDefaultsForNewProfiles()
    {
        var store = new GameSaveSettingsStore(InTemp("appdata"));
        Assert.That(await store.LoadAsync(ct), Is.EqualTo(new GameSaveSettings(InTemp("appdata", "GameSaveBackups"))));
        var custom = new GameSaveSettings(InTemp("user", "backups"), false);
        await store.SaveAsync(custom, ct);
        Assert.That(await store.LoadAsync(ct), Is.EqualTo(custom));
        Assert.That(Directory.GetFiles(InTemp("appdata", "GameSaves")).Select(Path.GetFileName),
            Is.EqualTo(new[] { "settings.json" }));
    }

    [Test]
    public async Task HonorsCancellationBeforeScanningOrCreatingSnapshot()
    {
        var entry = await CreateLiveSaveAsync();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var root = InTemp("backup");
        await Assert.CatchAsync<OperationCanceledException>(() => scanner.RescanGameAsync(game, ct: cancellation.Token));
        await Assert.CatchAsync<OperationCanceledException>(() => backups.BackupGameAsync(entry, root, ct: cancellation.Token));
        Assert.That(Directory.Exists(root), Is.False);
    }

    async Task<GameSaveEntry> CreateLiveSaveAsync()
    {
        game.SaveFiles!.Add(Rule("WinAppDataRoaming", "saves", "*.sav", recursive: true));
        Write(Path.Combine(Live, "chapter", "slot.sav"), [1, 2, 3]);
        Write(Path.Combine(Live, "profile.sav"), [4, 5]);
        return await scanner.RescanGameAsync(game, ct: ct);
    }

    string Live => InTemp("roaming", "saves");

    string Remote => InTemp("steam", "userdata", accountId.ToString(), appId.ToString(), "remote");

    string Screenshots => InTemp("steam", "userdata", accountId.ToString(), "760", "remote", appId.ToString(), "screenshots");

    string InTemp(params string[] parts) => Path.Combine(temp.FullName, Path.Combine(parts));

    static SteamAppSaveFile Rule(string root, string path, string? pattern = "*.sav", bool recursive = false)
        => new(appId, root, path, pattern) { Recursive = recursive };

    static void Write(string path, byte[] data, DateTime? time = null)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllBytes(path, data);
        File.SetLastWriteTimeUtc(path, time ?? originalTime);
    }

    static string RequireSuccess(GameSaveOperationResult result)
    {
        Assert.That(result.Success, Is.True, result.ErrorMessage);
        Assert.That(result.TargetPath, Is.Not.Null);
        return result.TargetPath!;
    }

    static JsonDocument ReadManifest(string snapshot) =>
        JsonDocument.Parse(File.ReadAllText(Path.Combine(snapshot, "manifest.json")));

    static void ReplaceManifestValue(string snapshot, string property, JsonNode? value)
    {
        var path = Path.Combine(snapshot, "manifest.json");
        var node = JsonNode.Parse(File.ReadAllText(path))!;
        node[property] = value;
        File.WriteAllText(path, node.ToJsonString());
    }

    static void CreateDirectoryLink(string link, string target)
    {
        try
        {
            Directory.CreateSymbolicLink(link, target);
        }
        catch (IOException) when (OperatingSystem.IsWindows())
        {
            // Junctions have the same reparse-point attribute and require no symlink privilege on Windows.
            using var process = Process.Start(new ProcessStartInfo("cmd.exe", $"/d /c mklink /J \"{link}\" \"{target}\"")
            {
                CreateNoWindow = true,
                UseShellExecute = false,
            })!;
            process.WaitForExit();
            Assert.That(process.ExitCode, Is.Zero, "Could not create a junction for the reparse-point test.");
        }
    }

    sealed class InlineProgress<T> : IProgress<T>
    {
        public List<T> Updates { get; } = [];

        public void Report(T value) => Updates.Add(value);
    }
}
