/*
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using AndroidX.Fragment.App;
using AndroidX.Navigation;
using AndroidX.Navigation.Fragment;
using AndroidX.Navigation.UI;
using Binding;
using ReactiveUI;
using System.Application.UI.Resx;
using System.Application.UI.ViewModels;
using CharSequence = Java.Lang.ICharSequence;

namespace System.Application.UI.Activities
{
    /// <summary>
    /// 使用 NavigationUI + BottomNavigationView 实现的 MainActivity
    /// <para>已知问题：</para>
    /// <para>NavigationUI 在切换时候使用 replace 而不是 show/hide 切换 Fragment，这会导致在切换时重建</para>
    /// <para>在 NavigationUI 2.4+ 中首个 Fragment 中的 View.Visibility 值不正确，隐藏或显示状态值错误</para>
    /// <para>当前首个 Fragment 为 CommunityFixFragment，且首次启动时列表不显示可能也与其相关</para>
    /// <para>有一些重写 FragmentNavigator 类的资料解决切换的问题，但其反射调用私有字段</para>
    /// <para>且当前 NavigationUI 版本已使用 kotlin 而不是 java 实现且内部方法字段改动过大，会有兼容性问题</para>
    /// <para>相关资料：</para>
    /// <para>https://github.com/android/architecture-components-samples/issues/530</para>
    /// <para>https://www.jianshu.com/p/adae9494d822</para>
    /// <para>https://zhuanlan.zhihu.com/p/143115625</para>
    /// <para>https://blog.csdn.net/m0_46958584/article/details/105430445</para>
    /// <para>解决方案：</para>
    /// <para>使用 MainActivity3 替代</para>
    /// </summary>
    [Obsolete("use MainActivity3")]
    [Register(JavaPackageConstants.Activities + nameof(MainActivity))]
    [Activity(Theme = ManifestConstants.MainTheme2_NoActionBar,
        LaunchMode = LaunchMode.SingleTask,
        ConfigurationChanges = ManifestConstants.ConfigurationChanges)]
    internal sealed partial class MainActivity : BaseActivity<activity_main>
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

        static MainActivity? mainActivity;
        public static MainActivity Instance => mainActivity ?? throw new ArgumentNullException(nameof(mainActivity));

        protected override void OnCreate2(Bundle? savedInstanceState)
        {
            base.OnCreate2(savedInstanceState);

            mainActivity = this;

            //InitCommunityFixVpnServiceActivityResultLauncher();

            SetSupportActionBar(binding!.toolbar);

            var appBarConfiguration = new AppBarConfiguration.Builder(
                Resource.Id.navigation_local_auth,
                Resource.Id.navigation_asf_plus,
                Resource.Id.navigation_community_fix,
                Resource.Id.navigation_game_list,
                Resource.Id.navigation_my)
                .Build();

            navController = ((NavHostFragment)SupportFragmentManager.FindFragmentById(Resource.Id.nav_host_fragment)).NavController;

            R.Subscribe(() =>
            {
                SetSubPageTitle(Resource.Id.navigation_local_auth, LocalAuthPageViewModel.DisplayName);
                SetSubPageTitle(Resource.Id.navigation_asf_plus, ArchiSteamFarmPlusPageViewModel.DisplayName);
                SetSubPageTitle(Resource.Id.navigation_my, MyPageViewModel.DisplayName);
                SetSubPageTitle(Resource.Id.navigation_community_fix, AppResources.CommunityFix);
                //SetSubPageTitle(Resource.Id.navigation_game_list, AppResources.GameList);
            }).AddTo(this);

            NavigationUI.SetupActionBarWithNavController(this, navController, appBarConfiguration);
            NavigationUI.SetupWithNavController(binding!.nav_view, navController);
        }

        protected override void OnNewIntent(Intent? intent)
        {
            base.OnNewIntent(intent);

            var action = intent?.Action;
            if (!string.IsNullOrWhiteSpace(action))
            {
                if (navController == null) return;
                if (Enum.TryParse<TabItemViewModel.TabItemId>(action, out var tabItem))
                {
                    var id = tabItem switch
                    {
                        TabItemViewModel.TabItemId.CommunityProxy => Resource.Id.navigation_community_fix,
                        TabItemViewModel.TabItemId.LocalAuth => Resource.Id.navigation_local_auth,
                        TabItemViewModel.TabItemId.ArchiSteamFarmPlus => Resource.Id.navigation_asf_plus,
                        _ => navController.CurrentDestination.Id,
                    };
                    if (id != navController.CurrentDestination.Id)
                    {
                        navController.Navigate(id);
                    }
                }
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            navController = null;
        }
    }
}
*/