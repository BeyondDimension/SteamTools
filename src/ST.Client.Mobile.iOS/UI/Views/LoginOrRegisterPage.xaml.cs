using ReactiveUI;
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
            BindingContext = new LoginOrRegisterPageViewModel
            {
                TbPhoneNumberReturnCommand = ReactiveCommand.Create<Entry>(textBox =>
                {
                    if (BindingContext is LoginOrRegisterPageViewModel vm) vm.SendSms.Invoke();
                    textBox?.Focus();
                }),
            };
        }
    }
}