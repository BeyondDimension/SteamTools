namespace BD.WTTS.Services;

readonly record struct ShaderCacheStatistics(long Bytes, long Files, DateTime? LastWriteUtc,
    long SkippedFiles, long SkippedDirectories)
{
    public bool IsKnownEmpty => LastWriteUtc.HasValue && Files == 0 && SkippedFiles == 0 && SkippedDirectories == 0;
}

sealed class ShaderCacheFileStore(Action<string, Exception> reportError)
{
    public static StringComparer PathComparer => OperatingSystem.IsWindows()
        ? StringComparer.OrdinalIgnoreCase
        : StringComparer.Ordinal;

    public static string CanonicalPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !Path.IsPathFullyQualified(path) ||
            (OperatingSystem.IsWindows() && (path.StartsWith(@"\\?\", StringComparison.Ordinal) ||
                path.StartsWith(@"\\.\", StringComparison.Ordinal))))
            throw new IOException("Expected an absolute filesystem path.");

        var fullPath = Path.TrimEndingDirectorySeparator(Path.GetFullPath(path));
        var root = Path.GetPathRoot(fullPath)!;
        if (PathComparer.Equals(fullPath, root) ||
            (OperatingSystem.IsWindows() && fullPath.AsSpan(root.Length).Contains(':')))
            throw new IOException("Expected a non-root directory or file path.");
        return fullPath;
    }

    public static bool IsWithin(string path, string root) =>
        path.Length > root.Length &&
        (path[root.Length] == Path.DirectorySeparatorChar || path[root.Length] == Path.AltDirectorySeparatorChar) &&
        PathComparer.Equals(path[..root.Length], root);

    public static string GetCacheDirectoryPath(ShaderCacheLocation location)
    {
        var root = CanonicalPath(location.Root);
        var target = CanonicalPath(location.Path);
        if (!PathComparer.Equals(target, root) && !IsWithin(target, root))
            throw new IOException("The cache is outside its declared root.");
        if (location.Kind is ShaderCacheKind.GameClient or ShaderCacheKind.Game && !PathComparer.Equals(target, root))
            throw new IOException("Expected the exact declared game cache directory.");
        if (location.Kind == ShaderCacheKind.SteamGame &&
            (!PathComparer.Equals(Path.GetDirectoryName(target), root) ||
                !PathComparer.Equals(Path.GetFileName(root), "shadercache") ||
                location.AppId is null or 0 ||
                !uint.TryParse(Path.GetFileName(target), out var appId) || appId != location.AppId))
            throw new IOException("Expected a Steam app ID directly under steamapps/shadercache.");
        CheckPath(target, directory: true);
        return target;
    }

    public string[] FindDirectories(string parent, string pattern, CancellationToken token)
    {
        var result = new List<string>();
        try
        {
            parent = CanonicalPath(parent);
            CheckPath(parent, directory: true);
            foreach (var path in Directory.EnumerateDirectories(parent, pattern))
            {
                token.ThrowIfCancellationRequested();
                result.Add(path);
            }
        }
        catch (Exception ex) when (ex is DirectoryNotFoundException or FileNotFoundException)
        {
            // Most optional caches will be missing on any particular machine.
        }
        catch (Exception ex) when (IsFileError(ex))
        {
            reportError(parent, ex);
        }
        return result.ToArray();
    }

    public ShaderCacheStatistics Scan(ShaderCacheLocation location, CancellationToken token) =>
        Inspect(location, delete: false, preview: null, token);

    public ShaderCacheStatistics Clean(ShaderCacheLocation location, CancellationToken token) =>
        Inspect(location, delete: true, preview: null, token);

    public ShaderCacheStatistics Preview(ShaderCacheLocation location, Action<string> addPath, CancellationToken token) =>
        Inspect(location, delete: false, preview: addPath, token);

    ShaderCacheStatistics Inspect(ShaderCacheLocation location, bool delete, Action<string>? preview, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        string target;
        try
        {
            target = GetCacheDirectoryPath(location);
        }
        catch (Exception ex) when (IsFileError(ex))
        {
            reportError(location.Path, ex);
            return new(0, 0, null, 0, 1);
        }

        long bytes = 0, files = 0, skippedFiles = 0, skippedDirectories = 0;
        DateTime? lastWrite = null;
        var changing = delete || preview != null;
        var incomplete = new HashSet<string>(PathComparer);
        var pending = new Stack<(string Path, bool FinishedDirectory)>();
        pending.Push((target, false));
        while (pending.TryPop(out var item))
        {
            token.ThrowIfCancellationRequested();
            var path = item.Path;
            var directory = item.FinishedDirectory;
            try
            {
                if (item.FinishedDirectory)
                {
                    if (incomplete.Remove(path))
                    {
                        MarkParentIncomplete(path);
                        continue;
                    }
                    if (PathComparer.Equals(path, target) && location.Kind != ShaderCacheKind.SteamGame)
                        continue;

                    // Never use recursive deletion. All children are handled and checked individually.
                    if (delete)
                    {
                        CheckPath(path, directory: true);
                        Directory.Delete(path);
                    }
                    preview?.Invoke(path + Path.DirectorySeparatorChar);
                    continue;
                }

                var canonical = CanonicalPath(path);
                if (!PathComparer.Equals(canonical, target) && !IsWithin(canonical, target))
                    throw new IOException("The entry is outside its cache.");

                var attributes = File.GetAttributes(canonical);
                directory = attributes.HasFlag(FileAttributes.Directory);
                CheckAttributes(attributes);
                if (directory)
                {
                    CheckPath(canonical, directory: true);
                    UpdateLastWrite(Directory.GetLastWriteTimeUtc(canonical));
                    if (changing)
                        pending.Push((canonical, true));

                    foreach (var child in Directory.EnumerateFileSystemEntries(canonical))
                    {
                        token.ThrowIfCancellationRequested();
                        pending.Push((child, false));
                    }
                    continue;
                }

                var file = new FileInfo(canonical);
                var length = file.Length;
                UpdateLastWrite(file.LastWriteTimeUtc);
                if (delete)
                {
                    CheckPath(canonical, directory: false);
                    File.Delete(canonical);
                }
                preview?.Invoke(canonical);
                bytes += length;
                files++;
            }
            catch (Exception ex) when (IsFileError(ex))
            {
                if (directory)
                    skippedDirectories++;
                else
                    skippedFiles++;
                if (changing)
                {
                    if (directory && !item.FinishedDirectory)
                        incomplete.Add(path);
                    MarkParentIncomplete(path);
                }
                reportError(path, ex);
            }
        }

        return new(bytes, files, lastWrite, skippedFiles, skippedDirectories);

        void MarkParentIncomplete(string path)
        {
            if (!PathComparer.Equals(path, target) && Path.GetDirectoryName(path) is { } parent)
                incomplete.Add(parent);
        }

        void UpdateLastWrite(DateTime value)
        {
            if (lastWrite == null || value > lastWrite)
                lastWrite = value;
        }
    }

    static void CheckPath(string path, bool directory)
    {
        // Checking each ancestor blocks links in both the cache root and its parents.
        var parent = Path.GetDirectoryName(path);
        if (directory)
            parent = path;

        while (parent != null)
        {
            var attributes = File.GetAttributes(parent);
            CheckAttributes(attributes);
            if (!attributes.HasFlag(FileAttributes.Directory))
                throw new IOException("An ancestor of the cache is not a directory.");
            parent = Path.GetDirectoryName(parent);
        }

        if (!directory)
        {
            var attributes = File.GetAttributes(path);
            CheckAttributes(attributes);
            if (attributes.HasFlag(FileAttributes.Directory))
                throw new IOException("The cache file has become a directory.");
        }
    }

    static void CheckAttributes(FileAttributes attributes)
    {
        if (attributes.HasFlag(FileAttributes.ReparsePoint))
            throw new IOException("Symbolic links and reparse points are skipped.");
    }

    static bool IsFileError(Exception ex) =>
        ex is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException
            or System.Security.SecurityException;
}
