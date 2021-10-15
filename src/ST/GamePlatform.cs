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

        #region WinAuth3 Compat

        HOTP,

        TOTP,

        #endregion

        // Add New ...
    }
}