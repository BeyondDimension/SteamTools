using Android.Runtime;
using Android.Views;
using AndroidX.Fragment.App;
using Binding;
using ReactiveUI;
using System.Application.Services;
using System.Application.UI.Adapters;
using System.Application.UI.Resx;
using System.Application.UI.ViewModels;
using static System.Application.UI.Resx.AppResources;
using Fragment = AndroidX.Fragment.App.Fragment;

namespace System.Application.UI.Fragments
{
    internal abstract class ASFPlusFragment<TViewBinding> : BaseFragment<TViewBinding, ArchiSteamFarmPlusPageViewModel> where TViewBinding : class
    {
        protected sealed override ArchiSteamFarmPlusPageViewModel? OnCreateViewModel()
        {
            return IViewModelManager.Instance.GetMainPageViewModel<ArchiSteamFarmPlusPageViewModel>();
        }
    }

    [Register(JavaPackageConstants.Fragments + nameof(ASFPlusFragment))]
    internal sealed class ASFPlusFragment : ASFPlusFragment<activity_tablayout_viewpager2>, ViewPagerWithTabLayoutAdapter.IHost
    {
        protected override int? LayoutResource => Resource.Layout.activity_tablayout_viewpager2;



        public override void OnCreateView(View view)
        {
            base.OnCreateView(view);

            var adapter = new ViewPagerWithTabLayoutAdapter(this, this);
            binding!.pager.SetupWithTabLayout(binding!.tab_layout, adapter);
        }

        int ViewPagerAdapter.IHost.ItemCount => 3;

        Fragment ViewPagerAdapter.IHost.CreateFragment(int position) => position switch
        {
            0 => new ASFPlusConsoleFragment(),
            1 => new ASFPlusBotFragment(),
            2 => new ASFPlusConfigFragment(),
            _ => throw new ArgumentOutOfRangeException(nameof(position), position.ToString()),
        };

        string ViewPagerWithTabLayoutAdapter.IHost.GetPageTitle(int position) => position switch
        {
            0 => "ASF控制台",
            1 => "Bot配置",
            2 => "ASF配置",
            _ => throw new ArgumentOutOfRangeException(nameof(position), position.ToString()),
        };
    }
}
