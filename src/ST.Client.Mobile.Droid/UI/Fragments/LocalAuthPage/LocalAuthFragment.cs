using Android.Content;
using Android.Runtime;
using Android.Views;
using AndroidX.RecyclerView.Widget;
using Binding;
using Com.Leinardi.Android.Speeddial;
using ReactiveUI;
using System.Application.Services;
using System.Application.UI.Activities;
using System.Application.UI.Adapters;
using System.Application.UI.Resx;
using System.Application.UI.ViewModels;
using System.Collections.Generic;
using System.Linq;
using static System.Application.UI.Resx.AppResources;
using static System.Application.UI.ViewModels.LocalAuthPageViewModel;

namespace System.Application.UI.Fragments
{
    [Register(JavaPackageConstants.Fragments + nameof(LocalAuthFragment))]
    internal sealed class LocalAuthFragment : BaseFragment<fragment_local_auth, LocalAuthPageViewModel>, SpeedDialView.IOnActionSelectedListener, SpeedDialView.IOnChangeListener
    {
        protected override int? LayoutResource => Resource.Layout.fragment_local_auth;

        IReadOnlyDictionary<ActionItem, FabWithLabelView>? speedDialDict;

        public override void OnCreateView(View view)
        {
            base.OnCreateView(view);

            R.Current.WhenAnyValue(x => x.Res).Subscribe(_ =>
            {
                if (binding == null) return;
                binding.tvEmptyTip.Text = LocalAuth_NoAuthTip_.Format(BottomRightCorner);
            }).AddTo(this);

            ViewModel!.WhenAnyValue(x => x.IsAuthenticatorsEmpty).Subscribe(value =>
            {
                if (binding == null) return;
                binding.tvEmptyTip.Visibility = value ? ViewStates.Visible : ViewStates.Gone;
            }).AddTo(this);

            var adapter = new GAPAuthenticatorAdapter(ViewModel!);
            var layout = new LinearLayoutManager(Context, LinearLayoutManager.Vertical, false);
            binding!.rvAuthenticators.SetLayoutManager(layout);
            binding.rvAuthenticators.SetAdapter(adapter);

            var actionItems = Enum2.GetAll<ActionItem>();
            speedDialDict = actionItems.ToDictionary(x => x, x => binding.speedDial.AddActionItem(new SpeedDialActionItem.Builder((int)x, ToIconRes(x))
                    .SetLabel(ToString2(x))
                    .SetFabBackgroundColor(Context.GetColorCompat(Resource.Color.white))
                    .SetFabImageTintColor(new(Context.GetColorCompat(Resource.Color.fab_icon_background_min)))
                    .Create()));

            binding.speedDial.SetOnActionSelectedListener(this);
            binding.speedDial.SetOnChangeListener(this);
        }

        public override void OnStop()
        {
            base.OnStop();
            AuthService.Current.SaveEditNameAuthenticators();
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
                        case ActionItem.Add:
                            ViewModel!.AddAuthCommand.Invoke();
                            break;
                        case ActionItem.Encrypt:
                            ViewModel!.EncryptionAuthCommand.Invoke();
                            break;
                        case ActionItem.Export:
                            ViewModel!.ExportAuthCommand.Invoke();
                            break;
                        case ActionItem.Lock:
                            ViewModel!.LockCommand.Invoke();
                            break;
                        case ActionItem.Refresh:
                            ViewModel!.RefreshAuthCommand.Invoke();
                            break;
                    }
                    return true;
                }
            }

            return false; // false will close it without animation
        }

        static int ToIconRes(ActionItem action) => action switch
        {
            ActionItem.Add => Resource.Drawable.baseline_add_black_24,
            ActionItem.Encrypt => Resource.Drawable.baseline_enhanced_encryption_black_24,
            ActionItem.Export => Resource.Drawable.baseline_save_alt_black_24,
            ActionItem.Lock => Resource.Drawable.baseline_lock_open_black_24,
            ActionItem.Refresh => Resource.Drawable.baseline_refresh_black_24,
            _ => throw new ArgumentOutOfRangeException(nameof(action), action, null),
        };

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
            if (Activity is IOnBackPressedCallback callback)
            {
                callback.HandleOnBackPressed = isOpen ? HandleOnBackPressed : null;
            }
        }

        bool HandleOnBackPressed()
        {
            if (binding?.speedDial.IsOpen ?? false)
            {
                binding.speedDial.Close();
                return true;
            }
            return false;
        }

        public override void OnDestroyView()
        {
            speedDialDict = null;
            if (Activity is IOnBackPressedCallback callback)
            {
                callback.HandleOnBackPressed = null;
            }
            base.OnDestroyView();
        }
    }
}