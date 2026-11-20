using System.Globalization;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace BD.WTTS.Client.Tools.Publish.Helpers;

static class NativeAotPublisher
{
    const string Manifest = "native/.wtts-native-manifest.json";
    const string Accelerator = "modules/Accelerator/Steam++.Accelerator.exe";

    internal static void Publish(string[] rids, bool archive, bool msix, string? output, NsisInstaller? installer, string? thumbprint)
    {
        var root = ProjectUtils.ProjPath;
        if (string.IsNullOrEmpty(root))
            throw new DirectoryNotFoundException("Run the publish tool from its SteamTools checkout.");
        if (!OperatingSystem.IsWindows())
        {
            foreach (var rid in rids)
                PublishPayload(root, rid, output is null ? null : Path.GetFullPath(Path.Combine(output, rid), root));
            return;
        }
        var outputRoot = Path.GetFullPath(output ?? "build/publish/aot-packages", root);
        var exe = installer is not null;
        var runId = $"{DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff", CultureInfo.InvariantCulture)}-{Guid.NewGuid():N}";

        foreach (var rid in rids)
        {
            var ridRoot = Path.Combine(outputRoot, rid);
            var work = Directory.CreateDirectory(Path.Combine(ridRoot, $".staging-{runId}")).FullName;
            var release = Directory.CreateDirectory(Path.Combine(work, "release")).FullName;
            var payload = Path.Combine(work, "payload");
            try
            {
                PublishPayload(root, rid, payload);
                var files = ReadPayload(payload, rid);
                var stem = $"Steam++_v{AssemblyInfo.InformationalVersion}_{rid.Replace('-', '_')}_aot";
                var installerStem = $"Steam++_v{AssemblyInfo.InformationalVersion}_{rid.Replace('-', '_')}";
                var archivePath = Path.Combine(work, installerStem + ".7z");
                if (archive || exe)
                    ICompressedPackageCommand.CreateSevenZipPack(archivePath, files);
                if (installer is not null)
                {
                    var setup = Path.Combine(release, installerStem + ".exe");
                    installer.Build(archivePath, setup, AssemblyInfo.FileVersion);
                    if (thumbprint is not null)
                        MSIXHelper.SignTool.SignWithThumbprint(setup, thumbprint);
                }
                if (archive)
                    File.Move(archivePath, Path.Combine(release, stem + ".7z"));
                if (msix)
                {
                    var package = Path.Combine(release, stem + ".msix");
                    BuildMsix(work, package, rid, files);
                    if (thumbprint is not null)
                        MSIXHelper.SignTool.SignWithThumbprint(package, thumbprint);
                    else
                        Console.WriteLine("MSIX is unsigned. Sign it with a certificate trusted by the target device before installing.");
                }

                using (var hashes = File.CreateText(Path.Combine(release, "SHA256SUMS.txt")))
                {
                    foreach (var file in Directory.EnumerateFiles(release).Where(file => Path.GetFileName(file) != "SHA256SUMS.txt").Order(StringComparer.Ordinal))
                        hashes.WriteLine($"{GetSha256(file)}  {Path.GetFileName(file)}");
                }
                var destination = Path.Combine(ridRoot, runId);
                Directory.Move(release, destination);
                Console.WriteLine($"Native AOT packages ({rid}): {destination}");
            }
            finally
            {
                try { Directory.Delete(work, recursive: true); }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
                {
                    Console.Error.WriteLine($"Could not remove packaging staging directory {work}: {ex.Message}");
                }
            }
        }
    }

    static AppPublishFileInfo[] ReadPayload(string directory, string rid)
    {
        var paths = Directory.EnumerateFiles(directory, "*", new EnumerationOptions
        {
            RecurseSubdirectories = true,
            AttributesToSkip = FileAttributes.ReparsePoint,
        }).ToDictionary(path => Path.GetRelativePath(directory, path).Replace('\\', '/'), StringComparer.OrdinalIgnoreCase);
        if (!paths.Remove(Manifest, out var manifestPath))
            throw new InvalidDataException("Native AOT payload manifest is missing.");

        using var manifest = JsonDocument.Parse(File.ReadAllText(manifestPath));
        var info = manifest.RootElement;
        if (info.GetProperty("Schema").GetInt32() != 1 || info.GetProperty("RuntimeIdentifier").GetString() != rid ||
            !paths.ContainsKey("Steam++.exe") || !paths.ContainsKey(Accelerator) || !paths.ContainsKey($"native/{rid}/e_sqlite3.dll"))
            throw new InvalidDataException($"Incomplete or incompatible Native AOT payload for {rid}.");

        var entries = info.GetProperty("Files");
        var files = new AppPublishFileInfo[entries.GetArrayLength() + 1];
        files[0] = new() { FilePath = manifestPath, RelativePath = Manifest };
        int i = 1;
        foreach (var entry in entries.EnumerateArray())
        {
            var name = entry.GetProperty("Path").GetString();
            if (name is null || !paths.Remove(name, out var path) ||
                !string.Equals(GetSha256(path), entry.GetProperty("Sha256").GetString(), StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException($"Native AOT payload file is missing or changed: {name}");
            files[i++] = new() { FilePath = path, RelativePath = name };
        }
        if (paths.Count != 0)
            throw new InvalidDataException("Unexpected files in the Native AOT payload.");
        return files;
    }

    static string GetSha256(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream));
    }

    static void BuildMsix(string work, string output, string rid, AppPublishFileInfo[] files)
    {
        var metadata = Directory.CreateDirectory(Path.Combine(work, "msix")).FullName;
        var architecture = rid == "win-arm64" ? Architecture.Arm64 : Architecture.X64;
        MSIXHelper.MakeAppx.GenerateAppxManifestXml(metadata, AssemblyInfo.FileVersion, architecture);
        MSIXHelper.MakePri.Start(metadata);
        var mapping = Path.Combine(work, "msix-files.txt");
        using (var writer = new StreamWriter(mapping, false, Encoding.Unicode))
        {
            writer.WriteLine("[Files]");
            foreach (var file in files)
                writer.WriteLine($"\"{file.FilePath}\" \"{file.RelativePath.Replace('/', '\\')}\"");
            foreach (var file in Directory.EnumerateFiles(metadata, "*", SearchOption.AllDirectories))
                writer.WriteLine($"\"{file}\" \"{Path.GetRelativePath(metadata, file)}\"");
        }
        MSIXHelper.MakeAppx.PackMapped(output, mapping);
    }

    static void PublishPayload(string directory, string rid, string? output)
    {
        var start = new ProcessStartInfo(OperatingSystem.IsWindows() ? "pwsh.exe" : "pwsh")
        {
            WorkingDirectory = directory,
            UseShellExecute = false,
        };
        if (OperatingSystem.IsWindows())
        {
            start.ArgumentList.Add("-ExecutionPolicy");
            start.ArgumentList.Add("Bypass");
        }
        foreach (var argument in new[] { "-NoProfile", "-NonInteractive", "-File", Path.Combine(directory, "build/publish-native.ps1"), "-RuntimeIdentifier", rid })
            start.ArgumentList.Add(argument);
        if (output is not null)
        {
            start.ArgumentList.Add("-OutputDirectory");
            start.ArgumentList.Add(output);
        }
        ProcessHelper.StartAndWaitForExit(start);
    }
}
