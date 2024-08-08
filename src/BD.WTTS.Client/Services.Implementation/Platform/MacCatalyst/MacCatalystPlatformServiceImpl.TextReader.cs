#if MACOS || MACCATALYST || IOS
// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

partial class MacCatalystPlatformServiceImpl
{
    public const string TextEdit = "TextEdit";
    public const string VSC = "Visual Studio Code";

    [Mobius(
"""
Mobius.Helpers.FolderHelper
""")]
    public string? GetFileName(TextReaderProvider provider) => provider switch
    {
        TextReaderProvider.VSCode => VSC,
        TextReaderProvider.Notepad => TextEdit,
        _ => null,
    };
}
#endif