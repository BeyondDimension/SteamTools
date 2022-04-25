using System.Application.Models;
using System.Application.UI.Styles;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using XFApplication = Xamarin.Forms.Application;

namespace System.Application.UI
{
    public partial class App : XFApplication
    {
        public App(AppTheme theme)
        {
            InitializeComponent();
            AppTheme = theme.Convert();
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

                _AppTheme = value;

                var isDark = value == OSAppTheme.Dark;
                var themeRes = Resources.MergedDictionaries.First();
                if (isDark)
                {
                    if (themeRes is ThemeDark) return;
                }
                else
                {
                    if (themeRes is ThemeLight) return;
                }

                var resources = Resources.MergedDictionaries.Skip(1).ToArray();
                Resources.MergedDictionaries.Clear();

                themeRes = isDark ? new ThemeDark() : new ThemeLight();
                Resources.MergedDictionaries.Add(themeRes);

                Array.ForEach(resources, Resources.MergedDictionaries.Add);
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
