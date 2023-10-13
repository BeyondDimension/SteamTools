using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BD.WTTS.UI.ViewModels;

public sealed partial class IdleSteamLoginPageViewModel
{
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
    /// 2FA 验证码
    /// </summary>
    [Reactive]
    public string? TwofactorCode { get; set; }

    /// <summary>
    /// 是否需要2FA令牌
    /// </summary>
    [Reactive]
    public bool Requires2FA { get; set; }

    /// <summary>
    /// 是否需要邮箱验证码
    /// </summary>
    [Reactive]
    public bool RequiresEmailAuth { get; set; }

    /// <summary>
    /// 是否记住登录状态
    /// </summary>
    [Reactive]
    public bool RemenberLogin { get; set; } = true;

    [Reactive]
    public bool IsLoading { get; set; }

    public ICommand Login { get; set; }
}
