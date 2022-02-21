using System.Collections.Concurrent;
using System.Collections.Generic;
using Compat = System.Application.Settings.GameLibrarySettings;
using System.Application.UI;
using System.Runtime.Versioning;
using System.Application.Models;

namespace System.Application.Settings
{
    [SupportedOSPlatform("Windows7.0")]
    [SupportedOSPlatform("macOS")]
    [SupportedOSPlatform("Linux")]
    public sealed class SteamAccountSettings : SettingsHost2<SteamAccountSettings>
    {
        #region Compat

        static readonly SerializableProperty<ConcurrentDictionary<long, string?>?>? _AccountRemarks = IApplication.IsDesktopPlatform ?
            Compat.GetProperty<ConcurrentDictionary<long, string?>?>(defaultValue: null, autoSave: false) : null;
        /// <summary>
        /// Steam账号备注字典
        /// </summary>
        public static SerializableProperty<ConcurrentDictionary<long, string?>?> AccountRemarks => _AccountRemarks ?? throw new PlatformNotSupportedException();

        // ----- AccountRemarks ClassName 之前用的 GameLibrarySettings 需要兼容已发行的版本，将错就错，之后新增的还是使用 GetProperty 而不是 Compat.GetProperty -----
        /// <summary>
        /// Steam家庭共享临时禁用
        /// </summary>
        public static SerializableProperty<IReadOnlyCollection<DisableAuthorizedDevice>> DisableAuthorizedDevice => GetProperty(defaultValue: (IReadOnlyCollection<DisableAuthorizedDevice>)Array.Empty<DisableAuthorizedDevice>(), autoSave: true);
        #endregion
    }
}
