using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;

namespace System.Application.Converters
{
    public class NameToFontFamilyConverter : IValueConverter
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
        static readonly Lazy<FontFamily> mDefault = new(() => GetDefaultFontFamily());
        static FontFamily GetDefaultFontFamily(bool isLight = false)
        {
            if (lazy_has_msyh.Value) // (版权、许可)不能在非WinOS上使用 微软雅黑字体，不可将字体嵌入程序
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
                return new FontFamily(name);
            }
            return FontFamily.Default;
        }

        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string s)
            {
                if (string.IsNullOrEmpty(s) || s.Equals("Default", StringComparison.OrdinalIgnoreCase))
                    return mDefault.Value;
                return FontFamily.Parse(s);
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return BindingOperations.DoNothing;
        }
    }
}