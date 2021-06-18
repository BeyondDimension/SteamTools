using Android.Views;
using Binding;
using ReactiveUI;
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
            return Resource.Layout.layout_preference_large_button;
        }
    }

    internal sealed class MyPreferenceButtonViewHolder : BaseReactiveViewHolder<TViewModel>
    {
        readonly layout_preference_large_button binding;

        public MyPreferenceButtonViewHolder(View itemView) : base(itemView)
        {
            binding = new(itemView);
        }

        public override void OnBind()
        {
            base.OnBind();

            ViewModel!.WhenAnyValue(x => x.Icon).Subscribe(value =>
            {
                binding.ivIcon.SetImageResourceIcon(value);
            }).AddTo(this);
            ViewModel!.WhenAnyValue(x => x.Title).Subscribe(value =>
            {
                binding.tvTitle.Text = value;
            }).AddTo(this);
        }
    }
}