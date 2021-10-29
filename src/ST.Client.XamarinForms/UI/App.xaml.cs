using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using XFApplication = Xamarin.Forms.Application;

namespace System.Application.UI
{
    public partial class App : XFApplication
    {
        public App()
        {
            InitializeComponent();
            MainPage = new AppShell
            {
                Visual = VisualMarker.Material,
            };
        }

        protected override void OnStart()
        {

        }

        protected override void OnSleep()
        {

        }

        protected override void OnResume()
        {

        }
    }
}
