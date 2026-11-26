using System.IO.Enumeration;

namespace BD.WTTS.GameList.Saves;

internal static class GameSaveFileSystem
{
    public static StringComparer PathComparer { get; } =
        OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;

    public static string CanonicalPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !Path.IsPathFullyQualified(path) || IsDevicePath(path))
            throw new IOException("Expected an absolute filesystem path.");

        var fullPath = Path.TrimEndingDirectorySeparator(Path.GetFullPath(path));
        var root = Path.GetPathRoot(fullPath)!;
        if (PathComparer.Equals(fullPath, root))
            throw new IOException("A filesystem root cannot be used as a save directory.");
        if (OperatingSystem.IsWindows() && !ValidSegments(path[Path.GetPathRoot(path)!.Length..], allowParents: false))
            throw new IOException("The filesystem path contains an invalid Windows path component.");
        return fullPath;
    }

    static bool IsDevicePath(string path) =>
        path.Length >= 4 && path[0] is '/' or '\\' && path[1] is '/' or '\\' &&
        path[2] is '?' or '.' && path[3] is '/' or '\\';

    public static bool IsWithin(string path, string root) =>
        path.Length > root.Length &&
        (path[root.Length] == Path.DirectorySeparatorChar || path[root.Length] == Path.AltDirectorySeparatorChar) &&
        PathComparer.Equals(path[..root.Length], root);

    public static bool IsAtOrWithin(string path, string root) => PathComparer.Equals(path, root) || IsWithin(path, root);

    public static string ResolveRelative(string root, string relative, bool allowParents = false, bool allowRoot = false)
    {
        if (relative.Length == 0 && allowRoot)
            return root;

        if (string.IsNullOrWhiteSpace(relative) || relative[0] is '/' or '\\' ||
            !ValidSegments(relative, allowParents))
            throw new IOException("Expected a safe relative path.");

        var path = CanonicalPath(Path.GetFullPath(Path.Combine(root, NormalizeSeparators(relative))));
        if (!(allowRoot ? IsAtOrWithin(path, root) : IsWithin(path, root)))
            throw new IOException("The path is outside its declared directory.");
        return path;
    }

    static bool ValidSegments(string path, bool allowParents)
    {
        var depth = 0;
        foreach (var segment in path.Split(['/', '\\'], StringSplitOptions.RemoveEmptyEntries))
        {
            if (segment == "." && allowParents)
                continue;
            if (segment == ".." && allowParents)
            {
                if (--depth < 0)
                    return false;
                continue;
            }
            if (segment is "." or ".." || segment.Contains(':') ||
                segment.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 ||
                (OperatingSystem.IsWindows() && IsAmbiguousWindowsName(segment)))
                return false;
            depth++;
        }
        return true;
    }

    static bool IsAmbiguousWindowsName(string segment)
    {
        if (segment[^1] is '.' or ' ')
            return true;
        var stem = segment.Split('.', 2)[0].TrimEnd().ToUpperInvariant();
        return stem is "CON" or "PRN" or "AUX" or "NUL" or "CONIN$" or "CONOUT$" ||
            (stem.Length == 4 && (stem.StartsWith("COM", StringComparison.Ordinal) ||
                stem.StartsWith("LPT", StringComparison.Ordinal)) && "123456789¹²³".Contains(stem[^1]));
    }

    public static string NormalizeSeparators(string path) =>
        path.Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar);

    // Check from the volume down so even a dangling link in an ancestor is rejected.
    public static bool CheckPath(string path, bool directory, bool allowMissing = false)
    {
        var parents = new Stack<string>();
        for (string? current = path; current != null; current = Path.GetDirectoryName(current))
            parents.Push(current);

        while (parents.TryPop(out var current))
        {
            FileAttributes attributes;
            try
            {
                attributes = File.GetAttributes(current);
            }
            catch (Exception ex) when (allowMissing && ex is FileNotFoundException or DirectoryNotFoundException)
            {
                return false;
            }
            CheckAttributes(attributes);
            if (attributes.HasFlag(FileAttributes.Directory) != (parents.Count > 0 || directory))
                throw new IOException($"Unexpected file or directory at '{current}'.");
        }
        return true;
    }

    public static void CheckAttributes(FileAttributes attributes)
    {
        if (attributes.HasFlag(FileAttributes.ReparsePoint))
            throw new UnauthorizedAccessException("Symbolic links and reparse points cannot be used for game saves.");
    }

    public static void CreateDirectory(string path)
    {
        if (!CheckPath(path, directory: true, allowMissing: true))
        {
            Directory.CreateDirectory(path);
            CheckPath(path, directory: true);
        }
    }

    public static bool IsFileError(Exception ex) =>
        ex is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException or SecurityException;

    public static bool IsValidPattern([NotNullWhen(true)] string? pattern) =>
        !string.IsNullOrWhiteSpace(pattern) && pattern.IndexOfAny(['/', '\\', ':', '\0']) < 0;

    public static bool MatchesPattern(string pattern, string fileName) =>
        FileSystemName.MatchesSimpleExpression(pattern == "*.*" ? "*" : pattern, fileName, OperatingSystem.IsWindows());

    public static List<GameSaveFile> CollectFiles(string root, string pattern, bool recursive,
        List<string> warnings, CancellationToken ct, Func<string, bool>? includeName = null)
    {
        if (!IsValidPattern(pattern))
            throw new IOException("The save pattern must be a filename pattern.");

        var files = new List<GameSaveFile>();
        var directories = new Stack<string>();
        directories.Push(root);
        while (directories.TryPop(out var directory))
        {
            ct.ThrowIfCancellationRequested();
            try
            {
                CheckPath(directory, directory: true);
                foreach (var path in Directory.EnumerateFileSystemEntries(directory))
                {
                    ct.ThrowIfCancellationRequested();
                    try
                    {
                        CanonicalPath(path);
                        var attributes = File.GetAttributes(path);
                        if (attributes.HasFlag(FileAttributes.Directory))
                        {
                            if (recursive)
                            {
                                CheckAttributes(attributes);
                                directories.Push(path);
                            }
                            continue;
                        }
                        var name = Path.GetFileName(path);
                        if (!MatchesPattern(pattern, name) || includeName?.Invoke(name) == false)
                            continue;
                        CheckAttributes(attributes);
                        var info = new FileInfo(path);
                        var relative = Path.GetRelativePath(root, path);
                        if (!PathComparer.Equals(path, ResolveRelative(root, relative)))
                            throw new IOException("This file name cannot be safely represented in a game save snapshot.");
                        files.Add(new(path, relative, info.Length, info.LastWriteTimeUtc));
                    }
                    catch (Exception ex) when (IsFileError(ex))
                    {
                        warnings.Add($"{path}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex) when (IsFileError(ex))
            {
                warnings.Add($"{directory}: {ex.Message}");
            }
        }
        return files;
    }
}

internal sealed record GameSaveFile(string Path, string RelativePath, long SizeBytes, DateTime LastWriteUtc);
