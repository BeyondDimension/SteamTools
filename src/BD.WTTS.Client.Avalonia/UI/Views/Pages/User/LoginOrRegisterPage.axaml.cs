namespace BD.WTTS.UI.Views.Pages;

public partial class LoginOrRegisterPage : ReactiveUserControl<LoginOrRegisterWindowViewModel>
{
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
        // 11.1 使用新 API  https://docs.avaloniaui.net/docs/concepts/services/activatable-lifetime#handling-uri-activation
        //if (Application.Current.TryGetFeature<IActivatableLifetime>() is { } activatableLifetime)
        //{
        //    activatableLifetime.Activated += (s, a) =>
        //    {
        //        if (a is ProtocolActivatedEventArgs protocolArgs && protocolArgs.Kind == ActivationKind.OpenUri)
        //        {
        //            Console.WriteLine($"App activated via Uri: {protocolArgs.Uri}");
        //        }
        //    };
        //}
        Application.Current!.UrlsOpened += Current_UrlsOpened;
    }

    private async void Current_UrlsOpened(object? sender, UrlOpenedEventArgs e)
    {
        var loginUrl = e.Urls.Where(x => x.StartsWith(Constants.UrlSchemes.Login)).FirstOrDefault();
        if (loginUrl != null)
        {
            //var token = loginUrl.TrimStart(Constants.UrlSchemes.Login);
            var token = loginUrl[Constants.UrlSchemes.Login.Length..];
            await ThirdPartyLoginHelper.LoginForStr(token);
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
        Application.Current!.UrlsOpened -= Current_UrlsOpened;
        base.OnDetachedFromVisualTree(e);
        if (DataContext is LoginOrRegisterWindowViewModel vm)
        {
            vm.RemoveAllDelegate();
        }
    }
}
