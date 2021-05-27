using System.Application.Services;
using System.Application.UI.Styles;
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
            XF.Material.Forms.Material.Init(this);
            AppTheme = RequestedTheme;
            RequestedThemeChanged += (_, e) => AppTheme = e.RequestedTheme;
            DependencyService.Register<MockDataStore>();
            MainPage = new AppShell();
        }

        public OSAppTheme AppTheme
        {
            set => Resources = value switch
            {
                OSAppTheme.Dark => new ThemeDark(),
                _ => new ThemeLight(),
            };
        }

        protected override void OnStart()
        {
            Toast.Show("It Just Works");
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}