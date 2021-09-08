using System.Application.UI.Resx;
using System.Collections.Generic;
#if !__MOBILE__
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
#endif

namespace System.Application.Services
{
    public interface IFontManager
    {
        public static IFontManager Instance => DI.Get<IFontManager>();

        protected static KeyValuePair<string, string> Default { get; } = KeyValuePair.Create(AppResources.Default, "Default");

        IReadOnlyCollection<KeyValuePair<string, string>> GetFonts();

        protected static IReadOnlyCollection<KeyValuePair<string, string>> GetFontsByGdiPlus()
        {
#if !__MOBILE__
            // https://docs.microsoft.com/zh-cn/typography/font-list
            var culture = R.Culture;
            InstalledFontCollection ifc = new();
            var list = ifc.Families.Where(x => x.IsStyleAvailable(FontStyle.Regular)).Select(x => KeyValuePair.Create(x.GetName(culture.LCID), x.GetName(1033))).ToList();
            list.Insert(0, IFontManager.Default);
            return list;
#else
            // Common7\IDE\ReferenceAssemblies\Microsoft\Framework\MonoAndroid\v1.0\Facades\System.Drawing.Common.dll
            // System.Drawing.Text.InstalledFontCollection
            // throw new PlatformNotSupportedException();
            return Array.Empty<KeyValuePair<string, string>>();
#endif
        }
    }
}