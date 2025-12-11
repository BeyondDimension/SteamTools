namespace BD.WTTS.UI.ViewModels;

public partial class AuthenticatorGeneralImportPageViewModel : ViewModelBase
{
    [Reactive]
    public string? AuthenticatorName { get; set; }

    [Reactive]
    public string? SecretCode { get; set; }

    [Reactive]
    public string? CurrentCode { get; set; } = "------";

    [Reactive]
    public AuthType AuthType { get; set; } = AuthType.TOTP;

    [Reactive]
    public HMACTypes HMACType { get; set; }

    /// <summary>
    /// 刷新间隔时间
    /// </summary>
    [Reactive]
    public int Period { get; set; } = 30;

    /// <summary>
    /// 令牌位数
    /// </summary>
    [Reactive]
    public int CodeDigits { get; set; } = 6;
}