using System.Application.UI.Resx;
using System.Collections.Generic;
using System.Text;

namespace System.Application.Services
{
    /// <summary>
    /// 字体管理
    /// </summary>
    public interface IFontManager
    {
        static IFontManager Instance => DI.Get<IFontManager>();

        public static KeyValuePair<string, string> Default { get; } = KeyValuePair.Create(AppResources.Default, KEY_Default);

        IReadOnlyCollection<KeyValuePair<string, string>> GetFonts();

        public const string KEY_Default = "Default";
        //public const string KEY_DefaultConsole = "DefaultConsole";
        //public const string ConsoleFont_CascadiaCode = "Cascadia Code";
        //public const string ConsoleFont_Consolas = "Consolas";
        //public const string ConsoleFont_SourceCodePro = "Source Code Pro";
        //public const string ConsoleFont_JetBrainsMono = "JetBrains Mono";
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
