using System.Drawing;
using BD.WTTS.Client.Resources;
using static BD.WTTS.ApiConstants;

namespace BD.WTTS.UI.ViewModels;

public sealed class ExternalLoginChannelViewModel : RIdTitleIconViewModel<string, ResIcon>
{
    ExternalLoginChannelViewModel()
    {
    }

    Color iconBgColor;

    public Color IconBgColor
    {
        get => iconBgColor;
        set => this.RaiseAndSetIfChanged(ref iconBgColor, value);
    }

    protected override void OnIdChanged(string? value)
    {
        IconBgColor = GetIconBgColorById(value);
    }

    public const string PhoneNumber = nameof(PhoneNumber);

    protected override ResIcon GetIconById(string? id)
    {
        return id switch
        {
            nameof(ExternalLoginChannel.Steam) => ResIcon.Steam,
            nameof(ExternalLoginChannel.Xbox) or
            nameof(ExternalLoginChannel.Microsoft) => ResIcon.Xbox,
            nameof(ExternalLoginChannel.Apple) => ResIcon.Apple,
            nameof(ExternalLoginChannel.QQ) => ResIcon.QQ,
            PhoneNumber => ResIcon.Phone,
            _ => ResIcon.None,
        };
    }

    protected override string GetTitleById(string? id)
    {
        if (id == PhoneNumber)
        {
            return Strings.User_UsePhoneNumLoginChannel;
        }
        else if (!string.IsNullOrWhiteSpace(id))
        {
            if (id == nameof(ExternalLoginChannel.Xbox) ||
                id == nameof(ExternalLoginChannel.Microsoft))
            {
                id = "Xbox Live";
            }
            return Strings.User_UseExternalLoginChannel_.Format(id);
        }
        return string.Empty;
    }

    static Color GetIconBgColorById(string? id)
    {
        var hexColor = id switch
        {
            nameof(ExternalLoginChannel.Steam) => "#145c8f",
            nameof(ExternalLoginChannel.Xbox) or
            nameof(ExternalLoginChannel.Microsoft) => "#027d00",
            nameof(ExternalLoginChannel.Apple) => "#000000",
            nameof(ExternalLoginChannel.QQ) => "#12B7F5",
            PhoneNumber => "#2196F3",
            _ => default,
        };
        return hexColor == default ? default : ColorConverters.FromHex(hexColor);
    }

    /// <summary>
    /// 创建实例
    /// </summary>
    /// <param name="id"></param>
    /// <param name="vm"></param>
    /// <returns></returns>
    public static ExternalLoginChannelViewModel Create(string id, IDisposableHolder vm)
    {
        ExternalLoginChannelViewModel r = new() { Id = id, };
        r.OnBind(vm);
        return r;
    }
}

public partial class LoginOrRegisterWindowViewModel
{
    public const string Agreement = "Agreement";
    public const string Privacy = "Privacy";

    [Reactive]
    public string? CurrentSelectChannel { get; set; }

    [Reactive]
    public string? PhoneNumber { get; set; }

    [Reactive]
    public string? SmsCode { get; set; }

    private int _TimeLimit = SMSInterval;

    public int TimeLimit
    {
        get => _TimeLimit;
        set
        {
            this.RaiseAndSetIfChanged(ref _TimeLimit, value);
            this.RaisePropertyChanged(nameof(IsUnTimeLimit));
        }
    }

    public static string DefaultBtnSendSmsCodeText => Strings.User_GetSMSCode;

    [Reactive]
    public string BtnSendSmsCodeText { get; set; } = DefaultBtnSendSmsCodeText;

    public bool IsUnTimeLimit => TimeLimit != SMSInterval;

    public bool SendSmsCodeSuccess { get; set; }

    [Reactive]
    public bool IsLoading { get; set; }

    [Reactive]
    public bool IsFastLogin { get; set; }

    [Reactive]
    public short LoginState { get; set; }

    //public SteamUser? SteamUser { get; } = SteamConnectService.Current.CurrentSteamUser;

    public ICommand Submit { get; }

    public Action? TbPhoneNumberFocus { get; set; }

    public Action? TbSmsCodeFocus { get; set; }

    public CancellationTokenSource? CTS { get; set; }

    public ICommand SendSms { get; }

    public ICommand OpenHyperlink { get; }

    public ICommand ManualLogin { get; }

    public ICommand ChangeState { get; }

    /// <summary>
    /// 选择快速登录渠道点击命令，参数类型为 <see cref="FastLoginChannelViewModel"/>.Id
    /// </summary>
    public ICommand ChooseChannel { get; }

    /// <summary>
    /// 快速登录渠道组
    /// </summary>
    [Reactive]
    public ObservableCollection<ExternalLoginChannelViewModel> ExternalLoginChannels { get; set; }
}
