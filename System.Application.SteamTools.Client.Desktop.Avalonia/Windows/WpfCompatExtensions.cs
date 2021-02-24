using System.Application.Services;
using System.IO;
using FontFamily = Avalonia.Media.FontFamily;
using Window = Avalonia.Controls.Window;

namespace System.Windows
{
    /// <summary>
    /// 通过扩展兼容WPF部分API的内容
    /// </summary>
    public static partial class WpfCompatExtensions
    {
        static readonly Lazy<bool> lazy_has_msyh = new Lazy<bool>(() =>
        {
            if (DI.Platform == Platform.Windows)
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

        public static void SetDefaultFontFamily(this Window window)
        {
            if (DI.Platform == Platform.Windows)
            {
                if (HAS_FONT_MSYH) // (版权、许可)不能在非WinOS上使用 微软雅黑字体，不可将字体嵌入程序
                {
                    string name;
                    var major = Environment.OSVersion.Version.Major;
                    if (major > 6 || (major == 6 && Environment.OSVersion.Version.Minor >= 2))
                    {
                        name = "Microsoft YaHei UI";
                    }
                    else
                    {
                        name = "Microsoft YaHei";
                    }
                    window.FontFamily = new FontFamily(name);
                }
            }
        }

        /// <summary>
        /// 设置调整大小模式。
        /// </summary>
        /// <param name="window"></param>
        /// <param name="value"></param>
        public static void SetResizeMode(this Window window, ResizeMode value)
        {
            var p = DI.Get<IDesktopPlatformService>();
            switch (value)
            {
                case ResizeMode.NoResize:
                case ResizeMode.CanMinimize:
                    window.CanResize = false;
                    break;
                case ResizeMode.CanResize:
#pragma warning disable CS0618 // 类型或成员已过时
                case ResizeMode.CanResizeWithGrip:
#pragma warning restore CS0618 // 类型或成员已过时
                    window.CanResize = true;
                    break;
            }
            p.SetResizeMode(window.PlatformImpl.Handle.Handle, (int)value);
        }
    }
}