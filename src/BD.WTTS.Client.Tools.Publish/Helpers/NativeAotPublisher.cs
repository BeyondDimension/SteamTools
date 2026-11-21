using System.ComponentModel;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace BD.WTTS.Client.Tools.Publish.Helpers;

static class NativeAotPublisher
{
    const string Accelerator = "modules/Accelerator/Steam++.Accelerator.exe";

    internal static void Publish(string[] rids, string[] formats, string? output, string? makensis, string? thumbprint)
    {
        if (!OperatingSystem.IsWindows())
            throw new PlatformNotSupportedException("Native AOT EXE, 7z and MSIX packaging runs on Windows. Use build/publish-native.ps1 on Linux or macOS.");
        if (rids.Length == 0 || rids.Any(rid => rid is not ("win-x64" or "win-arm64")))
            throw new ArgumentException("Specify win-x64 or win-arm64 for --rids.");
        if (formats.Length == 0 || formats.Any(format => format is not ("7z" or "exe" or "msix")))
            throw new ArgumentException("Specify 7z, exe or msix for --formats.");

        bool archive = formats.Contains("7z"), installer = formats.Contains("exe"), msix = formats.Contains("msix");
        var root = ProjectUtils.ProjPath;
        if (string.IsNullOrEmpty(root))
            throw new DirectoryNotFoundException("Run the publish tool from its SteamTools checkout.");
        var outputRoot = Path.GetFullPath(output ?? "build/publish/aot-packages", root);
        var nsis = installer ? ResolveMakeNsis(makensis) : null;
        var publisher = thumbprint is not null && (installer || msix) ? GetSigningPublisher(thumbprint) : null;
        var runId = $"{DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff", CultureInfo.InvariantCulture)}-{Guid.NewGuid():N}";

        foreach (var rid in rids.Distinct(StringComparer.Ordinal))
        {
            var ridRoot = Path.Combine(outputRoot, rid);
            var work = Directory.CreateDirectory(Path.Combine(ridRoot, $".staging-{runId}")).FullName;
            var release = Directory.CreateDirectory(Path.Combine(work, "release")).FullName;
            var payload = Path.Combine(work, "payload");
            try
            {
                RunPublishScript(rid, payload);
                var files = ReadPayload(root, payload, rid);
                var stem = $"Steam++_v{AssemblyInfo.InformationalVersion}_{rid.Replace('-', '_')}_aot";
                if (archive)
                    ICompressedPackageCommand.CreateSevenZipPack(Path.Combine(release, stem + ".7z"), files);
                if (installer)
                {
                    var setup = Path.Combine(release, stem + "_setup.exe");
                    BuildInstaller(nsis!, root, work, setup, rid, files);
                    if (publisher is not null)
                        MSIXHelper.SignTool.SignWithThumbprint(setup, thumbprint!);
                }
                if (msix)
                {
                    var package = Path.Combine(release, stem + ".msix");
                    BuildMsix(work, package, rid, files, publisher);
                    if (publisher is not null)
                        MSIXHelper.SignTool.SignWithThumbprint(package, thumbprint!);
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

    static AppPublishFileInfo[] ReadPayload(string root, string directory, string rid)
    {
        var paths = Directory.EnumerateFiles(directory, "*", new EnumerationOptions
        {
            RecurseSubdirectories = true,
            AttributesToSkip = FileAttributes.ReparsePoint,
        }).ToDictionary(path => Path.GetRelativePath(directory, path).Replace('\\', '/'), StringComparer.OrdinalIgnoreCase);
        // Keep the state key in sync with build/publish-native.ps1.
        var canonicalOutput = Path.GetFullPath(directory)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            .ToUpperInvariant();
        var stateKey = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonicalOutput)));
        var manifestPath = Path.Combine(root, "build", "artifacts", "native", rid, "package-state", $"{stateKey}.json");
        if (!File.Exists(manifestPath) || (File.GetAttributes(manifestPath) & FileAttributes.ReparsePoint) != 0)
            throw new InvalidDataException($"Native AOT payload manifest is missing or not a regular file: {manifestPath}");

        using var manifest = JsonDocument.Parse(File.ReadAllText(manifestPath));
        var info = manifest.RootElement;
        if (info.GetProperty("Schema").GetInt32() != 1 || info.GetProperty("RuntimeIdentifier").GetString() != rid ||
            !paths.ContainsKey("Steam++.exe") || !paths.ContainsKey(Accelerator) || !paths.ContainsKey($"native/{rid}/e_sqlite3.dll"))
            throw new InvalidDataException($"Incomplete or incompatible Native AOT payload for {rid}.");

        var entries = info.GetProperty("Files");
        var files = new AppPublishFileInfo[entries.GetArrayLength()];
        int i = 0;
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

    static string ResolveMakeNsis(string? explicitPath)
    {
        if (explicitPath is not null)
        {
            var fullPath = Path.GetFullPath(explicitPath);
            if (File.Exists(fullPath))
                return fullPath;
            throw new FileNotFoundException("MakeNSIS executable does not exist.", fullPath);
        }

        foreach (var folder in new[] { Environment.SpecialFolder.ProgramFilesX86, Environment.SpecialFolder.ProgramFiles })
        {
            var path = Path.Combine(Environment.GetFolderPath(folder), "NSIS", "makensis.exe");
            if (File.Exists(path))
                return path;
        }
        foreach (var folder in (Environment.GetEnvironmentVariable("PATH") ?? string.Empty).Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
        {
            if (Path.IsPathFullyQualified(folder))
            {
                var path = Path.Combine(folder, "makensis.exe");
                if (File.Exists(path))
                    return path;
            }
        }
        throw new FileNotFoundException("Install NSIS or pass --makensis <path to makensis.exe> to build an EXE installer.");
    }

    static string GetSigningPublisher(string thumbprint)
    {
        if (thumbprint.Length != 40 || !thumbprint.All(Uri.IsHexDigit))
            throw new ArgumentException("--sign-thumbprint must be a 40-character hexadecimal certificate thumbprint.");
        using var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
        store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
        var matches = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, validOnly: false);
        try
        {
            if (matches.Count != 1 || !matches[0].HasPrivateKey)
                throw new InvalidOperationException("The signing certificate and its private key must be available in CurrentUser\\My.");
            return matches[0].Subject;
        }
        finally
        {
            foreach (var certificate in matches)
                certificate.Dispose();
        }
    }

    static void BuildInstaller(string makensis, string root, string work, string output, string rid, AppPublishFileInfo[] files)
    {
        static string Encode(string path) => path.Replace("$", "$$", StringComparison.Ordinal);
        using (var include = new StreamWriter(Path.Combine(work, "payload.nsh"), false, new UTF8Encoding(true)))
        {
            include.WriteLine("!macro InstallPayload");
            foreach (var file in files)
            {
                var path = Encode(file.RelativePath.Replace('/', '\\'));
                var folder = Path.GetDirectoryName(path);
                include.WriteLine(string.IsNullOrEmpty(folder) ? "SetOutPath \"$INSTDIR\"" : $"SetOutPath \"$INSTDIR\\{folder}\"");
                include.WriteLine($"File \"payload\\{path}\"");
            }
            include.WriteLine("!macroend");
            include.WriteLine("!macro UninstallPayload");
            var folders = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var file in files)
            {
                var path = Encode(file.RelativePath.Replace('/', '\\'));
                include.WriteLine($"Delete \"$INSTDIR\\{path}\"");
                for (var folder = Path.GetDirectoryName(path); !string.IsNullOrEmpty(folder); folder = Path.GetDirectoryName(folder))
                    folders.Add(folder);
            }
            foreach (var folder in folders.OrderByDescending(path => path.Length).ThenBy(path => path, StringComparer.Ordinal))
                include.WriteLine($"RMDir \"$INSTDIR\\{folder}\"");
            include.WriteLine("!macroend");
        }
        Run(makensis, work, "/NOCD", "/NOCONFIG", "/INPUTCHARSET", "UTF8", "/V2", "/WX",
            $"/DAPP_VERSION={AssemblyInfo.FileVersion}", $"/DAPP_ARCH={rid[4..]}", $"/DOUTPUT={Path.GetRelativePath(work, output)}",
            Path.Combine(root, "packaging/watt-toolkit-aot.nsi"));
    }

    static void BuildMsix(string work, string output, string rid, AppPublishFileInfo[] files, string? publisher)
    {
        return;
        //var metadata = Directory.CreateDirectory(Path.Combine(work, "msix")).FullName;
        //var architecture = rid == "win-arm64" ? Architecture.Arm64 : Architecture.X64;
        //MSIXHelper.MakeAppx.GenerateAppxManifestXml(metadata, AssemblyInfo.FileVersion, architecture, publisher);
        //MSIXHelper.MakePri.Start(metadata);
        //var mapping = Path.Combine(work, "msix-files.txt");
        //using (var writer = new StreamWriter(mapping, false, Encoding.Unicode))
        //{
        //    writer.WriteLine("[Files]");
        //    foreach (var file in files)
        //        writer.WriteLine($"\"{file.FilePath}\" \"{file.RelativePath.Replace('/', '\\')}\"");
        //    foreach (var file in Directory.EnumerateFiles(metadata, "*", SearchOption.AllDirectories))
        //        writer.WriteLine($"\"{file}\" \"{Path.GetRelativePath(metadata, file)}\"");
        //}
        //MSIXHelper.MakeAppx.PackMapped(output, mapping);
    }

    internal static void RunPublishScript(string rid, string directory)
    {
        var root = ProjectUtils.ProjPath;
        if (string.IsNullOrEmpty(root))
            throw new DirectoryNotFoundException("Run the publish tool from its SteamTools checkout.");

        string[] arguments =
        [
            "-NoProfile", "-NonInteractive", "-ExecutionPolicy", "Bypass", "-File",
            Path.Combine(root, "build", "publish-native.ps1"), "-RuntimeIdentifier", rid, "-OutputDirectory", directory,
        ];

        if (!OperatingSystem.IsWindows())
        {
            Run("pwsh", root, arguments);
            return;
        }

        try
        {
            Run("pwsh.exe", root, arguments);
            return;
        }
        catch (Win32Exception ex) when (ex.NativeErrorCode is 2 or 3) // File or path not found.
        {
            // Visual Studio can inherit a PATH from before PowerShell 7 was installed.
        }

        var programFiles = Environment.GetEnvironmentVariable("ProgramW6432") ??
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        var powerShell7 = Path.Combine(programFiles, "PowerShell", "7", "pwsh.exe");
        if (File.Exists(powerShell7))
        {
            Run(powerShell7, root, arguments);
            return;
        }

        // Sysnative bypasses WOW64 redirection because this script needs a 64-bit shell.
        var systemDirectory = Environment.Is64BitOperatingSystem && !Environment.Is64BitProcess
            ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Sysnative")
            : Environment.GetFolderPath(Environment.SpecialFolder.System);
        var windowsPowerShell = Path.Combine(systemDirectory, "WindowsPowerShell", "v1.0", "powershell.exe");
        Run(windowsPowerShell, root, arguments);
    }

    internal static void Run(string executable, string directory, params string[] arguments)
    {
        var start = new ProcessStartInfo(executable) { WorkingDirectory = directory, UseShellExecute = false };
        foreach (var argument in arguments)
            start.ArgumentList.Add(argument);
        ProcessHelper.StartAndWaitForExit(start);
    }
}
