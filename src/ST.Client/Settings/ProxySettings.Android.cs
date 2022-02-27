using System.Runtime.Versioning;

namespace System.Application.Settings
{
    partial class ProxySettings
    {
        static readonly SerializableProperty<bool>? _IsVpnMode = OperatingSystem2.IsAndroid ? GetProperty(defaultValue: true, autoSave: true) : null;

        /// <summary>
        /// 是否启用 VPN 模式
        /// </summary>
        [SupportedOSPlatform("Android")]
        public static SerializableProperty<bool> IsVpnMode => _IsVpnMode ?? throw new PlatformNotSupportedException();
    }
}