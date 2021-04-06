using System.Application.UI.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace System.Application.UI.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginPage : ContentPage
    {
        public LoginPage()
        {
            InitializeComponent();
            BindingContext = new LoginViewModel();
            TbPhoneNumber.ReturnCommand = new Command(() => TbSmsCode.Focus());
            TbSmsCode.ReturnCommand = new Command(() => ((LoginViewModel)BindingContext).LoginCommand.Execute(null));
        }
    }
}