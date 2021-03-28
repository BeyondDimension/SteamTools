using System.IO;
using System.Runtime.InteropServices;
using FontFamily = Avalonia.Media.FontFamily;
using Window = Avalonia.Controls.Window;

namespace System.Windows
{
    public static partial class WpfCompatExtensions
    {
        static readonly Lazy<bool> lazy_has_msyh = new(() =>
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var fontsPath = Environment.GetFolderPath(Environment.SpecialFolder.Fonts);
                var msyhFilePath = Path.Combine(fontsPath, "msyh.ttc");
                return File.Exists(msyhFilePath);
            }
            else
            {
                return false;
            }
        });

        public static bool HAS_FONT_MSYH => lazy_has_msyh.Value;

        public static void SetDefaultFontFamily(this Window window, bool isLight = false)
        {
            if (HAS_FONT_MSYH) // (版权、许可)不能在非WinOS上使用 微软雅黑字体，不可将字体嵌入程序
            {
                string name;
                var major = Environment.OSVersion.Version.Major;
                if (major > 6 || (major == 6 && Environment.OSVersion.Version.Minor >= 2))
                {
                    name = $"Microsoft YaHei UI{(isLight ? " Light" : "")}";
                }
                else
                {
                    name = $"Microsoft YaHei{(isLight ? " Light" : "")}";
                }
                window.FontFamily = new FontFamily(name);
            }
        }
    }
}