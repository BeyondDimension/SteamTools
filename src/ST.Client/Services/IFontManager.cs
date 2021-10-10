using System;
using System.Application.UI.Resx;
using System.Collections.Generic;
using System.Text;

namespace System.Application.Services
{
    public interface IFontManager
    {
        public static IFontManager Instance => DI.Get<IFontManager>();

        public static KeyValuePair<string, string> Default { get; } = KeyValuePair.Create(AppResources.Default, "Default");

        IReadOnlyCollection<KeyValuePair<string, string>> GetFonts();
    }

    partial interface IPlatformService
    {
        /// <summary>
        /// 由 GDI+ 实现的获取当前系统字体数组，仅在 Windows 平台上实现，其他平台将返回空数组
        /// </summary>
        /// <returns></returns>
        public IReadOnlyCollection<KeyValuePair<string, string>> GetFontsByGdiPlus()
        {
            // Common7\IDE\ReferenceAssemblies\Microsoft\Framework\MonoAndroid\v1.0\Facades\System.Drawing.Common.dll
            // System.Drawing.Text.InstalledFontCollection
            // throw new PlatformNotSupportedException();
            return Array.Empty<KeyValuePair<string, string>>();
        }
    }
}
