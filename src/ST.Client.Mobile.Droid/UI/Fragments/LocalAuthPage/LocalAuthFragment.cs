using Android.OS;
using Android.Runtime;
using Android.Views;
using AndroidX.AppCompat.View.Menu;
using AndroidX.RecyclerView.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using Binding;
using ReactiveUI;
using System.Application.UI.Activities;
using System.Application.UI.Adapters;
using System.Application.UI.Resx;
using System.Application.UI.ViewModels;
using static System.Application.UI.Resx.AppResources;
using static System.Application.UI.ViewModels.LocalAuthPageViewModel;

namespace System.Application.UI.Fragments
{
    [Register(JavaPackageConstants.Fragments + nameof(LocalAuthFragment))]
    internal sealed class LocalAuthFragment : BaseFragment<fragment_local_auth, LocalAuthPageViewModel>/*, SpeedDialView.IOnActionSelectedListener, SpeedDialView.IOnChangeListener*/, SwipeRefreshLayout.IOnRefreshListener
    {
        protected override int? LayoutResource => Resource.Layout.fragment_local_auth;

        //IReadOnlyDictionary<ActionItem, FabWithLabelView>? speedDialDict;

        protected override LocalAuthPageViewModel? OnCreateViewModel() => Current;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            HasOptionsMenu = true;
        }

        MenuBuilder? menuBuilder;
        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            inflater.Inflate(Resource.Menu.local_auth_toolbar_menu, menu);
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

        public override void OnCreateView(View view)
        {
            base.OnCreateView(view);
            R.Current.WhenAnyValue(x => x.Res).SubscribeInMainThread(_ =>
            {
                if (binding == null) return;
                binding.tvEmptyTip.Text = LocalAuth_NoAuthTip_.Format(TopRightCorner/*BottomRightCorner*/);
                binding.tvLoading.Text = LocalAuth_Loading;
                //speedDialDict.ReplaceLabels(ToString2);
                SetMenuTitle();
            }).AddTo(this);

            void OnAuthenticatorsChanged(bool isAuthenticatorsEmpty, bool isFirstActivation)
            {
                if (binding == null) return;
                ViewStates state, state_reverse, /*fab_state,*/ loading_state;
                if (isFirstActivation)
                {
                    state = ViewStates.Gone;
                    state_reverse = ViewStates.Gone;
                    //fab_state = ViewStates.Gone;
                    loading_state = ViewStates.Visible;
                }
                else
                {
                    state = isAuthenticatorsEmpty ? ViewStates.Visible : ViewStates.Gone;
                    state_reverse = !isAuthenticatorsEmpty ? ViewStates.Visible : ViewStates.Gone;
                    //fab_state = ViewStates.Visible;
                    loading_state = ViewStates.Gone;
                }
                binding.tvEmptyTip.Visibility = state;
                binding.rvAuthenticators.Visibility = state_reverse;
                //binding.speedDial.Visibility = fab_state;
                binding.layoutLoading.Visibility = loading_state;
            }

            ViewModel!.WhenAnyValue(x => x.IsAuthenticatorsEmpty)
                .SubscribeInMainThread(x => OnAuthenticatorsChanged(x, ViewModel!.IsFirstActivation))
                .AddTo(this);
            ViewModel!.WhenAnyValue(x => x.IsFirstLoadedAuthenticatorsEmpty)
                .SubscribeInMainThread(x =>
                {
                    if (!x) return;
                    OnAuthenticatorsChanged(ViewModel!.IsAuthenticatorsEmpty, false);
                })
                .AddTo(this);
            ViewModel!.WhenAnyValue(x => x.IsRefreshing)
                .SubscribeInMainThread(x =>
                {
                    if (binding == null) return;
                    if (x) return;
                    if (!binding.swipeRefreshLayout.Refreshing) return;
                    binding.swipeRefreshLayout.Refreshing = false;
                })
                .AddTo(this);

            var adapter = new GAPAuthenticatorAdapter(this, ViewModel!);
            adapter.ItemClick += (_, e) =>
            {
                AuthDetailActivity.StartActivity(Activity, e.Current.Id);
            };
            var layout = new LinearLayoutManager(Context, LinearLayoutManager.Vertical, false);
            binding!.rvAuthenticators.SetLayoutManager(layout);
            binding.rvAuthenticators.AddItemDecoration(VerticalItemViewDecoration2.Get(Context, Resource.Dimension.activity_vertical_margin/*, Resource.Dimension.fab_full_height*/));
            binding.rvAuthenticators.SetAdapter(adapter);

            //var actionItems = Enum2.GetAll<ActionItem>();
            //speedDialDict = actionItems.ToDictionary(x => x, x => binding.speedDial.AddActionItem(new SpeedDialActionItem.Builder((int)x, ToIconRes(x))
            //        .SetLabel(ToString2(x))
            //        .SetFabBackgroundColor(Context.GetColorCompat(Resource.Color.white))
            //        .SetFabImageTintColor(new(Context.GetColorCompat(Resource.Color.fab_icon_background_min)))
            //        .Create()));

            //binding.speedDial.SetOnActionSelectedListener(this);
            //binding.speedDial.SetOnChangeListener(this);

            binding.swipeRefreshLayout.InitDefaultStyles();
            binding.swipeRefreshLayout.SetOnRefreshListener(this);
        }

        //public override void OnStop()
        //{
        //    base.OnStop();
        //    AuthService.Current.SaveEditNameAuthenticators();
        //}

        public override void OnResume()
        {
            base.OnResume();
            if (!MainApplication.AllowScreenshots)
            {
                Activity.SetWindowSecure(true);
            }
        }

        public override void OnPause()
        {
            base.OnPause();
            Activity.SetWindowSecure(false);
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

        //static int ToIconRes(ActionItem action) => action switch
        //{
        //    ActionItem.Add => Resource.Drawable.baseline_add_black_24,
        //    ActionItem.Encrypt => Resource.Drawable.baseline_enhanced_encryption_black_24,
        //    ActionItem.Export => Resource.Drawable.baseline_save_alt_black_24,
        //    ActionItem.Lock => Resource.Drawable.baseline_lock_open_black_24,
        //    ActionItem.Refresh => Resource.Drawable.baseline_refresh_black_24,
        //    _ => throw new ArgumentOutOfRangeException(nameof(action), action, null),
        //};

        static ActionItem MenuIdResToEnum(int resId)
        {
            if (resId == Resource.Id.menu_add)
            {
                return ActionItem.Add;
            }
            else if (resId == Resource.Id.menu_encrypt)
            {
                return ActionItem.Encrypt;
            }
            else if (resId == Resource.Id.menu_export)
            {
                return ActionItem.Export;
            }
            else if (resId == Resource.Id.menu_lock)
            {
                return ActionItem.Lock;
            }
            return default;
        }

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
        //    if (Activity is IOnBackPressedCallback callback)
        //    {
        //        callback.HandleOnBackPressed = isOpen ? HandleOnBackPressed : null;
        //    }
        //}

        //bool HandleOnBackPressed()
        //{
        //    if (binding?.speedDial.IsOpen ?? false)
        //    {
        //        binding.speedDial.Close();
        //        return true;
        //    }
        //    return false;
        //}

        public override void OnDestroyView()
        {
            //speedDialDict = null;
            //if (Activity is IOnBackPressedCallback callback)
            //{
            //    callback.HandleOnBackPressed = null;
            //}
            menuBuilder = null;
            base.OnDestroyView();
        }

        void SwipeRefreshLayout.IOnRefreshListener.OnRefresh()
        {
            ViewModel!.MenuItemClick(ActionItem.Refresh);
        }
    }
}