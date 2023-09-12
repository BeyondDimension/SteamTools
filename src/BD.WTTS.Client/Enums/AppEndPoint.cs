namespace BD.WTTS.Enums;

/// <summary>
/// 应用程序终结点路由
/// </summary>
public enum AppEndPoint : byte
{
    LoginOrRegister = 1,
    MessageBox,
    AddAuth,
    ShowAuth,
    AuthTrade,
    EncryptionAuth,
    TextBox,
    TaskBar,
    ExportAuth,
    ScriptStore,
    HideApp,
    EditAppInfo,
    IdleApp,
    ShareManage,
    [Obsolete]
    ChangeBindPhoneNumber,
    UserProfile,
    [Obsolete("not impl", true)]
    NewVersion,
    [Obsolete]
    BindPhoneNumber,
    ASF_AddBot,
    SteamShutdown,
    ProxySettings,
    Notice,
    SaveEditedAppInfo,
    Content,
}

#if DEBUG
[Obsolete("use AppEndPoint", true)]
public enum CustomWindow { }
#endif