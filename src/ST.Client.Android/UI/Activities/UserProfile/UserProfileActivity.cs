using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Binding;
using System.Application.UI.Adapters;
using System.Application.UI.Fragments;
using System.Application.UI.Resx;
using System.Application.UI.ViewModels;
using Fragment = AndroidX.Fragment.App.Fragment;
using NUserService = System.Application.Services.UserService;
using static System.Application.UI.ViewModels.UserProfileWindowViewModel;

namespace System.Application.UI.Activities
{
    [Authorize]
    [Register(JavaPackageConstants.Activities + nameof(UserProfileActivity))]
    [Activity(Theme = ManifestConstants.MainTheme2_NoActionBar,
         LaunchMode = LaunchMode.SingleTask,
         ConfigurationChanges = ManifestConstants.ConfigurationChanges)]
    internal sealed class UserProfileActivity : BaseActivity<activity_toolbar_tablayout_viewpager2, UserProfileWindowViewModel>, ViewPagerWithTabLayoutAdapter.IHost
    {
        protected override int? LayoutResource => Resource.Layout.activity_toolbar_tablayout_viewpager2;

        protected override void OnCreate2(Bundle? savedInstanceState)
        {
            base.OnCreate2(savedInstanceState);

            this.SetSupportActionBarWithNavigationClick(binding!.toolbar, true);

            R.Subscribe(() =>
            {
                Title = ViewModel!.Title;
            }).AddTo(this);

            var adapter = new ViewPagerWithTabLayoutAdapter(this, this);
            binding!.pager.SetupWithTabLayout(binding!.tab_layout, adapter);

            var position = this.GetViewModel<SubPageType>();
            GoToSubPage(position);
        }

        int ViewPagerAdapter.IHost.ItemCount => pageCount;

        Fragment ViewPagerAdapter.IHost.CreateFragment(int position) => position switch
        {
            0 => new UserBasicInfoFragment(),
            1 => new UserAccountBindFragment(),
            2 => NUserService.Current.HasPhoneNumber ? new ChangePhoneNumberFragment() : new BindPhoneNumberFragment(),
            _ => throw new ArgumentOutOfRangeException(nameof(position), position.ToString()),
        };

        string ViewPagerWithTabLayoutAdapter.IHost.GetPageTitle(int position) => position switch
        {
            0 => AppResources.User_BasicInfo,
            1 => AppResources.User_AccountBind,
            2 => NUserService.Current.HasPhoneNumber ? AppResources.User_ChangePhoneNum : AppResources.User_BindPhoneNum,
            _ => throw new ArgumentOutOfRangeException(nameof(position), position.ToString()),
        };

        public override void OnBackPressed()
        {
            var vm = ViewModel;
            if (vm != null)
            {
                if (vm.OnClose())
                {
                    return;
                }
            }

            base.OnBackPressed();
        }

        const int pageCount = 3;

        public static void StartActivity(Activity activity, SubPageType position = 0)
        {
            if (position.IsDefined())
            {
                GoToPlatformPages.StartActivity<UserProfileActivity, SubPageType>(activity, position);
            }
            else
            {
                activity.StartActivity<UserProfileActivity>();
            }
        }

        public void GoToSubPage(SubPageType position)
        {
            if (position.IsDefined())
            {
                binding!.pager.SetCurrentItem((int)position, false);
            }
        }

        protected override void OnDestroy()
        {
            ThirdPartyLoginHelper.DisposeServer();

            base.OnDestroy();
        }
    }
}