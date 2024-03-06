using BD.WTTS.Client.Resources;

namespace BD.WTTS.UI.ViewModels;

public partial class LoginOrRegisterWindowViewModel : WindowViewModel, SendSmsUIHelper.IViewModel
{
    public static string DisplayName => Strings.LoginAndRegister;

    public LoginOrRegisterWindowViewModel()
    {
        Title = DisplayName;

        Close = _ => INavigationService.Instance.GoBack(typeof(LoginOrRegisterWindowViewModel));

        ChooseChannel = ReactiveCommand.CreateFromTask<string>(async channel_ =>
        {
            if (Enum.TryParse<ExternalLoginChannel>(channel_, out var channel))
            {
                CurrentSelectChannel = channel_;
                ChangeLoginState(2);
                await ThirdPartyLoginHelper.StartAsync(this, channel, isBind: false);
            }
            else
            {
                switch (channel_)
                {
                    case ExternalLoginChannelViewModel.PhoneNumber:
                        ChangeLoginState(1);
                        break;
                }
            }
        });
        ManualLogin = ThirdPartyLoginHelper.ManualLogin;
        SendSms = ReactiveCommand.CreateFromTask(SendSmsAsync);
        Submit = ReactiveCommand.CreateFromTask(SubmitAsync);
        OpenHyperlink = ReactiveCommand.Create<string>(OpenHyperlink_);
        ChangeState = ReactiveCommand.Create<short>(ChangeLoginState);

        ExternalLoginChannels = new(GetFastLoginChannels());
    }

    internal static async Task SuccessAsync(LoginOrRegisterResponse rsp, Action<bool>? close = null)
    {
        await UserService.Current.RefreshUserAsync();
        var msg = Strings.Success_.Format((rsp?.IsLoginOrRegister ?? false) ? Strings.User_Login : Strings.User_Register);
        close?.Invoke(false);
        Toast.Show(ToastIcon.Success, msg);
        UserService.Current.RefreshShopToken();
    }

    async Task SendSmsAsync() => await this.SendSmsAsync(() => new()
    {
        PhoneNumber = PhoneNumber,
        Type = SmsCodeType.LoginOrRegister,
    });

    async Task SubmitAsync()
    {
        if (IsLoading || !this.CanSubmit()) return;

        var request = new LoginOrRegisterRequest
        {
            PhoneNumber = PhoneNumber,
            SmsCode = SmsCode
        };
        IsLoading = true;

        var response = await IMicroServiceClient.Instance.Account.LoginOrRegister(request);

        if (response.IsSuccess)
        {
            await SuccessAsync(response.Content!, Close);
            return;
        }

        IsLoading = false;
    }

    async void OpenHyperlink_(string parameter)
    {
        if (string.IsNullOrWhiteSpace(parameter)) return;
        switch (parameter)
        {
            case Agreement:
                parameter = Constants.Urls.OfficialWebsite_Box_Agreement;
                break;
            case Privacy:
                parameter = Constants.Urls.OfficialWebsite_Box_Privacy;
                break;
        }
        await Browser2.OpenAsync(parameter);
    }

    IEnumerable<ExternalLoginChannelViewModel> GetFastLoginChannels()
    {
        foreach (var item in ThirdPartyLoginHelper.ExternalLoginChannels)
        {
            if (item.IsSupported())
            {
                yield return ExternalLoginChannelViewModel.Create(item switch
                {
                    ExternalLoginChannel.Microsoft => nameof(ExternalLoginChannel.Xbox),
                    _ => item.ToString(),
                }, this);
            }
        }

        if (!IApplication.IsDesktop())
        {
            yield return ExternalLoginChannelViewModel.Create(ExternalLoginChannelViewModel.PhoneNumber, this);
        }
    }

    void ChangeLoginState(short state)
    {
        LoginState = state;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            CTS?.Cancel();
        }
        base.Dispose(disposing);
    }
}
