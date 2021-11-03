using System.Application.UI.Styles;
using System.Linq;
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
            AppTheme = RequestedTheme;
            RequestedThemeChanged += (_, e) => AppTheme = e.RequestedTheme;
            MainPage = new AppShell
            {
                Visual = VisualMarker.Material,
            };
        }

        OSAppTheme _AppTheme = OSAppTheme.Light;
        public OSAppTheme AppTheme
        {
            get => _AppTheme;
            set
            {
                if (value == OSAppTheme.Unspecified) value = OSAppTheme.Light;
                if (_AppTheme == value) return;
                var res = Resources.MergedDictionaries.Skip(1).ToArray();
                Resources.MergedDictionaries.Clear();
                ResourceDictionary theme = value == OSAppTheme.Dark ?
                    new ThemeDark() : new ThemeLight();
                Resources.MergedDictionaries.Add(theme);
                Array.ForEach(res, Resources.MergedDictionaries.Add);
                _AppTheme = value;
            }
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
