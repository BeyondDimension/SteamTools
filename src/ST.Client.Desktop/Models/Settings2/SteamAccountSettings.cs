using System.Application.Serialization;
using System.Collections.Generic;
using Compat = System.Application.Models.Settings.GameLibrarySettings;

namespace System.Application.Models.Settings
{
    public sealed class SteamAccountSettings : SettingsHost2<SteamAccountSettings>
    {
        /// <summary>
        /// Steam账号备注字典
        /// </summary>
        public static SerializableProperty<IReadOnlyDictionary<long, string?>> AccountRemarks { get; }
            = Compat.GetProperty(defaultValue: (IReadOnlyDictionary<long, string?>?)null, autoSave: false);

        // ----- AccountRemarks ClassName 之前用的 GameLibrarySettings 需要兼容已发行的版本，将错就错，之后新增的还是使用 GetProperty 而不是 Compat.GetProperty -----
    }
}