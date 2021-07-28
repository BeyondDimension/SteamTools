using System.Application.Serialization;

namespace System.Application.Models.Settings
{
    public sealed class SteamSettings : SettingsHost2<SteamSettings>
    {
        static SteamSettings()
        {
            IsRunSteamMinimized.ValueChanged += IsRunSteamMinimized_ValueChanged;
            IsRunSteamNoCheckUpdate.ValueChanged += IsRunSteamNoCheckUpdate_ValueChanged;
        }

        private static void IsRunSteamNoCheckUpdate_ValueChanged(object? sender, ValueChangedEventArgs<bool> e)
        {
            if (e.NewValue)
                SteamStratParameter.Value += " -noverifyfiles";
            else if (SteamStratParameter.Value != null)
                SteamStratParameter.Value = SteamStratParameter.Value.Replace("-noverifyfiles", "").Trim();
        }

        private static void IsRunSteamMinimized_ValueChanged(object? sender, ValueChangedEventArgs<bool> e)
        {
            if (e.NewValue)
                SteamStratParameter.Value += " -silent";
            else if (SteamStratParameter.Value != null)
                SteamStratParameter.Value = SteamStratParameter.Value.Replace("-silent", "").Trim();
        }

        /// <summary>
        /// Steam启动参数
        /// </summary>
        public static SerializableProperty<string> SteamStratParameter { get; }
            = GetProperty(defaultValue: string.Empty, autoSave: true);

        /// <summary>
        /// Steam皮肤
        /// </summary>
        public static SerializableProperty<string> SteamSkin { get; }
            = GetProperty(defaultValue: string.Empty, autoSave: true);

        /// <summary>
        /// 自动运行Steam
        /// </summary>
        public static SerializableProperty<bool> IsAutoRunSteam { get; }
            = GetProperty(defaultValue: false, autoSave: true);

        /// <summary>
        /// Steam启动时最小化到托盘
        /// </summary>
        public static SerializableProperty<bool> IsRunSteamMinimized { get; }
            = GetProperty(defaultValue: false, autoSave: true);

        /// <summary>
        /// Steam启动时不检查更新
        /// </summary>
        public static SerializableProperty<bool> IsRunSteamNoCheckUpdate { get; }
            = GetProperty(defaultValue: false, autoSave: true);

        /// <summary>
        /// 检测到Steam启动时弹出消息通知
        /// </summary>
        public static SerializableProperty<bool> IsEnableSteamLaunchNotification { get; }
            = GetProperty(defaultValue: false, autoSave: true);
    }
}