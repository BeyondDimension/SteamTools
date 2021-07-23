using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using AndroidX.Navigation;
using AndroidX.Navigation.UI;
using Binding;
using ReactiveUI;
using System.Application.UI.Resx;
using System.Application.UI.ViewModels;
using CharSequence = Java.Lang.ICharSequence;

namespace System.Application.UI.Activities
{
    [Register(JavaPackageConstants.Activities + nameof(MainActivity))]
    [Activity(Theme = ManifestConstants.MainTheme_NoActionBar,
        LaunchMode = LaunchMode.SingleTask,
        ConfigurationChanges = ManifestConstants.ConfigurationChanges)]
    internal sealed class MainActivity : BaseActivity<activity_main>
    {
        protected override int? LayoutResource => Resource.Layout.activity_main;

        protected override bool BackToHome => true;

        void SetBottomNavigationMenuTitle(int resId, CharSequence value)
        {
            if (binding == null) return;
            var menu = binding.nav_view.Menu;
            var menuItem = menu.FindItem(resId);
            if (menuItem == null) return;
            menuItem.SetTitle(value);
        }

        void SetSubPageTitle(int resId, string value)
        {
            var value_ = value.ToJavaString();
            SetBottomNavigationMenuTitle(resId, value_);
            this.SetNavigationGraphTitle(resId, value_);
        }

        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetSupportActionBar(binding!.toolbar);

            var appBarConfiguration = new AppBarConfiguration.Builder(
                Resource.Id.navigation_local_auth,
                Resource.Id.navigation_asf_plus,
                Resource.Id.navigation_community_fix,
                Resource.Id.navigation_game_list,
                Resource.Id.navigation_my)
                .Build();
            navController = Navigation.FindNavController(this, Resource.Id.nav_host_fragment_activity_main);

            R.Current.WhenAnyValue(x => x.Res).Subscribe(_ =>
            {
                SetSubPageTitle(Resource.Id.navigation_local_auth, LocalAuthPageViewModel.DisplayName);
                SetSubPageTitle(Resource.Id.navigation_my, MyPageViewModel.DisplayName);
            }).AddTo(this);

            NavigationUI.SetupActionBarWithNavController(this, navController, appBarConfiguration);
            NavigationUI.SetupWithNavController(binding!.nav_view, navController);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            navController = null;
        }
    }
}