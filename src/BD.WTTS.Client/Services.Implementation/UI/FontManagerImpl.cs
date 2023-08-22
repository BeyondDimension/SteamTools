// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

public class FontManagerImpl : IFontManager
{
    protected readonly IPlatformService platformService;

    public FontManagerImpl(IPlatformService platformService)
    {
        this.platformService = platformService;
    }

    public virtual IReadOnlyCollection<KeyValuePair<string, string>> GetFonts()
    {
        var fonts = platformService.GetFonts();
        return fonts;
    }
}