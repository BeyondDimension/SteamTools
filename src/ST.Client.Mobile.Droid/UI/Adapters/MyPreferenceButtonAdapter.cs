using Android.Views;
using System.Application.UI.ViewModels;
using TViewHolder = System.Application.UI.Adapters.MyPreferenceButtonViewHolder;
using TViewModel = System.Application.UI.ViewModels.MyPageViewModel.PreferenceButtonViewModel;

namespace System.Application.UI.Adapters
{
    internal sealed class MyPreferenceButtonAdapter : BaseReactiveRecycleViewAdapter<TViewHolder, TViewModel>
    {
        public MyPreferenceButtonAdapter(MyPageViewModel viewModel) : base(viewModel.PreferenceButtons)
        {
        }

        public override int? GetLayoutResource(int viewType)
        {
            return null;
        }

        public override void OnBindViewHolder(TViewHolder holder, TViewModel item, int position)
        {
        }
    }

    internal sealed class MyPreferenceButtonViewHolder : BaseReactiveViewHolder<TViewModel>
    {
        public MyPreferenceButtonViewHolder(View itemView) : base(itemView)
        {
        }
    }
}