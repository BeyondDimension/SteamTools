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
    public AuthType AuthType { get; set; }

    [Reactive]
    public HMACTypes HMACType { get; set; }

    [Reactive]
    public int CodeDigits { get; set; } = 30;

    [Reactive]
    public int Period { get; set; } = 6;
}