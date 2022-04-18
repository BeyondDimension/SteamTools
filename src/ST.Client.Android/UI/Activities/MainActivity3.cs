using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Binding;
using System.Application.UI.Adapters;
using System.Application.UI.Fragments;
using System.Application.UI.Resx;
using System.Application.UI.ViewModels;
using Google.Android.Material.BottomNavigation;
using CharSequence = Java.Lang.ICharSequence;
using Fragment = AndroidX.Fragment.App.Fragment;
using Google.Android.Material.Navigation;
using AndroidX.ViewPager2.Widget;

namespace System.Application.UI.Activities
{
    /// <summary>
    /// 使用 ViewPager2 + BottomNavigationView 实现的 MainActivity
    /// </summary>
    [Register(JavaPackageConstants.Activities + nameof(MainActivity))]
    [Activity(Theme = ManifestConstants.MainTheme2_NoActionBar,
       LaunchMode = LaunchMode.SingleTask,
       ConfigurationChanges = ManifestConstants.ConfigurationChanges)]
    internal sealed partial class MainActivity : BaseActivity<activity_main_3>, ViewPagerAdapter.IHost, BottomNavigationView.IOnItemSelectedListener
    {
        protected override int? LayoutResource => Resource.Layout.activity_main_3;

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

        (Type fragmentType, Func<string> getTitle, int menuId, TabItemViewModel.TabItemId tabItemId)[]? fragments;

        protected override void OnCreate2(Bundle? savedInstanceState)
        {
            base.OnCreate2(savedInstanceState);

            mainActivity = this;

            fragments = new (Type fragmentType, Func<string> getTitle, int menuId, TabItemViewModel.TabItemId tabItemId)[]
            {
                 (typeof(LocalAuthFragment), () => LocalAuthPageViewModel.DisplayName, Resource.Id.navigation_local_auth, TabItemViewModel.TabItemId.LocalAuth),
                 (typeof(ASFPlusFragment), () => ArchiSteamFarmPlusPageViewModel.DisplayName, Resource.Id.navigation_asf_plus, TabItemViewModel.TabItemId.ArchiSteamFarmPlus),
                 (typeof(CommunityFixFragment), () => AppResources.CommunityFix, Resource.Id.navigation_community_fix, TabItemViewModel.TabItemId.CommunityProxy),
                 (typeof(MyFragment), () => MyPageViewModel.DisplayName, Resource.Id.navigation_my, default),
            };

            SetSupportActionBar(binding!.toolbar);

            R.Subscribe(() =>
            {
                if (binding == null) return;
                foreach (var (_, getTitle, menuId, _) in fragments)
                {
                    var title = getTitle();
                    SetSubPageTitle(menuId, title);
                }
                SetTitleByPosition(binding.viewPager2.CurrentItem);
            }).AddTo(this);

            var adapter = new ViewPagerAdapter(this, this);
            binding.viewPager2.Adapter = adapter;
            binding.viewPager2.RegisterOnPageChangeCallback(new OnPageChangeCallback(this));
            binding.nav_view.SetOnItemSelectedListener(this);
        }

        sealed class OnPageChangeCallback : ViewPager2.OnPageChangeCallback
        {
            readonly MainActivity thiz;

            public OnPageChangeCallback(MainActivity thiz)
            {
                this.thiz = thiz;
            }

            public override void OnPageSelected(int position)
            {
                thiz.OnPageSelected(position);
            }
        }

        protected override void OnNewIntent(Intent? intent)
        {
            base.OnNewIntent(intent);

            if (binding == null || fragments == null) return;
            var action = intent?.Action;
            if (!string.IsNullOrWhiteSpace(action))
            {
                if (Enum.TryParse<TabItemViewModel.TabItemId>(action, out var tabItem))
                {
                    for (int i = 0; i < fragments!.Length; i++)
                    {
                        if (fragments[i].tabItemId == tabItem)
                        {
                            Navigate(i);
                        }
                    }
                }
            }
        }

        int ViewPagerAdapter.IHost.ItemCount => fragments!.Length;

        Fragment ViewPagerAdapter.IHost.CreateFragment(int position) => (Fragment)Activator.CreateInstance(fragments![position].fragmentType);

        bool NavigationBarView.IOnItemSelectedListener.OnNavigationItemSelected(IMenuItem item)
        {
            if (binding == null || fragments == null) return false;
            var itemId = item.ItemId;
            for (int i = 0; i < fragments!.Length; i++)
            {
                if (fragments[i].menuId == itemId)
                {
                    Navigate(i);
                    return true;
                }
            }
            return false;
        }

        void Navigate(int position)
        {
            binding!.viewPager2.SetCurrentItem(position, false);
            SetTitleByPosition(position);
            SetChecked(position);
        }

        void SetTitleByPosition(int position)
        {
            var title = fragments![position].getTitle();
            Title = title;
        }

        void OnPageSelected(int position)
        {
            if (binding == null) return;
            SetTitleByPosition(position);
            SetChecked(position);
        }

        void SetChecked(int position)
        {
            var menuItem = binding!.nav_view.Menu.FindItem(fragments![position].menuId);
            if (menuItem != null && !menuItem.IsChecked) menuItem.SetChecked(true);
        }
    }
}
