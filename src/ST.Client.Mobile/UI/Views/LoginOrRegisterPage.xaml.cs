using System.Application.UI.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace System.Application.UI.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    partial class LoginOrRegisterPage
    {
        public LoginOrRegisterPage()
        {
            InitializeComponent();
            BindingContext = new LoginOrRegisterPageViewModel();
            TbPhoneNumber.ReturnCommand = new Command(() => TbSmsCode.Focus());
            TbSmsCode.ReturnCommand = new Command(() =>
            {
                if (BindingContext is LoginOrRegisterPageViewModel vm)
                {
                    vm.Submit.Invoke();
                }
            });
        }
    }
}