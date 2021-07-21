using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using AndroidX.RecyclerView.Widget;
using Binding;
using Com.Leinardi.Android.Speeddial;
using ReactiveUI;
using System.Application.UI.Adapters;
using System.Application.UI.Resx;
using System.Application.UI.ViewModels;
using System.Collections.Generic;
using System.Linq;
using static System.Application.UI.Resx.AppResources;
using static System.Application.UI.ViewModels.AuthTradeWindowViewModel;

namespace System.Application.UI.Activities
{
    [Register(JavaPackageConstants.Activities + nameof(SteamAuthTradeActivity))]
    [Activity(Theme = ManifestConstants.MainTheme_NoActionBar,
        LaunchMode = LaunchMode.SingleTask,
        ConfigurationChanges = ManifestConstants.ConfigurationChanges)]
    internal sealed class SteamAuthTradeActivity : BaseActivity<activity_steam_auth_trade, AuthTradeWindowViewModel>, SpeedDialView.IOnActionSelectedListener, SpeedDialView.IOnChangeListener
    {
        protected override int? LayoutResource => Resource.Layout.activity_steam_auth_trade;

        IReadOnlyDictionary<ActionItem, FabWithLabelView>? speedDialDict;

        protected override AuthTradeWindowViewModel? OnCreateViewModel()
        {
            var vm = LocalAuthPageViewModel.Current.Authenticators?.FirstOrDefault(x => x.Id == this.GetViewModel<ushort>());
            return new(vm);
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

            R.Current.WhenAnyValue(x => x.Res).Subscribe(_ =>
            {
                Title = DisplayName;
                if (binding == null) return;
                binding.tvConfirmConutMessage.Text = ViewModel!.ConfirmationsConutMessage;
                binding.cbStreamRememberMe.Text = User_Rememberme;
                binding.tbCaptcha.Text = Steam_ImageCodeTip;
                binding.btnLogin.Text = Login;
                binding.tvSteamTradeLoginTip.Text = LocalAuth_SteamTradeLoginTip;
                binding.tvLoading.Text = LocalAuth_AuthTrade_GetTip;
                speedDialDict.ReplaceLabels(ToString2);
            }).AddTo(this);

            ViewModel!.WhenAnyValue(x => x.IsLoggedIn).Subscribe(value =>
            {
                if (binding == null) return;
                var state = value ? ViewStates.Gone : ViewStates.Visible;
                var state_reverse = !value ? ViewStates.Gone : ViewStates.Visible;
                binding.layoutContentSteamLogin.Visibility = state;
                binding.layoutContentConfirmations.Visibility = state_reverse;
                binding.speedDial.Visibility = state_reverse;
            }).AddTo(this);
            ViewModel!.WhenAnyValue(x => x.IsRequiresCaptcha).Subscribe(value =>
            {
                if (binding == null) return;
                var state = !value ? ViewStates.Gone : ViewStates.Visible;
                binding.ivCaptchaImage.Visibility = state;
                binding.layoutCaptcha.Visibility = state;
            }).AddTo(this);
            ViewModel!.WhenAnyValue(x => x.CodeImage).Subscribe(value =>
            {
                if (binding == null) return;
                binding.ivCaptchaImage.SetImageSource(value);
            }).AddTo(this);
            ViewModel!.WhenAnyValue(x => x.ConfirmationsConutMessage).Subscribe(value =>
            {
                if (binding == null) return;
                binding.tvConfirmConutMessage.Text = value;
            }).AddTo(this);
            ViewModel!.WhenAnyValue(x => x.IsLoading).Subscribe(value =>
            {
                if (binding == null) return;
                var state = !value ? ViewStates.Gone : ViewStates.Visible;
                binding.layoutContentConfirmations.Visibility = value || !ViewModel!.IsLoggedIn ? ViewStates.Gone : ViewStates.Visible;
                binding.layoutContentSteamLogin.Visibility = value || ViewModel!.IsLoggedIn ? ViewStates.Gone : ViewStates.Visible;
                binding.layoutLoading.Visibility = state;
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

            var actionItems = Enum2.GetAll<ActionItem>();
            speedDialDict = actionItems.ToDictionary(x => x, x => binding.speedDial.AddActionItem(new SpeedDialActionItem.Builder((int)x, ToIconRes(x))
                    .SetLabel(ToString2(x))
                    .SetFabBackgroundColor(this.GetColorCompat(Resource.Color.white))
                    .SetFabImageTintColor(new(this.GetColorCompat(Resource.Color.fab_icon_background_min)))
                    .Create()));

            binding.speedDial.SetOnActionSelectedListener(this);
            binding.speedDial.SetOnChangeListener(this);

            SetOnClickListener(binding.btnLogin);
        }

        protected override void OnClick(View view)
        {
            base.OnClick(view);
            if (view.Id == Resource.Id.btnLogin)
            {
                ViewModel!.LoginButton_Click();
            }
        }

        bool SpeedDialView.IOnActionSelectedListener.OnActionSelected(SpeedDialActionItem actionItem)
        {
            if (binding != null && actionItem != null)
            {
                var id = (ActionItem)actionItem.Id;
                if (id.IsDefined())
                {
                    binding.speedDial.Close();
                    switch (id)
                    {
                        case ActionItem.ConfirmAll:
                            ViewModel!.ConfirmAllButton_Click();
                            break;
                        case ActionItem.CancelAll:
                            ViewModel!.CancelAllButton_Click();
                            break;
                        case ActionItem.Refresh:
                            ViewModel!.Refresh_Click();
                            break;
                        case ActionItem.Logout:
                            ViewModel!.Logout_Click();
                            break;
                    }
                    return true;
                }
            }

            return false; // false will close it without animation
        }

        bool SpeedDialView.IOnChangeListener.OnMainActionSelected() => false;

        void SpeedDialView.IOnChangeListener.OnToggleChanged(bool isOpen)
        {
            if (isOpen)
            {
                binding!.clearFocus.RequestFocus();
                //var firstFab = speedDialDict?.FirstOrDefault().Value?.Fab;
                //if (firstFab?.IsFocused ?? false)
                //{
                //    // 第一个元素默认有焦点，去掉焦点
                //    firstFab.ClearFocus();
                //}
            }
            HandleOnBackPressed = isOpen ? HandleOnBackPressed : null;
        }

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

        protected override void OnDestroy()
        {
            speedDialDict = null;
            base.OnDestroy();
        }
    }
}