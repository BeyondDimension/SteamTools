using static BD.WTTS.GameList.Saves.GameSaveFileSystem;

namespace BD.WTTS.GameList.Saves;

public sealed record GameSaveTreeNode(
    string Name,
    string FullPath,
    string RelativePath,
    bool IsDirectory,
    long SizeBytes,
    int FileCount,
    DateTime LastWriteUtc,
    IReadOnlyList<GameSaveTreeNode> Children);

public static class GameSaveTreeBuilder
{
    public static IReadOnlyList<GameSaveTreeNode> Build(GameSaveLocationInfo location)
    {
        if (location.ResolvedDirectory is not { } directory || location.Files.Count == 0)
            return [];

        var rootPath = CanonicalPath(directory);
        var root = new DirectoryEntry("", rootPath, "");
        foreach (var file in location.Files)
        {
            var fullPath = ResolveRelative(rootPath, file.RelativePath);
            if (!PathComparer.Equals(fullPath, CanonicalPath(file.FullPath)))
                throw new IOException("The scanned file path does not match its save location.");

            var relativePath = Path.GetRelativePath(rootPath, fullPath);
            var parts = relativePath.Split(Path.DirectorySeparatorChar);
            var parent = root;
            foreach (var part in parts.AsSpan(0, parts.Length - 1))
                parent = parent.GetOrAddDirectory(part);
            parent.Files.Add(new(file.Name, fullPath, relativePath, false, file.SizeBytes, 1, file.LastWriteUtc, []));
        }
        return root.BuildChildren();
    }

    sealed class DirectoryEntry(string name, string fullPath, string relativePath)
    {
        readonly Dictionary<string, DirectoryEntry> directories = new(PathComparer);

        public List<GameSaveTreeNode> Files { get; } = [];

        public DirectoryEntry GetOrAddDirectory(string childName)
        {
            if (!directories.TryGetValue(childName, out var directory))
            {
                directory = new(childName, Path.Combine(fullPath, childName), Path.Combine(relativePath, childName));
                directories.Add(childName, directory);
            }
            return directory;
        }

        public IReadOnlyList<GameSaveTreeNode> BuildChildren() => directories.Values.Select(directory => directory.Build())
            .Concat(Files)
            .OrderByDescending(node => node.IsDirectory)
            .ThenBy(node => node.Name, StringComparer.OrdinalIgnoreCase)
            .ThenBy(node => node.Name, StringComparer.Ordinal)
            .ToArray();

        GameSaveTreeNode Build()
        {
            var children = BuildChildren();
            return new(name, fullPath, relativePath, true, children.Sum(node => node.SizeBytes),
                children.Sum(node => node.FileCount), children.Max(node => node.LastWriteUtc), children);
        }
    }
}
