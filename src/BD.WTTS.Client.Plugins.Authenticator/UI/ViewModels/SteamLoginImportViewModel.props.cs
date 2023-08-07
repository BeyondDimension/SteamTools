using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BD.WTTS.UI.ViewModels;

public sealed partial class SteamLoginImportViewModel
{
    [Reactive]
    public bool IsLoading { get; set; }

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
    /// 文字验证码
    /// </summary>
    [Reactive]
    public string? CaptchaCodeText { get; set; }

    /// <summary>
    /// 手机验证码
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
}
