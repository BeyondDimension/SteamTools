using Android.OS;
using Android.Runtime;
using Android.Views;
using AndroidX.AppCompat.View.Menu;
using AndroidX.Fragment.App;
using Binding;
using ReactiveUI;
using System.Application.Services;
using System.Application.UI.Adapters;
using System.Application.UI.Resx;
using System.Application.UI.ViewModels;
using static System.Application.UI.Resx.AppResources;
using Fragment = AndroidX.Fragment.App.Fragment;
using static System.Application.UI.ViewModels.ArchiSteamFarmPlusPageViewModel;
using System.Text;

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

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            HasOptionsMenu = true;
        }

        public override void OnCreateView(View view)
        {
            base.OnCreateView(view);

            var adapter = new ViewPagerWithTabLayoutAdapter(this, this);
            binding!.pager.SetupWithTabLayout(binding!.tab_layout, adapter);

            ASFService.Current.WhenAnyValue(x => x.IsASFRuning).SubscribeInMainThread(value =>
            {
                if (menuBuilder == null) return;
                var menu_start = menuBuilder.FindItem(Resource.Id.menu_start);
                if (menu_start == null) return;
                menu_start.SetIcon(value ?
                    Resource.Drawable.round_pause_circle_outline_black_24 :
                    Resource.Drawable.round_play_circle_outline_black_24);
            }).AddTo(this);
        }

        MenuBuilder? menuBuilder;
        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            inflater.Inflate(Resource.Menu.asf_plus_toolbar_menu, menu);
            menuBuilder = menu.SetOptionalIconsVisible();
            if (menuBuilder != null)
            {
                SetMenuTitle();
            }
        }

        void SetMenuTitle() => menuBuilder.SetMenuTitle(ToString2, MenuIdResToEnum);

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            var actionItem = MenuIdResToEnum(item.ItemId);
            if (actionItem.IsDefined())
            {
                ViewModel!.MenuItemClick(actionItem);
                return true;
            }
            return base.OnOptionsItemSelected(item);
        }

        static ActionItem MenuIdResToEnum(int resId)
        {
            if (resId == Resource.Id.menu_start)
            {
                return ActionItem.StartOrStop;
            }
            else if (resId == Resource.Id.menu_add)
            {
                return ActionItem.AddBot;
            }
            else if (resId == Resource.Id.menu_refresh)
            {
                return ActionItem.Refresh;
            }
            else if (resId == Resource.Id.menu_open_web_console)
            {
                return ActionItem.OpenWebConsole;
            }
            return default;
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
