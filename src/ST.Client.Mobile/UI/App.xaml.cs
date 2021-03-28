using System.Application.Proxy;
using System.Application.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using XFApplication = Xamarin.Forms.Application;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace System.Application.UI
{
    public partial class App : XFApplication
    {
        public App()
        {
            InitializeComponent();

            DependencyService.Register<MockDataStore>();
            MainPage = new AppShell();
        }

        readonly ProxyTestController controller = new();

        protected override void OnStart()
        {
            controller.StartProxy();
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}