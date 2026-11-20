using System.Text;

namespace BD.WTTS.Logging;

/// <summary>Discovers recent logs in configured locations without creating directories.</summary>
public static class LogFileReader
{
    public const int MaxTailBytes = 256 * 1024;
    const int MaxFiles = 100;
    const int MaxEntriesPerDirectory = 4096;
    const FileAttributes ExcludedAttributes = FileAttributes.ReparsePoint | FileAttributes.Device;

    public sealed class LogFile
    {
        internal LogFile(string path, string displayPath, DateTime lastWriteTimeUtc)
        {
            Path = path;
            DisplayPath = displayPath;
            LastWriteTimeUtc = lastWriteTimeUtc;
        }

        internal string Path { get; }
        public string DisplayPath { get; }
        internal DateTime LastWriteTimeUtc { get; }

        public override string ToString() => DisplayPath;
    }

    public static Task<(LogFile[] Files, bool HadReadError)> DiscoverAsync(
        string logRoot, string? asfLogRoot, CancellationToken cancellationToken, bool includeModuleDirectories = true)
        => Task.Run(() => Discover(logRoot, asfLogRoot, includeModuleDirectories, cancellationToken), cancellationToken);

    static (LogFile[] Files, bool HadReadError) Discover(
        string logRoot, string? asfLogRoot, bool includeModuleDirectories, CancellationToken cancellationToken)
    {
        var files = new List<LogFile>();
        var visited = new HashSet<string>(OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);
        var hadReadError = false;
        if (Path.IsPathFullyQualified(logRoot))
        {
            Scan(logRoot, string.Empty, false);
            if (includeModuleDirectories)
            {
                Scan(Path.Combine(logRoot, AssemblyInfo.Accelerator), AssemblyInfo.Accelerator, false);
                Scan(Path.Combine(logRoot, "Administrator"), "Administrator", false);
            }
        }
        if (!string.IsNullOrWhiteSpace(asfLogRoot) && Path.IsPathFullyQualified(asfLogRoot))
        {
            Scan(asfLogRoot, "ASF", true);
            Scan(Path.Combine(asfLogRoot, "logs"), Path.Combine("ASF", "logs"), true);
        }

        return (files.OrderByDescending(static file => file.LastWriteTimeUtc)
            .ThenBy(static file => file.DisplayPath, StringComparer.Ordinal)
            .Take(MaxFiles).ToArray(), hadReadError);

        void Scan(string directory, string displayDirectory, bool asf)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                directory = Path.TrimEndingDirectorySeparator(Path.GetFullPath(directory));
                if (!visited.Add(directory) || !IsPlainDirectory(directory))
                    return;

                foreach (var entry in new DirectoryInfo(directory).EnumerateFileSystemInfos().Take(MaxEntriesPerDirectory))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (IsLogName(entry.Name, asf) &&
                        (entry.Attributes & (ExcludedAttributes | FileAttributes.Directory)) == 0)
                    {
                        files.Add(new LogFile(entry.FullName, Path.Combine(displayDirectory, entry.Name), entry.LastWriteTimeUtc));
                    }
                }
            }
            catch (Exception ex) when (ex is FileNotFoundException or DirectoryNotFoundException)
            {
                // Optional sources may not exist yet, or may disappear during rotation.
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
            {
                hadReadError = true;
            }
        }
    }

    static bool IsLogName(string name, bool asf)
    {
        if (name.EndsWith(".log", StringComparison.OrdinalIgnoreCase))
            return true;
        if (!asf)
            return false;
        if (name.Equals("log.txt", StringComparison.OrdinalIgnoreCase))
            return true;
        return name.Length > 8 && name.StartsWith("log.", StringComparison.OrdinalIgnoreCase) &&
            name.EndsWith(".txt", StringComparison.OrdinalIgnoreCase) &&
            name.AsSpan(4, name.Length - 8).IndexOfAnyExceptInRange('0', '9') < 0;
    }

    static bool IsPlainDirectory(string path)
    {
        // Check from the filesystem root downward, so even attribute checks don't cross a link.
        var parent = Path.GetDirectoryName(path);
        return (parent == null || IsPlainDirectory(parent)) &&
            (File.GetAttributes(path) & (FileAttributes.Directory | ExcludedAttributes)) == FileAttributes.Directory;
    }

    public static Task<(string Text, bool IsTail)> ReadTailAsync(LogFile file, CancellationToken cancellationToken)
        => Task.Run(async () =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!IsPlainDirectory(Path.GetDirectoryName(file.Path)!) ||
                (File.GetAttributes(file.Path) & (ExcludedAttributes | FileAttributes.Directory)) != 0)
                throw new IOException("The log path is no longer a regular file in a log directory.");

            await using var stream = new FileStream(file.Path, FileMode.Open, FileAccess.Read,
                FileShare.ReadWrite | FileShare.Delete, bufferSize: 1, useAsync: true);
            var length = stream.Length;
            var size = (int)Math.Min(length, MaxTailBytes);
            if (size == 0)
                return (string.Empty, false);

            var offset = length - size;
            stream.Position = offset;
            var bytes = new byte[size];
            var count = 0;
            while (count < size)
            {
                var read = await stream.ReadAsync(bytes.AsMemory(count), cancellationToken).ConfigureAwait(false);
                if (read == 0)
                    break;
                count += read;
            }

            var start = 0;
            if (offset != 0)
            {
                // A bounded tail may start mid-codepoint. Skip continuation bytes at the cut.
                while (start < count && (bytes[start] & 0xC0) == 0x80)
                    start++;
            }
            else if (bytes.AsSpan(0, count).StartsWith(Encoding.UTF8.Preamble))
            {
                start = Encoding.UTF8.Preamble.Length;
            }

            // A writer may not have finished the last codepoint when Length was sampled.
            // A non-flushing decoder retains only that incomplete suffix.
            var decoder = Encoding.UTF8.GetDecoder();
            var charCount = decoder.GetCharCount(bytes, start, count - start, flush: false);
            var text = string.Create(charCount, (decoder, bytes, start, count), static (chars, state) =>
                state.decoder.GetChars(state.bytes.AsSpan(state.start, state.count - state.start), chars, flush: false));
            return (text, offset != 0);
        }, cancellationToken);
}
