using Android.Runtime;
using Android.Views;
using AndroidX.RecyclerView.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using Binding;
using ReactiveUI;
using System.Application.Services;
using System.Application.UI.Adapters;
using System.Application.UI.Resx;
using System.Application.UI.ViewModels;

namespace System.Application.UI.Fragments
{
    [Register(JavaPackageConstants.Fragments + nameof(CommunityFixFragment))]
    internal sealed class CommunityFixFragment : BaseFragment<fragment_community_fix, CommunityProxyPageViewModel>, SwipeRefreshLayout.IOnRefreshListener
    {
        protected override int? LayoutResource => Resource.Layout.fragment_community_fix;

        protected sealed override CommunityProxyPageViewModel? OnCreateViewModel()
        {
            return IViewModelManager.Instance.GetMainPageViewModel<CommunityProxyPageViewModel>();
        }

        public override void OnCreateView(View view)
        {
            base.OnCreateView(view);

            R.Subscribe(() =>
            {
                if (binding == null) return;
                binding.btnStart.Text = AppResources.CommunityFix_StartProxy;
                binding.btnStop.Text = AppResources.CommunityFix_StopProxy;
                binding.tvProxyMode.Text = AppResources.CommunityFix_ProxyMode + AppResources.CommunityFix_ProxyMode_WinSystem;
                binding.tvAccelerationsEnable.Text = AppResources.CommunityFix_AccelerationsEnable;
                binding.tvScriptsEnable.Text = AppResources.CommunityFix_ScriptsEnable;
            }).AddTo(this);

            var proxyS = ProxyService.Current;
            proxyS.WhenAnyValue(x => x.AccelerateTime).SubscribeInMainThread(value =>
            {
                if (binding == null) return;
                binding.tvAccelerateTime.Text = AppResources.CommunityFix_AlreadyProxy + value.ToString("hh:mm:ss");
            }).AddTo(this);
            proxyS.WhenAnyValue(x => x.ProxyStatus).SubscribeInMainThread(value =>
            {
                if (binding == null) return;
                binding.layoutRootCommunityFixContentReady.Visibility = !value ? ViewStates.Visible : ViewStates.Gone;
                binding.layoutRootCommunityFixContentStarting.Visibility = value ? ViewStates.Visible : ViewStates.Gone;
            }).AddTo(this);

            var ctx = RequireContext();
            var adapter = new AccelerateProjectGroupAdapter();
            adapter.ItemClick += (_, e) =>
            {
                e.Current.Enable = !e.Current.Enable;
            };
            var layout = new LinearLayoutManager2(ctx, LinearLayoutManager.Vertical, false);
            binding!.rvAccelerateProjectGroup.SetLayoutManager(layout);
            binding.rvAccelerateProjectGroup.AddItemDecoration(VerticalItemDecoration2.Get(ctx, Resource.Dimension.activity_vertical_margin, Resource.Dimension.fab_height_with_margin_top_bottom));
            binding.rvAccelerateProjectGroup.SetAdapter(adapter);

            binding.swipeRefreshLayout.InitDefaultStyles();
            binding.swipeRefreshLayout.SetOnRefreshListener(this);

            SetOnClickListener(binding.btnStart, binding.btnStop);
        }

        protected override bool OnClick(View view)
        {
            if (view.Id == Resource.Id.btnStart)
            {
                ViewModel!.StartProxyButton_Click(true);
                return true;
            }
            else if (view.Id == Resource.Id.btnStop)
            {
                ViewModel!.StartProxyButton_Click(false);
                return true;
            }
            return base.OnClick(view);
        }

        void SwipeRefreshLayout.IOnRefreshListener.OnRefresh()
        {
            binding!.swipeRefreshLayout.Refreshing = false;
            ViewModel!.RefreshButton_Click();
        }
    }
}