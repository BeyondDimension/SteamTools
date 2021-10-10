namespace System.Application.Settings
{
    public sealed class ASFSettings : SettingsHost2<ASFSettings>
    {
        static ASFSettings()
        {
        }

        /// <summary>
        /// ASF路径
        /// </summary>
        public static SerializableProperty<string> ArchiSteamFarmExePath { get; }
            = GetProperty(defaultValue: string.Empty, autoSave: true);

        /// <summary>
        /// 程序启动时自动运行ASF
        /// </summary>
        public static SerializableProperty<bool> AutoRunArchiSteamFarm { get; }
            = GetProperty(defaultValue: false, autoSave: true);
    }
}