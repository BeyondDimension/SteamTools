using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using AndroidX.Navigation;
using AndroidX.Navigation.UI;
using Binding;
using Java.Lang;
using ReactiveUI;
using System.Application.UI.Resx;
using System.Application.UI.ViewModels;

namespace System.Application.UI.Activities
{
    [Register(JavaPackageConstants.Activities + nameof(MainActivity))]
    [Activity(Theme = ManifestConstants.MainTheme_NoActionBar,
        LaunchMode = LaunchMode.SingleTask,
        ConfigurationChanges = ManifestConstants.ConfigurationChanges)]
    internal sealed class MainActivity : BaseActivity<activity_main>, IReadOnlyViewFor<MyPageViewModel>, IReadOnlyViewFor<LocalAuthPageViewModel>
    {
        public static MainActivity? Instance { get; private set; }

        protected override int? LayoutResource => Resource.Layout.activity_main;

        protected override bool BackToHome => true;

        public MyPageViewModel MyPageViewModel { get; } = new();

        public LocalAuthPageViewModel LocalAuthPageViewModel { get; } = new();

        MyPageViewModel? IReadOnlyViewFor<MyPageViewModel>.ViewModel => MyPageViewModel;

        LocalAuthPageViewModel? IReadOnlyViewFor<LocalAuthPageViewModel>.ViewModel => LocalAuthPageViewModel;

        void SetBottomNavigationMenuTitle(int resId, ICharSequence value)
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

            MyPageViewModel.AddTo(this);

            var appBarConfiguration = new AppBarConfiguration.Builder(
                Resource.Id.navigation_local_auth,
                Resource.Id.navigation_community_fix,
                Resource.Id.navigation_game_list,
                Resource.Id.navigation_my)
                .Build();
            navController = Navigation.FindNavController(this, Resource.Id.nav_host_fragment_activity_main);
            NavigationUI.SetupActionBarWithNavController(this, navController, appBarConfiguration);
            NavigationUI.SetupWithNavController(binding!.nav_view, navController);

            R.Current.WhenAnyValue(x => x.Res).Subscribe(_ =>
            {
                SetSubPageTitle(Resource.Id.navigation_local_auth, LocalAuthPageViewModel.DisplayName);
                SetSubPageTitle(Resource.Id.navigation_my, MyPageViewModel.DisplayName);
            }).AddTo(this);

            Instance = this;
        }

        protected override void OnDestroy()
        {
            Instance = null;
            base.OnDestroy();
            navController = null;
        }
    }
}