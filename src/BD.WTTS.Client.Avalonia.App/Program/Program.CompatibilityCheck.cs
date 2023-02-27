#if WINDOWS || NETFRAMEWORK
#if NETFRAMEWORK
using WPFMessageBox = System.Windows.MessageBox;
using WPFMessageBoxButton = System.Windows.MessageBoxButton;
using WPFMessageBoxImage = System.Windows.MessageBoxImage;
#endif
using AppResources = BD.WTTS.Client.Resources.Strings;

// ReSharper disable once CheckNamespace
namespace BD.WTTS;

public static partial class Program
{
    /// <summary>
    /// 兼容性检查
    /// </summary>
    /// <returns></returns>
#if NET40
    [MethodImpl((MethodImplOptions)0x100)]
#else
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    static bool CompatibilityCheck()
    {
        int major = 10, minor = 0, build = 17763;
#if NETFRAMEWORK
        if (Environment.OSVersion.Version < new Version(major, minor, build))
#else
        if (!OperatingSystem.IsWindowsVersionAtLeast(major, minor, build))
#endif
        {
            ShowErrMessageBox("此应用程序仅兼容 Windows 11 与 Windows 10 版本 1809（OS 内部版本 17763）或更高版本");
            return false;
        }
        var baseDirectory =
#if NET46_OR_GREATER || NETCOREAPP
        AppContext.BaseDirectory;
#else
        AppDomain.CurrentDomain.BaseDirectory;
#endif
        if (baseDirectory.StartsWith(Path.GetTempPath(), StringComparison.OrdinalIgnoreCase))
        {
            // 检测当前目录 Temp\Rar$ 这类目录，可能是在压缩包中直接启动程序导致的，还有一堆 文件找不到/加载失败的异常
            //  System.IO.DirectoryNotFoundException: Could not find a part of the path 'C:\Users\USER\AppData\Local\Temp\Rar$EXa15528.13350\Cache\switchproxy.reg'.
            //  System.IO.FileLoadException ...
            //  System.IO.FileNotFoundException: Could not load file or assembly ...
            ShowErrMessageBox(AppResources.Error_BaseDir_StartsWith_Temp);
            return false;
        }
        return true;
    }

#if NET40
    [MethodImpl((MethodImplOptions)0x100)]
#else
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static void ShowErrMessageBox(string error) => WPFMessageBox.Show(error, AppResources.Error, WPFMessageBoxButton.OK, WPFMessageBoxImage.Error);
}
#endif