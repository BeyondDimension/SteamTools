using System.Application.UI.Views;
using Xamarin.Forms;

namespace System.Application.UI
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(ItemDetailPage), typeof(ItemDetailPage));
            Routing.RegisterRoute(nameof(NewItemPage), typeof(NewItemPage));
            Routing.RegisterRoute("LoginOrRegister/PhoneNumber", typeof(LoginOrRegisterPage));
        }
    }
}