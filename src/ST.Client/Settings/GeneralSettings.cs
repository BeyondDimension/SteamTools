using System;
using System.Collections.Generic;
using System.Text;

namespace System.Application.Settings
{
    internal sealed class GeneralSettings : Abstractions.GeneralSettings<GeneralSettings>
    {

    }
}

namespace System.Application.Settings.Abstractions
{
    public abstract class GeneralSettings<TSettings> : SettingsHost2<TSettings> where TSettings : GeneralSettings<TSettings>, new()
    {
        /// <summary>
        /// 自动检查更新
        /// </summary>
        public static SerializableProperty<bool> IsAutoCheckUpdate { get; }
            = GetProperty(defaultValue: true, autoSave: true);

        /// <summary>
        /// 下载更新渠道
        /// </summary>
        public static SerializableProperty<UpdateChannelType> UpdateChannel { get; }
            = GetProperty(defaultValue: (UpdateChannelType)default, autoSave: true);
    }
}
