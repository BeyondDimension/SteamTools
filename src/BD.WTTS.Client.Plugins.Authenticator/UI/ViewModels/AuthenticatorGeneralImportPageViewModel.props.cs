namespace BD.WTTS.UI.ViewModels;

public partial class AuthenticatorGeneralImportPageViewModel : ViewModelBase
{
    [Reactive]
    public partial string? AuthenticatorName { get; set; }

    [Reactive]
    public partial string? SecretCode { get; set; }

    [Reactive]
    public partial string? CurrentCode { get; set; } = "------";

    [Reactive]
    public partial AuthType AuthType { get; set; } = AuthType.TOTP;

    [Reactive]
    public partial HMACTypes HMACType { get; set; }

    /// <summary>
    /// 刷新间隔时间
    /// </summary>
    [Reactive]
    public partial int Period { get; set; } = 30;

    /// <summary>
    /// 令牌位数
    /// </summary>
    [Reactive]
    public partial int CodeDigits { get; set; } = 6;
}