using System.Runtime.Versioning;

namespace System.Application.Settings;

partial class GeneralSettings
{
    static readonly SerializableProperty<bool>? _CaptureScreen = OperatingSystem2.IsAndroid() ? GetProperty(defaultValue: false) : null;

    /// <summary>
    /// 屏幕捕获(允许截图)
    /// </summary>
    [SupportedOSPlatform("Android")]
    public static SerializableProperty<bool> CaptureScreen => _CaptureScreen ?? throw new PlatformNotSupportedException();
}
