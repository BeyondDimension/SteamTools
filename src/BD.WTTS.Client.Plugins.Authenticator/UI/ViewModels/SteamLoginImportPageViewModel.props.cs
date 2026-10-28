namespace BD.WTTS.UI.ViewModels;

public sealed partial class SteamLoginImportPageViewModel
{
    [Reactive]
    public bool IsLoading { get; set; }

    /// <summary>
    /// 进度状态 0 登录及登录验证 1 登录后继续 2 手机或邮箱验证码 3 完成
    /// </summary>
    [Reactive]
    public int SelectIndex { get; set; }

    /// <summary>
    /// 共享 Steam 登录流程
    /// </summary>
    [Reactive]
    public SteamLoginViewModel Login { get; private set; }

    /// <summary>
    /// 手机短信验证码
    /// </summary>
    [Reactive]
    public string? PhoneCodeText { get; set; }

    /// <summary>
    /// 恢复代码
    /// </summary>
    [Reactive]
    public string? RevocationCodeText { get; set; }

    [Reactive]
    public string? PhoneNumberText { get; set; }

    /// <summary>
    /// 是否已验证账号的手机号
    /// </summary>
    [Reactive]
    public bool IsVerifyAccountPhone { get; set; }
}
