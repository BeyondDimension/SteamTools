using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BD.WTTS.Models;
public sealed partial class SteamLoginImportViewModel : TabItemViewModel
{
    public override string Name => nameof(SteamLoginImportViewModel);
    
    private int index;

    public int SelectIndex
    {
        get => index;
        set
        {
            this.RaiseAndSetIfChanged(ref index, value);
        }
    }

    private string? username;

    /// <summary>
    /// 用户名
    /// </summary>
    public string? UserNameText
    {
        get => username;
        set
        {
            this.RaiseAndSetIfChanged(ref username, value);
        }
    }

    private string? password;

    /// <summary>
    /// 密码
    /// </summary>
    public string? PasswordText
    {
        get => password;
        set
        {
            this.RaiseAndSetIfChanged(ref password, value);
        }
    }

    private string? captchacode;

    /// <summary>
    /// 文字验证码
    /// </summary>
    public string? CaptchaCodeText
    {
        get => captchacode;
        set
        {
            this.RaiseAndSetIfChanged(ref captchacode, value);
        }
    }

    private string? phonecode;

    /// <summary>
    /// 手机验证码
    /// </summary>
    public string? PhoneCodeText
    {
        get => phonecode;
        set
        {
            this.RaiseAndSetIfChanged(ref phonecode, value);
        }
    }

    private string? revocationcode;

    /// <summary>
    /// 恢复代码
    /// </summary>
    public string? RevocationCodeText
    {
        get => revocationcode;
        set
        {
            this.RaiseAndSetIfChanged(ref revocationcode, value);
        }
    }

    private bool emailcodetabcanskip;

    /// <summary>
    /// 邮箱验证码步骤是否可跳过
    /// </summary>
    public bool EmailCodeTabCanSkip
    {
        get => emailcodetabcanskip;
        set
        {
            this.RaiseAndSetIfChanged(ref emailcodetabcanskip, value);
        }
    }

    bool emailcodetabcannext;

    private string? emailauth;

    /// <summary>
    /// 邮箱验证码
    /// </summary>
    public string? EmailAuthText
    {
        get => emailauth;
        set
        {
            this.RaiseAndSetIfChanged(ref emailauth, value);
        }
    }

    private bool captchaimagetabcanskip;

    /// <summary>
    /// 文字验证码页面是否可跳过
    /// </summary>
    public bool CaptchaImageTabCanSkip
    {
        get => captchaimagetabcanskip;
        set
        {
            this.RaiseAndSetIfChanged(ref captchaimagetabcanskip, value);
        }
    }

    private string? captchaimage;

    /// <summary>
    /// 文字验证码图片链接
    /// </summary>
    public string? CaptchaImageText
    {
        get => captchaimage;
        set
        {
            this.RaiseAndSetIfChanged(ref captchaimage, value);
        }
    }

    private string? emaildomain;

    /// <summary>
    /// 邮箱文本
    /// </summary>
    public string? EmailDomainText
    {
        get => emaildomain;
        set
        {
            this.RaiseAndSetIfChanged(ref emaildomain, value);
        }
    }

    private bool finatabisvisible;

    /// <summary>
    /// 令牌添加结束页面是否显示
    /// </summary>
    public bool FinalTabIsVisible
    {
        get => finatabisvisible;
        set
        {
            this.RaiseAndSetIfChanged(ref finatabisvisible, value);
        }
    }
}
