using Android.OS;
using Android.Runtime;
using Android.Views;
using AndroidX.AppCompat.View.Menu;
using Binding;
using ReactiveUI;
using System.Application.Services;
using System.Application.UI.Adapters;
using System.Application.UI.ViewModels;
using static System.Application.UI.Resx.AppResources;
using Fragment = AndroidX.Fragment.App.Fragment;
using ASF_FS = System.Application.Services.Native.ArchiSteamFarmForegroundService;
using static System.Application.UI.ViewModels.ArchiSteamFarmPlusPageViewModel;
using System.Application.Services.Implementation;

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
                if (menuBuilder != null) // toolbar menu 因主活动改为 XF Shell，已由 XF 托管
                {
                    var menu_start = menuBuilder.FindItem(Resource.Id.menu_start);
                    if (menu_start != null)
                    {
                        menu_start.SetIcon(value ?
                            Resource.Drawable.round_pause_circle_outline_black_24 :
                            Resource.Drawable.round_play_circle_outline_black_24);
                    }
                }
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

        void StartOrStopASF_ForegroundService()
            => AndroidPlatformServiceImpl.StartOrStopForegroundService(
                RequireActivity(), nameof(ASFService));

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            var actionItem = MenuIdResToEnum(item.ItemId);
            if (actionItem.IsDefined())
            {
                switch (actionItem)
                {
                    case ActionItem.StartOrStop:
                        StartOrStopASF_ForegroundService();
                        break;
                    default:
                        ViewModel!.MenuItemClick(actionItem);
                        break;
                }
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
                return ActionItem.WebAddBot;
            }
            else if (resId == Resource.Id.menu_refresh)
            {
                return ActionItem.Refresh;
            }
            else if (resId == Resource.Id.menu_open_web_console)
            {
                return ActionItem.OpenWebConsole;
            }
            else if (resId == Resource.Id.menu_asf_wiki)
            {
                return ActionItem.Wiki;
            }
            else if (resId == Resource.Id.menu_asf_github)
            {
                return ActionItem.Repo;
            }
            else if (resId == Resource.Id.menu_asf_online_config_generator)
            {
                return ActionItem.ConfigGenerator;
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
            0 => ASF_Console,
            1 => ASF_BotManage,
            2 => ASF_GlobalConfig,
            _ => throw new ArgumentOutOfRangeException(nameof(position), position.ToString()),
        };
    }
}
