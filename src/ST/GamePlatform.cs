namespace System.Application
{
    public enum GamePlatform : byte
    {
        Steam = 1,

        Origin,

        EADesktop,

        Uplay,

        MicrosoftStore,

        GOG,

        Epic,

        PlayStation,

        Xbox,

        NintendoSwitch,

        BattleNet,

        Google,

        // Add New ...

        #region WinAuth3 Compat

        HOTP,

        TOTP,

        #endregion
    }

#if DEBUG

    [Obsolete("use GamePlatform", true)]
    public enum GamePlatformEnum
    {
    }

#endif
}