using System.Application.UI.Views;
using Xamarin.Forms;

namespace System.Application.UI
{
    public partial class AppShell : Shell
    {
        public const string Route_LoginOrRegister_Secondary = "LoginOrRegister/Secondary";

        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(ItemDetailPage), typeof(ItemDetailPage));
            Routing.RegisterRoute(nameof(NewItemPage), typeof(NewItemPage));
            Routing.RegisterRoute(Route_LoginOrRegister_Secondary, typeof(LoginOrRegisterPage));
        }
    }
}