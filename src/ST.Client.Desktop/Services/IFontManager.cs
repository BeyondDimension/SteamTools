using System.Application.UI.Resx;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;

namespace System.Application.Services
{
    public interface IFontManager
    {
        public static IFontManager Instance => DI.Get<IFontManager>();

        static KeyValuePair<string, string> Default { get; } = KeyValuePair.Create(AppResources.Default, "Default");

        IReadOnlyCollection<KeyValuePair<string, string>> GetFonts();

        protected static IReadOnlyCollection<KeyValuePair<string, string>> GetFontsByGdiPlus()
        {
            // https://docs.microsoft.com/zh-cn/typography/font-list
            var culture =
#if __MOBILE__
                AppResources
#else
                R
#endif
                .Culture;
            InstalledFontCollection ifc = new();
            var list = ifc.Families.Where(x => x.IsStyleAvailable(FontStyle.Regular)).Select(x => KeyValuePair.Create(x.GetName(culture.LCID), x.GetName(1033))).ToList();
            list.Insert(0, Default);
            return list;
        }
    }
}