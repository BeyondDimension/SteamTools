using System.Application.Services;
using System.Application.UI.Styles;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using XFApplication = Xamarin.Forms.Application;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace System.Application.UI
{
    public partial class App : XFApplication
    {
        public static new App Current => (App)XFApplication.Current;

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
            set
            {
                var appRes = Resources.MergedDictionaries.Skip(1).ToArray();
                Resources.MergedDictionaries.Clear();
                ResourceDictionary appThemeRes = value == OSAppTheme.Dark ? new ThemeDark() : new ThemeLight();
                Resources.MergedDictionaries.Add(appThemeRes);
                Array.ForEach(appRes, Resources.MergedDictionaries.Add);
            }
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