using System.Collections.Generic;
#if !__MOBILE__
using IPlatformService2 = System.Application.Services.IDesktopPlatformService;
#else
using IPlatformService2 = System.Application.Services.IMobilePlatformService;
#endif

namespace System.Application.Services.Implementation
{
    public class FontManagerImpl : IFontManager
    {
        protected readonly IPlatformService2 platformService;

        public FontManagerImpl(IPlatformService2 platformService)
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