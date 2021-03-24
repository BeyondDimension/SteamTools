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
            this.BindingContext = new LoginViewModel();
        }
    }
}