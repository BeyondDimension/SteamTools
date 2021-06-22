namespace System.Application
{
    /// <summary>
    /// 业务自定义窗口
    /// </summary>
    public enum CustomWindow
    {
#if !__MOBILE__
        LoginOrRegister,
#endif
        MessageBox,
#if !__MOBILE__
        AddAuth,
        ShowAuth,
        AuthTrade,
        EncryptionAuth,
#endif
        Password,
#if !__MOBILE__
        TaskBar,
        ExportAuth,
        ScriptStore,
        HideApp,
        IdleApp,
        ShareManage,
        ChangeBindPhoneNumber,
        UserProfile,
        NewVersion,
        WebView3,
        BindPhoneNumber,
#endif
    }
}