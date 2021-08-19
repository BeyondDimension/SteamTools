using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Binding;
using Google.Android.Material.Tabs;
using ReactiveUI;
using System.Application.UI.Adapters;
using System.Application.UI.Fragments;
using System.Application.UI.Resx;
using System.Application.UI.ViewModels;
using static System.Application.UI.Resx.AppResources;
using Fragment = AndroidX.Fragment.App.Fragment;

namespace System.Application.UI.Activities
{
    [Register(JavaPackageConstants.Activities + nameof(AddAuthActivity))]
    [Activity(Theme = ManifestConstants.MainTheme_NoActionBar,
        LaunchMode = LaunchMode.SingleTask,
        ConfigurationChanges = ManifestConstants.ConfigurationChanges)]
    internal sealed class AddAuthActivity : BaseActivity<activity_toolbar_tablayout_viewpager2, AddAuthWindowViewModel>, ViewPagerWithTabLayoutAdapter.IHost
    {
        protected override int? LayoutResource => Resource.Layout.activity_toolbar_tablayout_viewpager2;

        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            this.SetSupportActionBarWithNavigationClick(binding!.toolbar, true);

            binding.tab_layout.TabMode = TabLayout.ModeScrollable;
            var adapter = new ViewPagerWithTabLayoutAdapter(this, this);
            binding!.pager.SetupWithTabLayout(binding!.tab_layout, adapter);

            R.Current.WhenAnyValue(x => x.Res).SubscribeInMainThread(_ =>
            {
                Title = AddAuthWindowViewModel.TitleName;
            }).AddTo(this);
        }

        int ViewPagerAdapter.IHost.ItemCount => 2;
        //4;

        Fragment ViewPagerAdapter.IHost.CreateFragment(int position) => position switch
        {
            //2 => new LocalAuthSteamUserImportFragment(),
            1 => new LocalAuthSteamAppImportFragment(),
            0 => new LocalAuthSteamToolsImportFragment.MLKitCameraX(),
            //3 => new LocalAuthOtherImportFragment(),
            _ => throw new ArgumentOutOfRangeException(nameof(position), position.ToString()),
        };

        string ViewPagerWithTabLayoutAdapter.IHost.GetPageTitle(int position) => position switch
        {
            //2 => LocalAuth_SteamUserImport,
            1 => LocalAuth_SteamAppImport,
            0 => LocalAuth_SteamToolsImport,
            //3 => LocalAuth_OtherImport,
            _ => throw new ArgumentOutOfRangeException(nameof(position), position.ToString()),
        };
    }
}