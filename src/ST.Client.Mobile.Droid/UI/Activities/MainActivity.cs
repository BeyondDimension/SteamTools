using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using AndroidX.Navigation;
using AndroidX.Navigation.UI;
using Binding;
using Java.Lang;
using ReactiveUI;
using System.Application.Mvvm;
using System.Application.UI.ViewModels;
using System.Collections.Generic;
using System.Reactive.Disposables;

namespace System.Application.UI.Activities
{
    [Register(JavaPackageConstants.Activities + nameof(MainActivity))]
    [Activity(Theme = "@style/MainTheme",
        LaunchMode = LaunchMode.SingleTask,
        ConfigurationChanges = ManifestConstants.ConfigurationChanges)]
    internal sealed class MainActivity : BaseActivity<activity_main>, IDisposableHolder, IReadOnlyViewFor<MyPageViewModel>
    {
        readonly CompositeDisposable disposables = new();
        ICollection<IDisposable> IDisposableHolder.CompositeDisposable => disposables;

        protected override int? LayoutResource => Resource.Layout.activity_main;

        protected override bool BackToHome => true;

        public MyPageViewModel MyPageViewModel { get; } = new();

        MyPageViewModel? IReadOnlyViewFor<MyPageViewModel>.ViewModel => MyPageViewModel;

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

            MyPageViewModel.WhenAnyValue(x => x.Title).Subscribe(value =>
            {
                SetSubPageTitle(Resource.Id.navigation_my, value);
            }).AddTo(this);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            navController = null;
            disposables.Dispose();
        }
    }
}