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
        AddAuth,
        ShowAuth,
        AuthTrade,
        EncryptionAuth,
        Password,
#if !__MOBILE__
        TaskBar,
#endif
        ExportAuth,
#if !__MOBILE__
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
#if __MOBILE__
        TextBox,
#endif
    }
}