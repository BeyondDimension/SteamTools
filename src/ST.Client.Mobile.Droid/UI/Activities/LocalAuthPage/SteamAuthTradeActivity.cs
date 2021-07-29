using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using AndroidX.AppCompat.View.Menu;
using AndroidX.RecyclerView.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using Binding;
using ReactiveUI;
using System.Application.Services;
using System.Application.UI.Adapters;
using System.Application.UI.Resx;
using System.Application.UI.ViewModels;
using System.Linq;
using System.Reactive.Linq;
using static System.Application.UI.Resx.AppResources;
using static System.Application.UI.ViewModels.AuthTradeWindowViewModel;

namespace System.Application.UI.Activities
{
    [Register(JavaPackageConstants.Activities + nameof(SteamAuthTradeActivity))]
    [Activity(Theme = ManifestConstants.MainTheme_NoActionBar,
        LaunchMode = LaunchMode.SingleTask,
        ConfigurationChanges = ManifestConstants.ConfigurationChanges)]
    internal sealed class SteamAuthTradeActivity : BaseActivity<activity_steam_auth_trade, AuthTradeWindowViewModel>/*, SpeedDialView.IOnActionSelectedListener, SpeedDialView.IOnChangeListener*/, SwipeRefreshLayout.IOnRefreshListener
    {
        protected override int? LayoutResource => Resource.Layout.activity_steam_auth_trade;

        //IReadOnlyDictionary<ActionItem, FabWithLabelView>? speedDialDict;

        protected override AuthTradeWindowViewModel? OnCreateViewModel()
        {
            var vm = AuthService.Current.Authenticators.Items.FirstOrDefault(x => x.Id == this.GetViewModel<ushort>());
            return new(vm);
        }

        void SetLogoutMenuVisible(bool visible)
        {
            var menu_logout = menuBuilder?.FindItem(Resource.Id.menu_logout);
            if (menu_logout != default) menu_logout.SetVisible(visible);
        }

        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (!ViewModel.HasValue())
            {
                Finish();
                return;
            }

            this.SetSupportActionBarWithNavigationClick(binding!.toolbar, true);

            R.Current.WhenAnyValue(x => x.Res).SubscribeInMainThread(_ =>
            {
                Title = DisplayName;
                if (binding == null) return;
                binding.layoutSteamUserName.Hint = Steam_User;
                binding.layoutSteamPassword.Hint = Steam_Password;
                binding.layoutCaptcha.Hint = Steam_ImageCodeTip;
                binding.tvConfirmConutMessage.Text = ViewModel!.ConfirmationsConutMessage;
                binding.cbStreamRememberMe.Text = User_Rememberme;
                binding.btnLogin.Text = Login;
                binding.tvSteamTradeLoginTip.Text = LocalAuth_SteamTradeLoginTip;
                //speedDialDict.ReplaceLabels(ToString2);
                binding.btnCancelTrade.Text = LocalAuth_AuthTrade_Cancel;
                binding.btnConfirmTrade.Text = LocalAuth_AuthTrade_Confirm;
                SetMenuTitle();
            }).AddTo(this);

            ViewModel!.WhenAnyValue(x => x.IsLoggedIn).SubscribeInMainThread(value =>
            {
                if (binding == null) return;
                var state = value ? ViewStates.Gone : ViewStates.Visible;
                var state_reverse = !value ? ViewStates.Gone : ViewStates.Visible;
                binding.layoutContentSteamLogin.Visibility = state;
                binding.layoutContentConfirmations.Visibility = state_reverse;
                binding.layoutActions.Visibility = state_reverse;
                //binding.speedDial.Visibility = state;
                SetLogoutMenuVisible(value);
            }).AddTo(this);
            ViewModel!.WhenAnyValue(x => x.IsRequiresCaptcha).SubscribeInMainThread(value =>
            {
                if (binding == null) return;
                var state = !value ? ViewStates.Gone : ViewStates.Visible;
                binding.ivCaptchaImage.Visibility = state;
                binding.layoutCaptcha.Visibility = state;
            }).AddTo(this);
            ViewModel!.WhenAnyValue(x => x.CodeImage).SubscribeInMainThread(value =>
            {
                if (binding == null) return;
                binding.ivCaptchaImage.SetImageSource(value);
            }).AddTo(this);
            ViewModel!.WhenAnyValue(x => x.ConfirmationsConutMessage).SubscribeInMainThread(value =>
            {
                if (binding == null) return;
                binding.tvConfirmConutMessage.Text = value;
            }).AddTo(this);
            ViewModel!.WhenAnyValue(x => x.IsLoading).SubscribeInMainThread(value =>
            {
                if (binding == null) return;
                var ismain = MainThread2.IsMainThread;
                var state = !value ? ViewStates.Gone : ViewStates.Visible;
                binding.layoutContentConfirmations.Visibility = (value || !ViewModel!.IsLoggedIn) ? ViewStates.Gone : ViewStates.Visible;
                binding.layoutContentSteamLogin.Visibility = (value || ViewModel!.IsLoggedIn) ? ViewStates.Gone : ViewStates.Visible;
                binding.layoutLoading.Visibility = state;

                if (!value && binding.swipeRefreshLayout.Refreshing)
                {
                    binding.swipeRefreshLayout.Refreshing = false;
                }
            }).AddTo(this);
            ViewModel!.WhenAnyValue(x => x.UnselectAll).SubscribeInMainThread(value =>
            {
                if (binding == null) return;
                binding.cbSelectAll.Checked = !value;
            }).AddTo(this);
            ViewModel!.WhenAnyValue(x => x.SelectAllText).SubscribeInMainThread(value =>
            {
                if (binding == null) return;
                binding.cbSelectAll.Text = value;
            }).AddTo(this);
            ViewModel!.WhenAnyValue(x => x.LoadingText).SubscribeInMainThread(value =>
            {
                if (binding == null) return;
                binding.tvLoading.Text = value;
            }).AddTo(this);

            binding!.tbSteamUserName.TextChanged += (_, _) =>
            {
                ViewModel!.UserName = binding!.tbSteamUserName.Text;
            };
            binding.tbSteamPassword.TextChanged += (_, _) =>
            {
                ViewModel!.Password = binding!.tbSteamPassword.Text;
            };
            binding.cbStreamRememberMe.CheckedChange += (_, e) =>
            {
                ViewModel!.RememberMe = e.IsChecked;
            };
            binding.tbCaptcha.TextChanged += (_, _) =>
            {
                ViewModel!.CodeImageChar = binding.tbCaptcha.Text;
            };

            var adapter = new SteamAuthTradeConfirmationAdapter(ViewModel!);
            var layout = new LinearLayoutManager(this, LinearLayoutManager.Vertical, false);
            binding!.rvConfirmations.SetLayoutManager(layout);
            binding.rvConfirmations.AddItemDecoration(VerticalItemViewDecoration2.Get(this, Resource.Dimension.activity_vertical_margin, Resource.Dimension.fab_full_height, noTop: true));
            binding.rvConfirmations.SetAdapter(adapter);

            //var actionItems = Enum2.GetAll<ActionItem>().Where(x => x != ActionItem.Refresh);
            //speedDialDict = actionItems.ToDictionary(x => x, x => binding.speedDial.AddActionItem(new SpeedDialActionItem.Builder((int)x, ToIconRes(x))
            //        .SetLabel(ToString2(x))
            //        .SetFabBackgroundColor(this.GetColorCompat(Resource.Color.white))
            //        .SetFabImageTintColor(new(this.GetColorCompat(Resource.Color.fab_icon_background_min)))
            //        .Create()));

            //binding.speedDial.SetOnActionSelectedListener(this);
            //binding.speedDial.SetOnChangeListener(this);

            binding.swipeRefreshLayout.InitDefaultStyles();
            binding.swipeRefreshLayout.SetOnRefreshListener(this);

            SetOnClickListener(binding.btnLogin, binding.btnCancelTrade, binding.btnConfirmTrade);
        }

        MenuBuilder? menuBuilder;
        public override bool OnCreateOptionsMenu(IMenu? menu)
        {
            MenuInflater.Inflate(Resource.Menu.auth_trade_toolbar_menu, menu);
            menuBuilder = menu.SetOptionalIconsVisible();
            if (menuBuilder != null)
            {
                SetMenuTitle();
                SetLogoutMenuVisible(ViewModel!.IsLoggedIn);
            }
            return true;
        }

        void SetMenuTitle() => menuBuilder.SetMenuTitle(ToString2, MenuIdResToEnum);

        protected override void OnClick(View view)
        {
            base.OnClick(view);
            if (view.Id == Resource.Id.btnLogin)
            {
                ViewModel!.LoginButton_Click();
            }
            else if (view.Id == Resource.Id.btnCancelTrade)
            {
                ViewModel!.MenuItemClick(ActionItem.CancelAll);
            }
            else if (view.Id == Resource.Id.btnConfirmTrade)
            {
                ViewModel!.MenuItemClick(ActionItem.ConfirmAll);
            }
        }

        //bool SpeedDialView.IOnActionSelectedListener.OnActionSelected(SpeedDialActionItem actionItem)
        //{
        //    if (binding != null && actionItem != null)
        //    {
        //        var id = (ActionItem)actionItem.Id;
        //        if (id.IsDefined())
        //        {
        //            binding.speedDial.Close();
        //            ViewModel!.MenuItemClick(id);
        //            return true;
        //        }
        //    }

        //    return false; // false will close it without animation
        //}

        //bool SpeedDialView.IOnChangeListener.OnMainActionSelected() => false;

        //void SpeedDialView.IOnChangeListener.OnToggleChanged(bool isOpen)
        //{
        //    if (isOpen)
        //    {
        //        binding!.clearFocus.RequestFocus();
        //        //var firstFab = speedDialDict?.FirstOrDefault().Value?.Fab;
        //        //if (firstFab?.IsFocused ?? false)
        //        //{
        //        //    // 第一个元素默认有焦点，去掉焦点
        //        //    firstFab.ClearFocus();
        //        //}
        //    }
        //    HandleOnBackPressed = isOpen ? HandleOnBackPressed : null;
        //}

        static int ToIconRes(ActionItem action) => action switch
        {
            ActionItem.ConfirmAll => Resource.Drawable.baseline_check_black_24,
            ActionItem.CancelAll => Resource.Drawable.baseline_close_black_24,
            ActionItem.Refresh => Resource.Drawable.baseline_refresh_black_24,
            ActionItem.Logout => Resource.Drawable.baseline_logout_black_24,
            _ => throw new ArgumentOutOfRangeException(nameof(action), action, null),
        };

        public static void StartActivity(Activity activity, ushort authId)
        {
            GoToPlatformPages.StartActivity<SteamAuthTradeActivity, ushort>(activity, authId);
        }

        static ActionItem MenuIdResToEnum(int resId)
        {
            //if (resId == Resource.Id.menu_confirm_all)
            //{
            //    return ActionItem.ConfirmAll;
            //}
            //else if (resId == Resource.Id.menu_cancel_all)
            //{
            //    return ActionItem.CancelAll;
            //}
            //else
            if (resId == Resource.Id.menu_logout)
            {
                return ActionItem.Logout;
            }
            return default;
        }

        protected override void OnDestroy()
        {
            //speedDialDict = null;
            base.OnDestroy();
        }

        void SwipeRefreshLayout.IOnRefreshListener.OnRefresh()
        {
            ViewModel!.MenuItemClick(ActionItem.Refresh);
        }
    }
}