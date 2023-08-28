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
        this.SetViewModel<LoginOrRegisterWindowViewModel>(false);
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
        base.OnDetachedFromVisualTree(e);
        if (DataContext is LoginOrRegisterWindowViewModel vm)
        {
            vm.RemoveAllDelegate();
        }
    }
}
