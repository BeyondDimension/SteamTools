namespace BD.WTTS.UI.ViewModels;

public sealed partial class SteamLoginImportPageViewModel
{
    [Reactive]
    public bool IsLoading { get; set; }

    /// <summary>
    /// 进度状态  0 登录 1 绑定手机 2 手机验证码 3 完成
    /// </summary>
    [Reactive]
    public int SelectIndex { get; set; }

    /// <summary>
    /// 用户名
    /// </summary>
    [Reactive]
    public string? UserNameText { get; set; }

    /// <summary>
    /// 密码
    /// </summary>
    [Reactive]
    public string? PasswordText { get; set; }

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

    /// <summary>
    /// 邮箱验证码
    /// </summary>
    [Reactive]
    public string? EmailAuthText { get; set; }

    /// <summary>
    /// 是否需要2FA令牌
    /// </summary>
    [Reactive]
    public bool Requires2FA { get; set; }

    /// <summary>
    /// 文字验证码图片链接
    /// </summary>
    [Reactive]
    public string? CaptchaImageText { get; set; }

    /// <summary>
    /// 邮箱文本
    /// </summary>
    [Reactive]
    public string? EmailDomainText { get; set; }

    [Reactive]
    public string? PhoneNumberText { get; set; }

    /// <summary>
    /// 是否已验证账号的手机号
    /// </summary>
    [Reactive]
    public bool IsVerifyAccountPhone { get; set; }
}