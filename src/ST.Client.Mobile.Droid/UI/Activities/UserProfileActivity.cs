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

namespace System.Application.UI.Activities
{
    [Register(JavaPackageConstants.Activities + nameof(UserProfileActivity))]
    [Activity(Theme = ManifestConstants.MainTheme,
         LaunchMode = LaunchMode.SingleTask,
         ConfigurationChanges = ManifestConstants.ConfigurationChanges)]
    internal sealed class UserProfileActivity : BaseActivity<activity_toolbar_tablayout_viewpager2, UserProfilePageViewModel>, ViewPagerWithTabLayoutAdapter.IHost
    {
        protected override int? LayoutResource => Resource.Layout.activity_toolbar_tablayout_viewpager2;

        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            var adapter = new ViewPagerWithTabLayoutAdapter(this, this);
            binding!.pager.SetupWithTabLayout(binding!.tab_layout, adapter);
        }

        int ViewPagerAdapter.IHost.ItemCount => 2;

        Fragment ViewPagerAdapter.IHost.CreateFragment(int position) => position switch
        {
            0 => new UserBasicInfoFragment(),
            1 => new UserAccountBindFragment(),
            _ => throw new ArgumentOutOfRangeException(nameof(position), position.ToString()),
        };

        string ViewPagerWithTabLayoutAdapter.IHost.GetPageTitle(int position) => position switch
        {
            0 => AppResources.User_BasicInfo,
            1 => AppResources.User_AccountBind,
            _ => throw new ArgumentOutOfRangeException(nameof(position), position.ToString()),
        };
    }
}