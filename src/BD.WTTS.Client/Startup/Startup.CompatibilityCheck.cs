#if WINDOWS || NETFRAMEWORK || APP_HOST
using static BD.WTTS.Client.Resources.Strings;

// ReSharper disable once CheckNamespace
namespace BD.WTTS;

partial class Startup // 平台兼容性检查
{
    /// <summary>
    /// 兼容性检查
    /// <para>此应用程序仅兼容 Windows 11 与 Windows 10 版本 1809（OS 内部版本 17763）或更高版本</para>
    /// <para>不能在临时文件夹中运行此程序，请将所有文件复制或解压到其他路径后再启动程序</para>
    /// </summary>
    /// <returns></returns>
#if NET35 || NET40
    [MethodImpl((MethodImplOptions)0x100)]
#else
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    static bool CompatibilityCheck(string baseDirectory)
    {
#if NETFRAMEWORK || WINDOWS
        int major = 10, minor = 0, build = 17763;
#if NETFRAMEWORK
        if (Environment.OSVersion.Version < new Version(major, minor, build))
#else
        if (!OperatingSystem.IsWindowsVersionAtLeast(major, minor, build))
#endif
        {
            ShowErrMessageBox(Error_IncompatibleOS);
            return false;
        }
#endif
        if (baseDirectory.StartsWith(Path.GetTempPath(), StringComparison.OrdinalIgnoreCase))
        {
            // 检测当前目录 Temp\Rar$ 这类目录，可能是在压缩包中直接启动程序导致的，还有一堆 文件找不到/加载失败的异常
            //  System.IO.DirectoryNotFoundException: Could not find a part of the path 'C:\Users\USER\AppData\Local\Temp\Rar$EXa15528.13350\Cache\switchproxy.reg'.
            //  System.IO.FileLoadException ...
            //  System.IO.FileNotFoundException: Could not load file or assembly ...
            ShowErrMessageBox(Error_BaseDir_StartsWith_Temp);
            return false;
        }
        return true;
    }

#if NET35 || NET40
    [MethodImpl((MethodImplOptions)0x100)]
#else
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static WPFMessageBoxResult ShowErrMessageBox(string error, WPFMessageBoxButton button = WPFMessageBoxButton.OK)
    {
        try
        {
            if (Environment.UserInteractive)
            {
                // 在 Win 11 上 MessageBox 与 net35 WPF 上的样式不一致，net35 中的 Title 缺少一些 Padding
                // 发布此 win-any 与 win-x64 可比较区别
                return WPFMessageBox.Show(error,
                    Error, button, WPFMessageBoxImage.Error);
            }
        }
        catch
        {

        }

        Console.WriteLine("---------------------------");
        Console.WriteLine(Error);
        Console.WriteLine("---------------------------");
        Console.WriteLine(error);
        Console.WriteLine("---------------------------");
        switch (button)
        {
            case WPFMessageBoxButton.OKCancel:
                {
                    Console.WriteLine("OK/Cancel");
                    Console.WriteLine("---------------------------");
                    var line = Console.ReadLine();
                    if (string.Equals(line, nameof(WPFMessageBoxResult.OK)) || string.Equals(line, nameof(WPFMessageBoxResult.Yes)) || string.Equals(line, "o") || string.Equals(line, "y"))
                        return WPFMessageBoxResult.OK;
                    if (string.Equals(line, nameof(WPFMessageBoxResult.Cancel)) || string.Equals(line, nameof(WPFMessageBoxResult.No)) || string.Equals(line, "c") || string.Equals(line, "n"))
                        return WPFMessageBoxResult.Cancel;
                }
                break;
            case WPFMessageBoxButton.YesNoCancel:
                {
                    Console.WriteLine("Yes/No/Cancel");
                    Console.WriteLine("---------------------------");
                    var line = Console.ReadLine();
                    if (string.Equals(line, nameof(WPFMessageBoxResult.OK)) || string.Equals(line, nameof(WPFMessageBoxResult.Yes)) || string.Equals(line, "o") || string.Equals(line, "y"))
                        return WPFMessageBoxResult.Yes;
                    if (string.Equals(line, nameof(WPFMessageBoxResult.No)) || string.Equals(line, "n"))
                        return WPFMessageBoxResult.No;
                    if (string.Equals(line, nameof(WPFMessageBoxResult.Cancel)) || string.Equals(line, "c"))
                        return WPFMessageBoxResult.Cancel;
                }
                break;
            case WPFMessageBoxButton.YesNo:
                {
                    Console.WriteLine("Yes/No");
                    Console.WriteLine("---------------------------");
                    var line = Console.ReadLine();
                    if (string.Equals(line, nameof(WPFMessageBoxResult.OK)) || string.Equals(line, nameof(WPFMessageBoxResult.Yes)) || string.Equals(line, "o") || string.Equals(line, "y"))
                        return WPFMessageBoxResult.Yes;
                    if (string.Equals(line, nameof(WPFMessageBoxResult.Cancel)) || string.Equals(line, nameof(WPFMessageBoxResult.No)) || string.Equals(line, "c") || string.Equals(line, "n"))
                        return WPFMessageBoxResult.No;
                }
                break;
        }
        return WPFMessageBoxResult.OK;
    }
}

#endif
