#if LINUX
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using BD.WTTS.Models;

namespace BD.WTTS.Services.Implementation;

internal static class LinuxDesktopAccountActions
{
    const string PortableDesktopFile = "Watt Toolkit.desktop";
    const string PackagedDesktopFile = "Steam++.desktop";
    const string ActionPrefix = "WattToolkitSteamAccount-";
    const string ActionGroupPrefix = "[Desktop Action ";
    const string LauncherName = "Steam++";
    static readonly string[] launcherFiles = [PortableDesktopFile, PackagedDesktopFile];
    static readonly object sync = new();

    internal static void Apply(IReadOnlyList<SteamAccountShortcut> accounts)
    {
        if (GetLauncher() is not { } launcher)
            return;

        var dataHome = Environment.GetEnvironmentVariable("XDG_DATA_HOME") ?? string.Empty;
        if (!Path.IsPathFullyQualified(dataHome))
            dataHome = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local", "share");
        if (!Path.IsPathFullyQualified(dataHome))
            return;

        var applicationDirectory = Path.Combine(dataHome, "applications");
        lock (sync)
        {
            var entry = FindEntry(applicationDirectory, launcher);
            if (entry is null && !accounts.Any(static account => !string.IsNullOrWhiteSpace(account.AccountName)))
                return;

            var fileName = entry?.FileName ?? PortableDesktopFile;
            var path = Path.Combine(applicationDirectory, fileName);
            if (entry is null && File.Exists(path))
            {
                // Keep a shortcut from a separate installation intact.
                fileName = $"net.steampp.WattToolkit-{StableId(launcher.Command)}.desktop";
                path = Path.Combine(applicationDirectory, fileName);
            }

            var content = entry?.Content ?? (File.Exists(path) ? File.ReadAllText(path) : CreateEntry(launcher));
            var updated = ReplaceActions(content, launcher.Command, accounts);
            if (updated != content || !File.Exists(path))
                WriteEntry(path, updated);
        }
    }

    static (string FileName, string Content)? FindEntry(string applicationDirectory, Launcher launcher)
    {
        var systemDirectories = Environment.GetEnvironmentVariable("XDG_DATA_DIRS");
        if (string.IsNullOrEmpty(systemDirectories))
            systemDirectories = "/usr/local/share:/usr/share";
        var directories = systemDirectories.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries)
            .Where(Path.IsPathFullyQualified)
            .Select(directory => Path.Combine(directory, "applications"))
            .Prepend(applicationDirectory).ToArray();

        // GNOME and other GIO launchers expose the desktop file that actually started this process.
        var launchedFile = Environment.GetEnvironmentVariable("GIO_LAUNCHED_DESKTOP_FILE");
        if (int.TryParse(Environment.GetEnvironmentVariable("GIO_LAUNCHED_DESKTOP_FILE_PID"), out var launcherPid) &&
            launcherPid == Environment.ProcessId && Path.IsPathFullyQualified(launchedFile ?? string.Empty) &&
            string.Equals(Path.GetExtension(launchedFile), ".desktop", StringComparison.Ordinal) && File.Exists(launchedFile))
        {
            var name = Path.GetFileName(launchedFile)!;
            foreach (var directory in directories)
            {
                var relative = Path.GetRelativePath(directory, launchedFile!);
                if (!Path.IsPathRooted(relative) && relative != ".." &&
                    !relative.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal))
                {
                    name = relative;
                    break;
                }
            }
            var userFile = Path.Combine(applicationDirectory, name);
            var content = File.ReadAllText(File.Exists(userFile) ? userFile : launchedFile!);
            if (GetMainValue(content, "Type") == "Application" &&
                GetMainValue(content, "Exec") is { } exec && launcher.Matches(exec))
                return (name, content);
        }

        var desktopFileNames = launcherFiles.Append($"net.steampp.WattToolkit-{StableId(launcher.Command)}.desktop");

        var hiddenIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (var directory in directories)
        {
            foreach (var fileName in desktopFileNames)
            {
                var file = Path.Combine(directory, fileName);
                if (hiddenIds.Contains(fileName) || !File.Exists(file))
                    continue;

                hiddenIds.Add(fileName);
                var content = File.ReadAllText(file);
                if (GetMainValue(content, "Type") == "Application" &&
                    GetMainValue(content, "Exec") is { } exec && launcher.Matches(exec))
                    return (fileName, content);
            }
        }

        return null;
    }

    static Launcher? GetLauncher()
    {
        var root = Path.TrimEndingDirectorySeparator(AppContext.BaseDirectory);
        if (Path.GetFileName(root) == "assemblies" && Path.GetDirectoryName(root) is { } packageRoot)
            root = packageRoot;

        var executable = Path.Combine(root, LauncherName);
        var script = executable + ".sh";
        var icon = Path.Combine(root, "Icons", "Watt-Toolkit.png");
        if (!File.Exists(icon))
            icon = executable + ".ico";
        if (File.Exists(script))
            return new Launcher(script, executable, null, File.Exists(icon) ? icon : null);

        var processPath = Environment.ProcessPath;
        if (string.IsNullOrWhiteSpace(processPath))
            return null;

        var isDotnetHost = Path.GetFileNameWithoutExtension(processPath) == "dotnet";
        var assemblyPath = isDotnetHost
            ? Assembly.GetEntryAssembly()?.Location : null;
        if (isDotnetHost && string.IsNullOrEmpty(assemblyPath))
            return null;
        return new Launcher(processPath, executable, assemblyPath, File.Exists(icon) ? icon : null);
    }

    static string CreateEntry(Launcher launcher) => $"""
        [Desktop Entry]
        Type=Application
        Name=Watt Toolkit
        Exec={launcher.Command}
        Terminal=false
        StartupWMClass={LauncherName}
        """ + (launcher.Icon is { } icon ? $"\nIcon={EscapeValue(icon)}\n" : "\n");

    static string ReplaceActions(string content, string command, IReadOnlyList<SteamAccountShortcut> accounts)
    {
        var newLine = content.Contains("\r\n", StringComparison.Ordinal) ? "\r\n" : "\n";
        var lines = content.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');
        var retained = new List<string>(lines.Length);
        var unrelated = new List<string>();
        var actionsIndex = -1;
        var entryEnd = -1;
        var main = false;
        var generated = false;
        var hadGeneratedActions = false;
        var hasStartupClass = false;

        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (trimmed.StartsWith('[') && trimmed.EndsWith(']'))
            {
                if (main)
                    entryEnd = retained.Count;
                main = trimmed == "[Desktop Entry]";
                generated = trimmed.StartsWith(ActionGroupPrefix, StringComparison.Ordinal) &&
                    IsGeneratedId(trimmed.AsSpan(ActionGroupPrefix.Length, trimmed.Length - ActionGroupPrefix.Length - 1));
                hadGeneratedActions |= generated;
            }
            if (generated)
                continue;

            if (main && GetValue(trimmed, "StartupWMClass") is not null)
                hasStartupClass = true;
            if (main && GetValue(trimmed, "Actions") is { } value)
            {
                actionsIndex = retained.Count;
                foreach (var id in value.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                {
                    if (IsGeneratedId(id))
                        hadGeneratedActions = true;
                    else
                        unrelated.Add(id);
                }
            }
            retained.Add(line);
        }
        if (main)
            entryEnd = retained.Count;
        if (entryEnd < 0)
            return content;

        var shortcuts = accounts.Where(account => !string.IsNullOrWhiteSpace(account.AccountName))
            .DistinctBy(account => account.AccountName, StringComparer.Ordinal)
            .Select(account => (Account: account, Id: ActionPrefix + StableId(account.AccountName))).ToArray();
        if (!hadGeneratedActions && shortcuts.Length == 0)
            return content;

        unrelated.AddRange(shortcuts.Select(shortcut => shortcut.Id));
        if (actionsIndex >= 0)
            retained.RemoveAt(actionsIndex);
        else
            actionsIndex = entryEnd;
        if (unrelated.Count > 0)
            retained.Insert(actionsIndex, $"Actions={string.Join(';', unrelated)};");
        if (!hasStartupClass && shortcuts.Length > 0)
            retained.Insert(actionsIndex, $"StartupWMClass={LauncherName}");

        while (retained.Count > 0 && retained[^1].Length == 0)
            retained.RemoveAt(retained.Count - 1);

        foreach (var (account, id) in shortcuts)
        {
            retained.Add(string.Empty);
            retained.Add($"[Desktop Action {id}]");
            retained.Add($"Name={EscapeValue(account.Title)}");
            retained.Add($"Exec={command} -clt steam -account {QuoteArgument(account.AccountName)}");
        }

        return string.Join(newLine, retained) + newLine;
    }

    static string? GetMainValue(string content, string key)
    {
        var main = false;
        using var reader = new StringReader(content);
        while (reader.ReadLine() is { } line)
        {
            line = line.Trim();
            if (line.StartsWith('[') && line.EndsWith(']'))
            {
                if (main)
                    break;
                main = line == "[Desktop Entry]";
            }
            if (main && GetValue(line, key) is { } value)
                return value;
        }
        return null;
    }

    static string? GetValue(string line, string key)
    {
        var separator = line.IndexOf('=');
        return separator >= 0 && line.AsSpan(0, separator).Trim().SequenceEqual(key)
            ? line[(separator + 1)..].Trim() : null;
    }

    static void WriteEntry(string path, string content)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var temp = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            using (var stream = new FileStream(temp, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            using (var writer = new StreamWriter(stream, new UTF8Encoding(false)))
                writer.Write(content);
            if (OperatingSystem.IsLinux() && File.Exists(path))
                File.SetUnixFileMode(temp, File.GetUnixFileMode(path));
            File.Move(temp, path, overwrite: true);
        }
        finally
        {
            File.Delete(temp);
        }
    }

    static string StableId(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();

    static bool IsGeneratedId(ReadOnlySpan<char> id) =>
        id.Length == ActionPrefix.Length + SHA256.HashSizeInBytes * 2 && id.StartsWith(ActionPrefix, StringComparison.Ordinal) &&
        id[ActionPrefix.Length..].IndexOfAnyExcept("0123456789abcdef") < 0;

    static string EscapeValue(string value) => value.Replace("\\", "\\\\", StringComparison.Ordinal)
        .Replace("\n", "\\n", StringComparison.Ordinal).Replace("\r", "\\r", StringComparison.Ordinal)
        .Replace("\t", "\\t", StringComparison.Ordinal);

    static string QuoteArgument(string value) => EscapeValue('"' + value.Replace("\\", "\\\\", StringComparison.Ordinal)
        .Replace("\"", "\\\"", StringComparison.Ordinal).Replace("$", "\\$", StringComparison.Ordinal)
        .Replace("`", "\\`", StringComparison.Ordinal).Replace("%", "%%", StringComparison.Ordinal) + '"');

    readonly record struct Launcher(string Executable, string NativeExecutable, string? Assembly, string? Icon)
    {
        internal string Command => Assembly is { Length: > 0 }
            ? QuoteArgument(Executable) + " " + QuoteArgument(Assembly) : QuoteArgument(Executable);

        internal bool Matches(string command) => StartsWithCommand(command, Command) ||
            (Assembly is { Length: > 0 }
                ? StartsWithCommand(command, Executable + " " + Assembly)
                : StartsWithCommand(command, Executable) || StartsWithCommand(command, NativeExecutable) ||
                    StartsWithCommand(command, QuoteArgument(NativeExecutable)));

        static bool StartsWithCommand(string command, string executable) =>
            command.StartsWith(executable, StringComparison.Ordinal) &&
            (command.Length == executable.Length || char.IsWhiteSpace(command[executable.Length]));
    }
}
#endif
