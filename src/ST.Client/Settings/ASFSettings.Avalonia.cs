using System.Collections.Generic;
using System.Application.UI;
using System.Runtime.Versioning;
using System.Application.Services;

namespace System.Application.Settings
{
    partial class ASFSettings
    {
        static readonly SerializableProperty<string>? _ConsoleFontName = IApplication.IsDesktopPlatform ? GetProperty(defaultValue: IFontManager.KEY_Default, autoSave: true) : null;
        /// <summary>
        /// 字体
        /// </summary>
        [SupportedOSPlatform("Windows7.0")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Linux")]
        public static SerializableProperty<string> ConsoleFontName => _ConsoleFontName ?? throw new PlatformNotSupportedException();
    }
}
