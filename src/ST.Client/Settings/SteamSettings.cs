#if NETSTANDARD
using JustArchiNET.Madness;
#else
using System.Runtime.Versioning;
#endif

namespace System.Application.Settings
{
    [SupportedOSPlatform("Windows")]
    [SupportedOSPlatform("macOS")]
    [SupportedOSPlatform("Linux")]
    public sealed class SteamSettings : SettingsHost2<SteamSettings>
    {
        static SteamSettings()
        {
            if (!OperatingSystem2.Application.UseAvalonia) return;
            IsRunSteamMinimized.ValueChanged += IsRunSteamMinimized_ValueChanged;
            IsRunSteamNoCheckUpdate.ValueChanged += IsRunSteamNoCheckUpdate_ValueChanged;
        }

        static void IsRunSteamNoCheckUpdate_ValueChanged(object? sender, ValueChangedEventArgs<bool> e)
        {
            if (e.NewValue)
                SteamStratParameter.Value += " -noverifyfiles";
            else if (SteamStratParameter.Value != null)
                SteamStratParameter.Value = SteamStratParameter.Value.Replace("-noverifyfiles", "").Trim();
        }

        static void IsRunSteamMinimized_ValueChanged(object? sender, ValueChangedEventArgs<bool> e)
        {
            if (e.NewValue)
                SteamStratParameter.Value += " -silent";
            else if (SteamStratParameter.Value != null)
                SteamStratParameter.Value = SteamStratParameter.Value.Replace("-silent", "").Trim();
        }

        readonly static SerializableProperty<string>? _SteamStratParameter = OperatingSystem2.Application.UseAvalonia ? GetProperty(defaultValue: string.Empty, autoSave: true) : null;
        /// <summary>
        /// Steam启动参数
        /// </summary>
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Linux")]
        public static SerializableProperty<string> SteamStratParameter => _SteamStratParameter ?? throw new PlatformNotSupportedException();

        readonly static SerializableProperty<string>? _SteamSkin = OperatingSystem2.Application.UseAvalonia ? GetProperty(defaultValue: string.Empty, autoSave: true) : null;
        /// <summary>
        /// Steam皮肤
        /// </summary>
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Linux")]
        public static SerializableProperty<string> SteamSkin => _SteamSkin ?? throw new PlatformNotSupportedException();

        readonly static SerializableProperty<bool>? _IsAutoRunSteam = OperatingSystem2.Application.UseAvalonia ? GetProperty(defaultValue: false, autoSave: true) : null;
        /// <summary>
        /// 自动运行Steam
        /// </summary>
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Linux")]
        public static SerializableProperty<bool> IsAutoRunSteam => _IsAutoRunSteam ?? throw new PlatformNotSupportedException();

        readonly static SerializableProperty<bool>? _IsRunSteamMinimized = OperatingSystem2.Application.UseAvalonia ? GetProperty(defaultValue: false, autoSave: true) : null;
        /// <summary>
        /// Steam启动时最小化到托盘
        /// </summary>
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Linux")]
        public static SerializableProperty<bool> IsRunSteamMinimized => _IsRunSteamMinimized ?? throw new PlatformNotSupportedException();

        readonly static SerializableProperty<bool>? _IsRunSteamNoCheckUpdate = OperatingSystem2.Application.UseAvalonia ? GetProperty(defaultValue: false, autoSave: true) : null;
        /// <summary>
        /// Steam启动时不检查更新
        /// </summary>
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Linux")]
        public static SerializableProperty<bool> IsRunSteamNoCheckUpdate => _IsRunSteamNoCheckUpdate ?? throw new PlatformNotSupportedException();

        readonly static SerializableProperty<bool>? _IsEnableSteamLaunchNotification = OperatingSystem2.Application.UseAvalonia ? GetProperty(defaultValue: true, autoSave: true) : null;
        /// <summary>
        /// 检测到Steam登录时弹出消息通知
        /// </summary>
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Linux")]
        public static SerializableProperty<bool> IsEnableSteamLaunchNotification => _IsEnableSteamLaunchNotification ?? throw new PlatformNotSupportedException();
    }
}