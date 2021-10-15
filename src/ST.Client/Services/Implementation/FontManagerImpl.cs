using System;
using System.Collections.Generic;
using System.Text;

namespace System.Application.Services.Implementation
{
    public class FontManagerImpl : IFontManager
    {
        protected readonly IPlatformService platformService;

        public FontManagerImpl(IPlatformService platformService)
        {
            this.platformService = platformService;
        }

        public virtual IReadOnlyCollection<KeyValuePair<string, string>> GetFonts()
        {
            var fonts = platformService.GetFontsByGdiPlus();
            return fonts;
        }
    }
}
