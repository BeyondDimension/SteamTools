using System.Collections.Generic;
using System.Application.UI;
using System.Runtime.Versioning;

namespace System.Application.Settings
{
    partial class ASFSettings
    {
        static readonly SerializableProperty<string>? _FontName = IApplication.IsDesktopPlatform ? GetProperty(defaultValue: "Default", autoSave: true) : null;
        /// <summary>
        /// 字体
        /// </summary>
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Linux")]
        public static SerializableProperty<string> FontName => _FontName ?? throw new PlatformNotSupportedException();
    }
}
