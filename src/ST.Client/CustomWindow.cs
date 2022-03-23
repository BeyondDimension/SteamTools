using System;
using System.Collections.Generic;
using System.Text;

namespace System.Application
{
    /// <summary>
    /// 业务自定义窗口
    /// </summary>
    public enum CustomWindow
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
        NewVersion,
        [Obsolete]
        BindPhoneNumber,
        ASF_AddBot,
        SteamShutdown,
        ProxySettings,
        Notice,
    }
}
