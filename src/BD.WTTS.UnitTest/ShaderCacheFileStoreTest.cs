using BD.WTTS.Services;

namespace BD.WTTS.UnitTest;

[TestFixture]
public sealed class ShaderCacheFileStoreTest
{
    DirectoryInfo temp = null!;
    ShaderCacheFileStore files = null!;

    [SetUp]
    public void SetUp()
    {
        temp = Directory.CreateTempSubdirectory("wtts-shader-cache-");
        files = new((_, _) => { });
    }

    [TearDown]
    public void TearDown() => temp.Delete(recursive: true);

    [Test]
    public void PreviewListsContentsAndCleanKeepsDriverFolder()
    {
        var cache = Directory.CreateDirectory(Path.Combine(temp.FullName, "DXCache")).FullName;
        var child = Directory.CreateDirectory(Path.Combine(cache, "nested")).FullName;
        var file = Path.Combine(child, "shader.bin");
        File.WriteAllBytes(file, new byte[64]);
        var location = new ShaderCacheLocation(cache, cache, ShaderCacheKind.Driver, "NVIDIA");
        var paths = new List<string>();

        var scan = files.Scan(location, default);
        var preview = files.Preview(location, paths.Add, default);

        Assert.That(scan.Files, Is.EqualTo(1));
        Assert.That(scan.LastWriteUtc, Is.Not.Null);
        Assert.That(preview.Bytes, Is.EqualTo(64));
        Assert.That(paths, Is.EqualTo(new[] { file, child + Path.DirectorySeparatorChar }));
        Assert.That(File.Exists(file), Is.True);

        var result = files.Clean(location, default);

        Assert.That(result.Bytes, Is.EqualTo(64));
        Assert.That(Directory.Exists(cache), Is.True);
        Assert.That(Directory.EnumerateFileSystemEntries(cache), Is.Empty);
    }

    [Test]
    public void SteamDiscoveryAndCleanupOnlyTouchNumericAppFolders()
    {
        var steamApps = Path.Combine(temp.FullName, "steamapps");
        var root = Path.Combine(steamApps, "shadercache");
        var app = Directory.CreateDirectory(Path.Combine(root, "42")).FullName;
        var unrelated = Directory.CreateDirectory(Path.Combine(root, "other")).FullName;
        var steamTemporary = Directory.CreateDirectory(Path.Combine(temp.FullName, "shader_cache_temp_dir_1")).FullName;
        File.WriteAllBytes(Path.Combine(app, "shader.bin"), new byte[12]);
        var names = new List<string>();

        var discovered = ShaderCacheLocations.FindSteamCaches(files, [temp.FullName], [steamApps], default).ToArray();
        var location = discovered.Single(x => x.Kind == ShaderCacheKind.SteamGame);
        files.Preview(location, names.Add, default);

        Assert.That(discovered.Single(x => x.Kind == ShaderCacheKind.SteamTemporary).Path, Is.EqualTo(steamTemporary));
        Assert.That(location.AppId, Is.EqualTo(42u));
        Assert.That(names, Does.Contain(app + Path.DirectorySeparatorChar));
        Assert.That(Directory.Exists(app), Is.True);

        Assert.That(files.Clean(location, default).Bytes, Is.EqualTo(12));
        Assert.That(Directory.Exists(app), Is.False);
        Assert.That(Directory.Exists(root), Is.True);
        Assert.That(Directory.Exists(unrelated), Is.True);
    }

    [Test]
    public void RefusesSiblingTraversalEmptyPathsAndDriveRoots()
    {
        var cache = Directory.CreateDirectory(Path.Combine(temp.FullName, "cache")).FullName;
        var sibling = Directory.CreateDirectory(Path.Combine(temp.FullName, "cache-other")).FullName;
        var sentinel = Path.Combine(sibling, "leave-me");
        File.WriteAllText(sentinel, "outside");
        var root = Path.GetPathRoot(temp.FullName)!;
        string[] rejected = [sibling, Path.Combine(cache, "..", "cache-other"), string.Empty, root];

        foreach (var path in rejected)
        {
            var result = files.Clean(new(path, cache, ShaderCacheKind.Driver, "cache"), default);
            Assert.That(result.SkippedDirectories, Is.EqualTo(1), path);
            Assert.That(result.Bytes, Is.Zero, path);
        }

        Assert.That(files.Clean(new(cache, root, ShaderCacheKind.Driver, "cache"), default).SkippedDirectories, Is.EqualTo(1));
        Assert.That(File.ReadAllText(sentinel), Is.EqualTo("outside"));
    }

    [Test]
    public void RefusesReparsePointsInsideOrAboveCache()
    {
        var cache = Directory.CreateDirectory(Path.Combine(temp.FullName, "cache")).FullName;
        var outside = Directory.CreateDirectory(Path.Combine(temp.FullName, "outside")).FullName;
        var sentinel = Path.Combine(outside, "leave-me");
        File.WriteAllText(sentinel, "outside");
        var link = Path.Combine(cache, "link");
        if (OperatingSystem.IsWindows())
        {
            using var process = Process.Start(new ProcessStartInfo(Path.Combine(Environment.SystemDirectory, "cmd.exe"))
            {
                Arguments = $"/d /c mklink /j \"{link}\" \"{outside}\"",
                UseShellExecute = false,
                CreateNoWindow = true,
            })!;
            Assert.That(process.WaitForExit(10_000) && process.ExitCode == 0, Is.True, "Test junction creation failed.");
        }
        else
        {
            Directory.CreateSymbolicLink(link, outside);
        }

        try
        {
            var viaLink = Path.Combine(link, "nested");
            Directory.CreateDirectory(Path.Combine(outside, "nested"));

            var location = new ShaderCacheLocation(cache, cache, ShaderCacheKind.Driver, "cache");
            var paths = new List<string>();
            Assert.That(files.Preview(location, paths.Add, default).SkippedDirectories, Is.EqualTo(1));
            Assert.That(paths, Is.Empty);
            Assert.That(files.Clean(location, default).SkippedDirectories, Is.EqualTo(1));
            Assert.That(files.Clean(new(link, cache, ShaderCacheKind.Driver, "link"), default).SkippedDirectories, Is.EqualTo(1));
            Assert.That(files.Clean(new(viaLink, viaLink, ShaderCacheKind.Driver, "nested"), default).SkippedDirectories, Is.EqualTo(1));
            Assert.That(File.ReadAllText(sentinel), Is.EqualTo("outside"));
        }
        finally
        {
            Directory.Delete(link);
        }
    }

    [Test]
    public void RunningScanHonorsCancellation()
    {
        using var cancellation = new CancellationTokenSource();
        var cache = new ShaderCacheLocation(temp.FullName, temp.FullName, ShaderCacheKind.Driver, "cache");
        cancellation.Cancel();

        Assert.Throws<OperationCanceledException>(() => files.Scan(cache, cancellation.Token));
    }

    [Test]
    [Platform("Win")]
    public void LockedWindowsFileIsSkippedAndOtherFilesAreCleaned()
    {
        var cache = temp.FullName;
        var lockedPath = Path.Combine(cache, "locked.bin");
        var otherPath = Path.Combine(cache, "other.bin");
        File.WriteAllBytes(lockedPath, new byte[16]);
        File.WriteAllBytes(otherPath, new byte[32]);
        using var locked = File.Open(lockedPath, FileMode.Open, FileAccess.Read, FileShare.Read);

        var result = files.Clean(new(cache, cache, ShaderCacheKind.Driver, "cache"), default);

        Assert.That(result.SkippedFiles, Is.EqualTo(1));
        Assert.That(result.Bytes, Is.EqualTo(32));
        Assert.That(File.Exists(lockedPath), Is.True);
        Assert.That(File.Exists(otherPath), Is.False);
    }
}
