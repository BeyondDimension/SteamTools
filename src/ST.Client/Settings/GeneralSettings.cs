namespace System.Application.Settings;

public sealed partial class GeneralSettings : SettingsHost2<GeneralSettings>
{
    /// <summary>
    /// 自动检查更新
    /// </summary>
    public static SerializableProperty<bool> IsAutoCheckUpdate { get; }
        = GetProperty(defaultValue: true);

    /// <summary>
    /// 下载更新渠道
    /// </summary>
    public static SerializableProperty<UpdateChannelType> UpdateChannel { get; }
        = GetProperty(defaultValue: default(UpdateChannelType));
}
