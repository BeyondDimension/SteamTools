using System.Runtime.InteropServices;
using System.Text.Json;

namespace BD.WTTS.Client.Tools.Publish.Helpers;

static class BundledRuntimeHelper
{
    const string CoreFramework = "Microsoft.NETCore.App";
    const string AspNetFramework = "Microsoft.AspNetCore.App";

    readonly record struct FrameworkRequirement(string Name, string Version, string RollForward, bool ApplyPatches);

    sealed record RuntimeVersion(string Name, Version Number, string[] Prerelease) : IComparable<RuntimeVersion>
    {
        public int CompareTo(RuntimeVersion? other)
        {
            if (other is null) return 1;
            var comparison = Number.CompareTo(other.Number);
            if (comparison != 0) return comparison;
            if (Prerelease.Length == 0) return other.Prerelease.Length == 0 ? 0 : 1;
            if (other.Prerelease.Length == 0) return -1;
            for (var i = 0; i < Math.Min(Prerelease.Length, other.Prerelease.Length); i++)
            {
                var leftNumeric = ulong.TryParse(Prerelease[i], out var left);
                var rightNumeric = ulong.TryParse(other.Prerelease[i], out var right);
                comparison = leftNumeric && rightNumeric ? left.CompareTo(right) :
                    leftNumeric != rightNumeric ? (leftNumeric ? -1 : 1) :
                    string.CompareOrdinal(Prerelease[i], other.Prerelease[i]);
                if (comparison != 0) return comparison;
            }
            return Prerelease.Length.CompareTo(other.Prerelease.Length);
        }
    }

    internal static void Copy(string rootPublishDir, Platform platform, Architecture architecture)
    {
        var isWindows = platform is Platform.Windows or Platform.UWP or Platform.WinUI;
        if (!isWindows && platform != Platform.Linux) return;

        // The Windows Arm64 package runs the main application as x64. Its Arm64 proxy is self-contained.
        var runtimeArchitecture = isWindows && architecture == Architecture.Arm64 ? Architecture.X64 : architecture;
        var requirements = EnumerateRuntimeConfigs(rootPublishDir).SelectMany(ReadRequirements).ToArray();
        if (requirements.Length == 0) return; // includedFrameworks belong to self-contained executables, not this runtime.

        var errors = new List<Exception>();
        var sourceRoots = isWindows ? GetWindowsRoots(runtimeArchitecture) : GetLinuxRoots(rootPublishDir, runtimeArchitecture);
        foreach (var sourceRoot in sourceRoots.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            try
            {
                // Resolve and validate everything before copying; an incomplete installation must not produce a partial bundle.
                var directories = GetFrameworkClosure(sourceRoot, requirements, isWindows, runtimeArchitecture);
                var destinationRoot = Path.Combine(rootPublishDir, "dotnet");
                foreach (var directory in directories)
                    CopyDirectory(directory, Path.Combine(destinationRoot, Path.GetRelativePath(sourceRoot, directory)), true);
                if (!isWindows)
                {
                    // Linux launch scripts invoke the muxer. Do not copy SDKs, packs, or unrelated shared frameworks.
                    foreach (var file in Directory.EnumerateFiles(sourceRoot))
                        File.Copy(file, Path.Combine(destinationRoot, Path.GetFileName(file)), true);
                }
                return;
            }
            catch (Exception exception) when (exception is DirectoryNotFoundException or FileNotFoundException or BadImageFormatException)
            {
                errors.Add(exception);
            }
        }
        throw new AggregateException($"No complete {runtimeArchitecture} runtime installation satisfies the published runtimeconfig files.", errors);
    }

    static IEnumerable<string> EnumerateRuntimeConfigs(string root)
    {
        foreach (var file in Directory.EnumerateFiles(root, "*.runtimeconfig.json"))
            yield return file;
        foreach (var directory in Directory.EnumerateDirectories(root))
        {
            if (string.Equals(Path.GetFileName(directory), "dotnet", StringComparison.OrdinalIgnoreCase)) continue;
            foreach (var file in Directory.EnumerateFiles(directory, "*.runtimeconfig.json", SearchOption.AllDirectories))
                yield return file;
        }
    }

    static List<FrameworkRequirement> ReadRequirements(string path)
    {
        using var stream = File.OpenRead(path);
        using var document = JsonDocument.Parse(stream);
        var requirements = new List<FrameworkRequirement>();
        if (!document.RootElement.TryGetProperty("runtimeOptions", out var options)) return requirements;
        if (options.TryGetProperty("framework", out var framework)) Add(framework);
        if (options.TryGetProperty("frameworks", out var frameworks))
            foreach (var item in frameworks.EnumerateArray()) Add(item);
        return requirements;

        void Add(JsonElement item)
        {
            var name = item.GetProperty("name").GetString()!;
            var version = item.GetProperty("version").GetString()!;
            if (string.IsNullOrWhiteSpace(name) || name != Path.GetFileName(name))
                throw new InvalidDataException($"Invalid framework name in {path}.");
            var rollForward = GetProperty("rollForward")?.GetString();
            if (rollForward is null)
            {
                rollForward = GetProperty("rollForwardOnNoCandidateFx")?.GetInt32() switch
                {
                    0 => "LatestPatch",
                    2 => "Major",
                    _ => "Minor",
                };
            }
            requirements.Add(new(name, version, rollForward, GetProperty("applyPatches")?.GetBoolean() ?? true));

            JsonElement? GetProperty(string property) => item.TryGetProperty(property, out var value) ? value :
                options.TryGetProperty(property, out value) ? value : null;
        }
    }

    static HashSet<string> GetFrameworkClosure(string sourceRoot, FrameworkRequirement[] requirements, bool isWindows, Architecture architecture)
    {
        var directories = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var queue = new Queue<FrameworkRequirement>(requirements);
        while (queue.TryDequeue(out var requirement))
        {
            var frameworkRoot = Path.Combine(sourceRoot, "shared", requirement.Name);
            var version = ResolveVersion(frameworkRoot, requirement);
            var directory = Path.Combine(frameworkRoot, version.Name);
            if (!directories.Add(directory)) continue;

            var config = Path.Combine(directory, requirement.Name + ".runtimeconfig.json");
            if (File.Exists(config))
                foreach (var dependency in ReadRequirements(config)) queue.Enqueue(dependency);

            if (requirement.Name == CoreFramework)
            {
                var hostDirectory = Path.Combine(sourceRoot, "host", "fxr", version.Name);
                var hostPath = Path.Combine(hostDirectory, isWindows ? "hostfxr.dll" : "libhostfxr.so");
                if (!File.Exists(hostPath)) throw new FileNotFoundException("Matching hostfxr is required by the launcher.", hostPath);
                if (isWindows) ValidateWindowsArchitecture(hostPath, architecture);
                directories.Add(hostDirectory);
                if (isWindows)
                {
                    // The existing native/.NET Framework launcher explicitly requires AspNet alongside Core at the same version.
                    queue.Enqueue(new(AspNetFramework, version.Name, "Disable", true));
                }
            }
        }
        return directories;
    }

    static RuntimeVersion ResolveVersion(string frameworkRoot, FrameworkRequirement requirement)
    {
        var requested = ParseVersion(requirement.Version) ?? throw new InvalidDataException($"Invalid runtime version: {requirement.Version}");
        var candidates = Directory.EnumerateDirectories(frameworkRoot)
            .Select(path => ParseVersion(Path.GetFileName(path)))
            .OfType<RuntimeVersion>()
            .Where(candidate => IsCompatible(candidate, requested, requirement))
            .ToArray();
        IOrderedEnumerable<RuntimeVersion> ordered = requirement.RollForward switch
        {
            "Minor" => candidates.OrderBy(version => version.Number.Minor).ThenByDescending(version => version),
            "Major" => candidates.OrderBy(version => version.Number.Major).ThenBy(version => version.Number.Minor).ThenByDescending(version => version),
            _ => candidates.OrderByDescending(version => version),
        };
        return ordered.FirstOrDefault() ?? throw new DirectoryNotFoundException(
            $"No {requirement.Name} version satisfies {requirement.Version} ({requirement.RollForward}) in {frameworkRoot}.");
    }

    static bool IsCompatible(RuntimeVersion candidate, RuntimeVersion requested, FrameworkRequirement requirement)
    {
        if (requirement.RollForward == "Disable") return candidate.Name == requested.Name;
        if (candidate.CompareTo(requested) < 0 || (requested.Prerelease.Length == 0 && candidate.Prerelease.Length != 0)) return false;
        var sameMajor = candidate.Number.Major == requested.Number.Major;
        var sameMinor = sameMajor && candidate.Number.Minor == requested.Number.Minor;
        if (!requirement.ApplyPatches && sameMinor && candidate.Number.Build != requested.Number.Build) return false;
        return requirement.RollForward switch
        {
            "LatestPatch" => sameMinor,
            "Minor" or "LatestMinor" => sameMajor,
            "Major" or "LatestMajor" => true,
            _ => throw new InvalidDataException($"Unsupported runtime rollForward policy: {requirement.RollForward}"),
        };
    }

    static RuntimeVersion? ParseVersion(string name)
    {
        var value = name.Split('+', 2)[0].Split('-', 2);
        return Version.TryParse(value[0], out var version) ?
            new(name, version, value.Length == 1 ? [] : value[1].Split('.')) : null;
    }

    static IEnumerable<string> GetLinuxRoots(string packagePath, Architecture architecture)
    {
        yield return Path.GetFullPath(Path.Combine(packagePath, "..", "dotnet-Runtime", $"{Platform.Linux}-{architecture}"));
        if (OperatingSystem.IsLinux() && RuntimeInformation.ProcessArchitecture == architecture)
            yield return Path.GetFullPath(Path.Combine(RuntimeEnvironment.GetRuntimeDirectory(), "..", "..", ".."));
    }

    static IEnumerable<string> GetWindowsRoots(Architecture architecture)
    {
        var configuredRoot = Environment.GetEnvironmentVariable($"DOTNET_ROOT_{ArchToString(architecture).ToUpperInvariant()}");
        if (!string.IsNullOrWhiteSpace(configuredRoot)) yield return configuredRoot;
        if (RuntimeInformation.ProcessArchitecture == architecture)
            yield return Path.GetFullPath(Path.Combine(RuntimeEnvironment.GetRuntimeDirectory(), "..", "..", ".."));
        var programFiles = architecture == Architecture.X86 ?
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86) :
            Environment.GetEnvironmentVariable("ProgramW6432") ?? Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        var root = Path.Combine(programFiles, "dotnet");
        if (architecture == Architecture.X64 && RuntimeInformation.OSArchitecture == Architecture.Arm64)
            yield return Path.Combine(root, "x64");
        yield return root;
    }

    static void ValidateWindowsArchitecture(string path, Architecture architecture)
    {
        using var stream = File.OpenRead(path);
        using var reader = new BinaryReader(stream);
        if (reader.ReadUInt16() != 0x5a4d) throw new BadImageFormatException($"Invalid runtime host: {path}");
        stream.Position = 0x3c;
        stream.Position = reader.ReadUInt32();
        if (reader.ReadUInt32() != 0x00004550) throw new BadImageFormatException($"Invalid runtime host: {path}");
        var expectedMachine = architecture switch
        {
            Architecture.X86 => 0x014c,
            Architecture.X64 => 0x8664,
            Architecture.Arm64 => 0xaa64,
            _ => throw new PlatformNotSupportedException($"Unsupported Windows runtime architecture: {architecture}"),
        };
        if (reader.ReadUInt16() != expectedMachine)
            throw new BadImageFormatException($"Runtime host architecture does not match {architecture}: {path}");
    }
}
