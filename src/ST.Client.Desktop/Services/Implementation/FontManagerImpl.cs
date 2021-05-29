using System.Collections.Generic;

namespace System.Application.Services.Implementation
{
    public class FontManagerImpl : IFontManager
    {
        public virtual IReadOnlyCollection<KeyValuePair<string, string>> GetFonts()
        {
            var fonts = IFontManager.GetFontsByGdiPlus();
            return fonts;
        }
    }
}