namespace BD.WTTS.UI.Views.Pages;

public partial class LoginOrRegisterPage : ReactiveUserControl<LoginOrRegisterWindowViewModel>
{
    private IActivatableLifetime? activatableLifetime;

    public LoginOrRegisterPage()
    {
        InitializeComponent();

        TbPhoneNumber.KeyUp += (_, e) =>
        {
            if (e.Key == Key.Return)
            {
                if (DataContext is LoginOrRegisterWindowViewModel vm)
                {
                    vm.SendSms.Invoke();
                    e.Handled = true;
                }
                TbSmsCode.Focus();
            }
        };
        TbSmsCode.KeyUp += (_, e) =>
        {
            if (e.Key == Key.Return)
            {
                if (DataContext is LoginOrRegisterWindowViewModel vm)
                {
                    vm.Submit.Invoke();
                    e.Handled = true;
                }
            }
        };
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        this.SetViewModel<LoginOrRegisterWindowViewModel>();

        if (this.ViewModel != null)
        {
            this.ViewModel.LoginState = 0;
            if (UserService.Current.IsAuthenticated)
            {
                Toast.Show(ToastIcon.Info, "当前已是登录状态");
                this.ViewModel.Close(false);
            }
        }
        if (activatableLifetime == null &&
            Application.Current?.TryGetFeature<IActivatableLifetime>() is { } lifetime)
        {
            activatableLifetime = lifetime;
            activatableLifetime.Activated += Current_Activated;
        }
    }

    private async void Current_Activated(object? sender, ActivatedEventArgs e)
    {
        if (e is ProtocolActivatedEventArgs
            {
                Kind: ActivationKind.OpenUri,
                Uri: { } uri,
            })
        {
            var loginUrl = uri.ToString();
            if (loginUrl.StartsWith(Constants.UrlSchemes.Login))
            {
                var token = loginUrl[Constants.UrlSchemes.Login.Length..];
                await ThirdPartyLoginHelper.LoginForStr(token);
            }
        }
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        if (DataContext is LoginOrRegisterWindowViewModel vm)
        {
            vm.TbPhoneNumberFocus = () => TbPhoneNumber.Focus();
            vm.TbSmsCodeFocus = () => TbSmsCode.Focus();
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        if (activatableLifetime != null)
        {
            activatableLifetime.Activated -= Current_Activated;
            activatableLifetime = null;
        }
        base.OnDetachedFromVisualTree(e);
        if (DataContext is LoginOrRegisterWindowViewModel vm)
        {
            vm.RemoveAllDelegate();
        }
    }
}
