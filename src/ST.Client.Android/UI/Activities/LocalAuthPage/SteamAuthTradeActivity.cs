using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using AndroidX.AppCompat.View.Menu;
using AndroidX.RecyclerView.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using Binding;
using DynamicData;
using ReactiveUI;
using System.Application.Models;
using System.Application.Services;
using System.Application.UI.Adapters;
using System.Application.UI.Resx;
using System.Application.UI.ViewModels;
using System.Linq;
using System.Reactive.Linq;
using WinAuth;
using static System.Application.UI.Resx.AppResources;
using static System.Application.UI.ViewModels.AuthTradeWindowViewModel;

namespace System.Application.UI.Activities
{
    [Register(JavaPackageConstants.Activities + nameof(SteamAuthTradeActivity))]
    [Activity(Theme = ManifestConstants.MainTheme2_NoActionBar,
        LaunchMode = LaunchMode.SingleTask,
        ConfigurationChanges = ManifestConstants.ConfigurationChanges)]
    internal sealed class SteamAuthTradeActivity : BaseSteamAuthTradeActivity
    {

    }

#if DEBUG
    [Register(JavaPackageConstants.Activities + nameof(MockSteamAuthTradeActivity))]
    [Activity(Theme = ManifestConstants.MainTheme2_NoActionBar,
        LaunchMode = LaunchMode.SingleTask,
        //MainLauncher = true,
        Label = "AuthTrade(Mock)",
        ConfigurationChanges = ManifestConstants.ConfigurationChanges)]
    internal sealed class MockSteamAuthTradeActivity : BaseSteamAuthTradeActivity
    {
        protected override AuthTradeWindowViewModel? OnCreateViewModel()
        {
            GAPAuthenticatorValueDTO.SteamAuthenticator steam_auth = new()
            {

            };
            var client = steam_auth.GetClient();
            client.Session = new WinAuthSteamClient.SteamSession()
            {
                OAuthToken = "1234",
            };
            GAPAuthenticatorDTO gap_auth = new()
            {
                Value = steam_auth,
            };
            MyAuthenticator auth = new(gap_auth)
            {

            };
            AuthTradeWindowViewModel vm = new(auth)
            {

            };
            vm.ConfirmationsSourceList.AddRange(new WinAuthSteamClient.Confirmation[] {
                new()
                {
                    Details = "出售 - XX Y 号武器箱 （ABCDEFG）",
                    Traded = "￥x.xx（￥y.yy）",
                    When = "z 小时以前",
                    Image = "https://img-prod-cms-rt-microsoft-com.akamaized.net/cms/api/am/imageFileData/RE4wHfJ",
                },
                new()
                {
                    Details = "出售 - XX Y 号武器箱 （ABCDEFG）",
                    Traded = "￥x.xx（￥y.yy）",
                    When = "z 小时以前",
                    Image = "https://cn.bing.com/th?id=OHR.AmericanRobin_ZH-CN0667508209_1920x1080.jpg",
                },
                new()
                {
                    Details = "出售 - XX Y 号武器箱 （ABCDEFG）",
                    Traded = "￥x.xx（￥y.yy）",
                    When = "z 小时以前",
                    Image = "https://cn.bing.com/th?id=OHR.ElPanecilloHill_ZH-CN0527709139_1920x1080.jpg",
                },
                new()
                {
                    Details = "出售 - XX Y 号武器箱 （ABCDEFG）",
                    Traded = "￥x.xx（￥y.yy）",
                    When = "z 小时以前",
                    Image = "https://cn.bing.com/th?id=OHR.WickerCultivation_ZH-CN0310713697_1920x1080.jpg",
                },
                new()
                {
                    Details = "出售 - XX Y 号武器箱 （ABCDEFG）",
                    Traded = "￥x.xx（￥y.yy）",
                    When = "z 小时以前",
                    Image = "https://cn.bing.com/th?id=OHR.ShadowEverest_ZH-CN9951649290_1920x1080.jpg",
                },
                new()
                {
                    Details = "出售 - XX Y 号武器箱 （ABCDEFG）",
                    Traded = "￥x.xx（￥y.yy）",
                    When = "z 小时以前",
                    Image = "https://cn.bing.com/th?id=OHR.FoxDovrefjell_ZH-CN9554491452_1920x1080.jpg",
                },
            });
            return vm;
        }
    }
#endif

    internal abstract class BaseSteamAuthTradeActivity : BaseActivity<activity_steam_auth_trade, AuthTradeWindowViewModel>/*, SpeedDialView.IOnActionSelectedListener, SpeedDialView.IOnChangeListener*/, SwipeRefreshLayout.IOnRefreshListener
    {
        protected override int? LayoutResource => Resource.Layout.activity_steam_auth_trade;

        //IReadOnlyDictionary<ActionItem, FabWithLabelView>? speedDialDict;

        protected override AuthTradeWindowViewModel? OnCreateViewModel()
        {
            var vm = AuthService.Current.Authenticators.Items.FirstOrDefault(x => x.Id == this.GetViewModel<ushort>());
            return new(vm);
        }

        void SetIsLoggedInMenuVisible(bool visible)
        {
            if (menuBuilder == null) return;
            var menus = new[] { Resource.Id.menu_logout, Resource.Id.menu_refresh, };
            foreach (var menuId in menus)
            {
                var menu = menuBuilder.FindItem(menuId);
                if (menu != default) menu.SetVisible(visible);
            }
        }

        protected override void OnCreate2(Bundle? savedInstanceState)
        {
            base.OnCreate2(savedInstanceState);

            if (!ViewModel.HasValue())
            {
                Finish();
                return;
            }

            this.SetSupportActionBarWithNavigationClick(binding!.toolbar, true);

            R.Subscribe(() =>
            {
                Title = ViewModel!.Title;
                if (binding == null) return;
                binding.layoutSteamUserName.Hint = Steam_User;
                binding.layoutSteamPassword.Hint = Steam_Password;
                binding.layoutCaptcha.Hint = Steam_ImageCodeTip;
                if (!ViewModel!.IsConfirmationsAny)
                    binding.tvConfirmConutMessage.Text = ViewModel!.ConfirmationsConutMessage;
                binding.cbStreamRememberMe.Text = User_Rememberme;
                binding.btnLogin.Text = Login;
                binding.tvSteamTradeLoginTip.Text = LocalAuth_SteamTradeLoginTip;
                //speedDialDict.ReplaceLabels(ToString2);
                binding.btnCancelTrade.Text = LocalAuth_AuthTrade_Cancel;
                binding.btnConfirmTrade.Text = LocalAuth_AuthTrade_Confirm;
                binding.btnShowCaptchaImage.Text = Steam_ImageCodeShowError;
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
                SetIsLoggedInMenuVisible(value);
            }).AddTo(this);
            ViewModel!.WhenAnyValue(x => x.IsRequiresCaptcha).SubscribeInMainThread(value =>
            {
                if (binding == null) return;
                var state = !value ? ViewStates.Gone : ViewStates.Visible;
                binding.ivCaptchaImage.Visibility = state;
                binding.layoutCaptcha.Visibility = state;
            }).AddTo(this);
            ViewModel!.WhenAnyValue(x => x.CaptchaImage).SubscribeInMainThread(value =>
            {
                if (binding == null) return;
                binding.ivCaptchaImage.SetImageSource(value, Resource.Dimension.img_size_captcha_w, Resource.Dimension.img_size_captcha_h);
            }).AddTo(this);
            ViewModel!.WhenAnyValue(x => x.ConfirmationsConutMessage).SubscribeInMainThread(value =>
            {
                if (binding == null) return;
                if (!ViewModel!.IsConfirmationsAny)
                {
                    binding.tvConfirmConutMessage.Text = value;
                    binding.tvConfirmConutMessage.Visibility = ViewStates.Visible;
                    binding.layoutActions.Visibility = ViewStates.Gone;
                }
                else
                {
                    binding.tvConfirmConutMessage.Visibility = ViewStates.Gone;
                    binding.layoutActions.Visibility = ViewStates.Visible;
                    Toast.Show(value, ToastLength.Short);
                }
            }).AddTo(this);
            ViewModel!.WhenAnyValue(x => x.IsLoading).SubscribeInMainThread(value =>
            {
                if (binding == null) return;
                var state = !value ? ViewStates.Gone : ViewStates.Visible;
                binding.layoutContentConfirmations.Visibility = (value || !ViewModel!.IsLoggedIn) ? ViewStates.Gone : ViewStates.Visible;
                binding.layoutContentSteamLogin.Visibility = (value || ViewModel!.IsLoggedIn) ? ViewStates.Gone : ViewStates.Visible;
                binding.layoutLoading.Visibility = state;
                //binding.swipeRefreshLayout.Refreshing = value;
                binding.layoutActions.Visibility = (value || !ViewModel!.IsLoggedIn || !ViewModel!.IsConfirmationsAny) ? ViewStates.Gone : ViewStates.Visible;
            }).AddTo(this);
            ViewModel!.WhenAnyValue(x => x.UnselectAll).SubscribeInMainThread(value =>
            {
                if (binding == null) return;
                binding.cbSelectAll.Checked = !value;
            }).AddTo(this);
            ViewModel!.WhenAnyValue(x => x.SelectAllText).SubscribeInMainThread(value =>
            {
                if (binding == null) return;
                if (value == null)
                {
                    if (binding.layoutActions.Visibility != ViewStates.Gone)
                    {
                        binding.layoutActions.Visibility = ViewStates.Gone;
                    }
                }
                else
                {
                    if (binding.layoutActions.Visibility != ViewStates.Visible)
                    {
                        binding.layoutActions.Visibility = ViewStates.Visible;
                    }
                    binding.cbSelectAll.Text = value;
                }
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
                ViewModel!.CaptchaText = binding.tbCaptcha.Text;
            };
            binding.cbSelectAll.CheckedChange += (_, e) =>
            {
                ViewModel!.UnselectAll = !e.IsChecked;
            };

            var adapter = new SteamAuthTradeConfirmationAdapter(ViewModel!);
            adapter.ItemClick += (_, e) =>
            {
                if (e.Current.IsOperate != 0) return;
                e.Current.NotChecked = !e.Current.NotChecked;
            };
            binding!.rvConfirmations.SetLinearLayoutManager();
            binding.rvConfirmations.AddVerticalItemDecorationRes(Resource.Dimension.activity_vertical_margin, Resource.Dimension.tab_height, noTop: true);
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

            SetOnClickListener(binding.btnLogin,
                binding.btnCancelTrade,
                binding.btnConfirmTrade,
                binding.ivCaptchaImage,
                binding.btnShowCaptchaImage);
        }

        MenuBuilder? menuBuilder;
        public override bool OnCreateOptionsMenu(IMenu? menu)
        {
            MenuInflater.Inflate(Resource.Menu.auth_trade_toolbar_menu, menu);
            menuBuilder = menu.SetOptionalIconsVisible();
            if (menuBuilder != null)
            {
                SetMenuTitle();
                SetIsLoggedInMenuVisible(ViewModel!.IsLoggedIn);
            }
            return true;
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
            else if (view.Id == Resource.Id.ivCaptchaImage)
            {
                Browser2.Open(ViewModel?.CaptchaImage);
            }
            else if (view.Id == Resource.Id.btnShowCaptchaImage)
            {
                ViewModel!.CaptchaUrlButton_Click();
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
            else if (resId == Resource.Id.menu_refresh)
            {
                return ActionItem.Refresh;
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
            binding!.swipeRefreshLayout.Refreshing = false;
            ViewModel!.MenuItemClick(ActionItem.Refresh);
        }
    }
}