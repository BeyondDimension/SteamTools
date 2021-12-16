using Android.Runtime;
using Android.Views;
using AndroidX.RecyclerView.Widget;
using Binding;
using System.Application.Services;
using System.Application.UI.Adapters;
using System.Application.UI.Resx;
using System.Application.UI.ViewModels;

namespace System.Application.UI.Fragments
{
    [Register(JavaPackageConstants.Fragments + nameof(CommunityFixFragment))]
    internal sealed class CommunityFixFragment : BaseFragment<fragment_community_fix, CommunityProxyPageViewModel>
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
                binding.btnStart.Text = AppResources.CommunityFix_Acceleration;
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

            SetOnClickListener(binding.btnStart);
        }
    }
}