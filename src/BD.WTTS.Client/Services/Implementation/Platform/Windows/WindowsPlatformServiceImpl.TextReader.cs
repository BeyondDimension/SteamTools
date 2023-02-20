#if WINDOWS

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

partial class WindowsPlatformServiceImpl
{
    static string GetFolderPath(Environment.SpecialFolder folder)
    {
        switch (folder)
        {
            case Environment.SpecialFolder.ProgramFiles:
                var trimEndMark = " (x86)";
                var value = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
                if (value.EndsWith(trimEndMark, StringComparison.OrdinalIgnoreCase))
                {
                    var value2 = value[..^trimEndMark.Length];
                    if (Directory.Exists(value2))
                    {
                        return value2;
                    }
                }
                return value;
            default:
                return Environment.GetFolderPath(folder);
        }
    }

    public string? GetFileName(TextReaderProvider provider)
    {
        switch (provider)
        {
            case TextReaderProvider.VSCode:
                var vsCodePaths = new[] {
                    GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                    GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                }.Distinct().Select(x => Path.Combine(x, "Microsoft VS Code", "Code.exe"));

                foreach (var vsCodePath in vsCodePaths)
                {
                    if (File.Exists(vsCodePath))
                    {
                        return vsCodePath;
                    }
                }

                return null;
            case TextReaderProvider.Notepad:
                return "notepad";
            default:
                return null;
        }
    }
}
#endif