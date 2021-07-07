using Android.Content;
using Android.Runtime;
using Android.Views;
using AndroidX.RecyclerView.Widget;
using Binding;
using Com.Leinardi.Android.Speeddial;
using System.Application.Services;
using System.Application.UI.Activities;
using System.Application.UI.Adapters;
using System.Application.UI.ViewModels;
using System.Collections.Generic;
using System.Linq;
using static System.Application.UI.ViewModels.LocalAuthPageViewModel;

namespace System.Application.UI.Fragments
{
    [Register(JavaPackageConstants.Fragments + nameof(LocalAuthFragment))]
    internal sealed class LocalAuthFragment : BaseFragment<fragment_local_auth, LocalAuthPageViewModel>, SpeedDialView.IOnActionSelectedListener, SpeedDialView.IOnChangeListener
    {
        protected override int? LayoutResource => Resource.Layout.fragment_local_auth;

        IReadOnlyDictionary<int, ActionItem>? actionIds;
        IReadOnlyDictionary<ActionItem, FabWithLabelView>? speedDialDict;

        public override void OnCreateView(View view)
        {
            base.OnCreateView(view);

            var adapter = new GAPAuthenticatorAdapter(ViewModel!);
            var layout = new LinearLayoutManager(Context, LinearLayoutManager.Vertical, false);
            binding!.rvAuthenticators.SetLayoutManager(layout);
            binding.rvAuthenticators.SetAdapter(adapter);

            var actionItems = Enum2.GetAll<ActionItem>();
            actionIds = actionItems.ToDictionary(ToIdRes, x => x);
            speedDialDict = actionItems.ToDictionary(x => x, x => binding.speedDial.AddActionItem(new SpeedDialActionItem.Builder(ToIdRes(x), ToIconRes(x))
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
                var id = actionIds![actionItem.Id];
                if (id.IsDefined())
                {
                    binding.speedDial.Close();
                    Toast.Show(id.ToString());
                    switch (id)
                    {
                        case ActionItem.Add:
                            break;
                        case ActionItem.Encrypt:
                            break;
                        case ActionItem.Export:
                            break;
                        case ActionItem.Lock:
                            break;
                        case ActionItem.Refresh:
                            break;
                    }
                    return true;
                }
            }

            return false; // false will close it without animation
        }

        public static int ToIconRes(ActionItem action) => action switch
        {
            ActionItem.Add => Resource.Drawable.baseline_add_black_24,
            ActionItem.Encrypt => Resource.Drawable.baseline_enhanced_encryption_black_24,
            ActionItem.Export => Resource.Drawable.baseline_save_alt_black_24,
            ActionItem.Lock => Resource.Drawable.baseline_lock_open_black_24,
            ActionItem.Refresh => Resource.Drawable.baseline_refresh_black_24,
            _ => throw new ArgumentOutOfRangeException(nameof(action), action, null),
        };

        public static int ToIdRes(ActionItem action) => action switch
        {
            ActionItem.Add => Resource.Id.fab_action1,
            ActionItem.Encrypt => Resource.Id.fab_action2,
            ActionItem.Export => Resource.Id.fab_action3,
            ActionItem.Lock => Resource.Id.fab_action4,
            ActionItem.Refresh => Resource.Id.fab_action5,
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
            actionIds = null;
            speedDialDict = null;
            if (Activity is IOnBackPressedCallback callback)
            {
                callback.HandleOnBackPressed = null;
            }
            base.OnDestroyView();
        }
    }
}