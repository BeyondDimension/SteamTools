using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using AndroidX.RecyclerView.Widget;
using Binding;
using Com.Leinardi.Android.Speeddial;
using ReactiveUI;
using System.Application.UI.Adapters;
using System.Application.UI.Resx;
using System.Application.UI.ViewModels;
using System.Collections.Generic;
using System.Linq;
using static System.Application.UI.ViewModels.AuthTradeWindowViewModel;

namespace System.Application.UI.Activities
{
    [Register(JavaPackageConstants.Activities + nameof(SteamAuthTradeActivity))]
    [Activity(Theme = ManifestConstants.MainTheme_NoActionBar,
        LaunchMode = LaunchMode.SingleTask,
        ConfigurationChanges = ManifestConstants.ConfigurationChanges)]
    internal sealed class SteamAuthTradeActivity : BaseActivity<activity_steam_auth_trade, AuthTradeWindowViewModel>, SpeedDialView.IOnActionSelectedListener
    {
        protected override int? LayoutResource => Resource.Layout.activity_steam_auth_trade;

        IReadOnlyDictionary<ActionItem, FabWithLabelView>? speedDialDict;

        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            var vm = MainActivity.Instance?.LocalAuthPageViewModel.Authenticators.FirstOrDefault(x => x.Id == this.GetViewModel<ushort>());
            if (vm == null)
            {
                Finish();
                return;
            }

            ViewModel = new(vm);
            ViewModel.AddTo(this);

            R.Current.WhenAnyValue(x => x.Res).Subscribe(_ =>
            {
                if (binding == null) return;
                binding.tvConfirmConutMessage.Text = ViewModel.ConfirmationsConutMessage;
            }).AddTo(this);

            var adapter = new SteamAuthTradeConfirmationAdapter(ViewModel);
            var layout = new LinearLayoutManager(this, LinearLayoutManager.Vertical, false);
            binding!.rvConfirmations.SetLayoutManager(layout);
            binding.rvConfirmations.AddItemDecoration(new VerticalItemViewDecoration(Resources!.GetDimensionPixelSize(Resource.Dimension.activity_vertical_margin)));
            binding.rvConfirmations.SetAdapter(adapter);

            var actionItems = Enum2.GetAll<ActionItem>();
            speedDialDict = actionItems.ToDictionary(x => x, x => binding.speedDial.AddActionItem(new SpeedDialActionItem.Builder((int)x, ToIconRes(x))
                    .SetLabel(ToString2(x))
                    .SetFabBackgroundColor(this.GetColorCompat(Resource.Color.white))
                    .SetFabImageTintColor(new(this.GetColorCompat(Resource.Color.fab_icon_background_min)))
                    .Create()));

            binding.speedDial.SetOnActionSelectedListener(this);
            //binding.speedDial.SetOnChangeListener(this);
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

        static int ToIconRes(ActionItem action) => action switch
        {
            ActionItem.ConfirmAll => Resource.Drawable.baseline_add_black_24,
            ActionItem.CancelAll => Resource.Drawable.baseline_enhanced_encryption_black_24,
            ActionItem.Refresh => Resource.Drawable.baseline_refresh_black_24,
            ActionItem.Logout => Resource.Drawable.baseline_save_alt_black_24,
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