#if LINUX
// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

partial class LinuxPlatformServiceImpl
{
    public const string VSC = "code";

    public string? GetFileName(TextReaderProvider provider) => provider switch
    {
        TextReaderProvider.VSCode => VSC,
        TextReaderProvider.Notepad => xdg,
        _ => null,
    };
}
#endif