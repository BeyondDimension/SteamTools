namespace System.Application;

public static class OSNames
{
    const string WindowsDesktopBridge = "Windows Desktop Bridge";
    const string Android = "Android";

    public enum Value : byte
    {
        /// <summary>
        /// Universal Windows Platform / 通用 Windows 平台
        /// </summary>
        UWP = 1,

        /// <summary>
        /// Windows 桌面桥应用
        /// <para>桌面桥应用是使用桌面桥转换为通用 Windows 平台 (UWP) 应用的 Windows 桌面应用程序。 转换后，将以面向 Windows 10 桌面版的 UWP 应用包（.appx 或 .appxbundle）的形式打包、维护和部署 Windows 桌面应用程序。</para>
        /// </summary>
        WindowsDesktopBridge,

        /// <summary>
        /// 桌面 Win32 应用
        /// </summary>
        Windows,

        /// <summary>
        /// 适用于 Android™ 的 Windows 子系统
        /// </summary>
        WSA,

        /// <summary>
        /// 未知设备类型的 Android
        /// </summary>
        AndroidUnknown,

        /// <summary>
        /// https://www.apple.com.cn/ipados
        /// </summary>
        iPadOS,

        /// <summary>
        /// https://developer.apple.com/cn/ios
        /// </summary>
        iOS,

        /// <summary>
        /// https://www.apple.com.cn/macos
        /// </summary>
        macOS,

        /// <summary>
        /// https://developer.apple.com/cn/tvos
        /// </summary>
        tvOS,

        /// <summary>
        /// https://www.apple.com.cn/watchos
        /// </summary>
        watchOS,

        /// <summary>
        /// GNU/Linux
        /// </summary>
        Linux,

        /// <summary>
        /// Android 手机
        /// </summary>
        AndroidPhone,

        /// <summary>
        /// Android 平板
        /// </summary>
        AndroidTablet,

        /// <summary>
        /// Android 桌面端
        /// </summary>
        AndroidDesktop,

        /// <summary>
        /// https://developer.android.google.cn/training/tv
        /// </summary>
        AndroidTV,

        /// <summary>
        /// https://developer.android.google.cn/training/wearables
        /// </summary>
        AndroidWatch,

        /// <summary>
        /// Android 模拟器
        /// </summary>
        AndroidVirtual,

        /// <summary>
        /// https://github.com/chromeos
        /// </summary>
        ChromeOS,

        /// <summary>
        /// Windows UI 库 (WinUI) 3
        /// </summary>
        WinUI,
    }

    public static string ToDisplayName(this Value value) => value == default ? string.Empty :
        (value.IsAndroid() ? Android :
            value switch
            {
                Value.WindowsDesktopBridge => WindowsDesktopBridge,
                _ => value.ToString(),
            });

    public static Value Parse(string value)
    {
        if (Enum.TryParse<Value>(value, true, out var valueEnum)) return valueEnum;
        if (string.Equals(value, WindowsDesktopBridge, StringComparison.OrdinalIgnoreCase))
            return Value.WindowsDesktopBridge;
        return default;
    }

    public static bool IsAndroid(this Value value) => value switch
    {
        Value.WSA or
        Value.AndroidUnknown or
        Value.AndroidPhone or
        Value.AndroidTablet or
        Value.AndroidDesktop or
        Value.AndroidTV or
        Value.AndroidWatch or
        Value.AndroidVirtual or
        Value.ChromeOS => true,
        _ => false,
    };
}
