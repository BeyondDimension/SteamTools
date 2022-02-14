using Android.Views;
using Binding;
using ReactiveUI;
using System.Application.UI.ViewModels;
using TViewHolder = System.Application.UI.Adapters.ExplorerViewHolder;
using TViewModel = System.Application.UI.ViewModels.ExplorerPageViewModel.PathInfoViewModel;

namespace System.Application.UI.Adapters
{
    internal sealed class ExplorerAdapter : BaseReactiveRecycleViewAdapter<TViewHolder, TViewModel>, IReadOnlyViewFor<ExplorerPageViewModel>
    {
        readonly ExplorerPageViewModel viewModel;

        ExplorerPageViewModel? IReadOnlyViewFor<ExplorerPageViewModel>.ViewModel => viewModel;

        public ExplorerAdapter(ExplorerPageViewModel viewModel) : base(viewModel.PathInfos)
        {
            this.viewModel = viewModel;
        }

        public override int? GetLayoutResource(int viewType)
        {
            return Resource.Layout.layout_file_or_dir;
        }
    }

    internal sealed class ExplorerViewHolder : BaseReactiveViewHolder<TViewModel>
    {
        readonly layout_file_or_dir binding;

        public ExplorerViewHolder(View itemView) : base(itemView)
        {
            binding = new(itemView);
        }

        public override void OnBind()
        {
            base.OnBind();

            binding.ivIcon.SetImageResource(
                ViewModel!.IsDirectory ?
                Resource.Drawable.baseline_folder_black_24 :
                Resource.Drawable.baseline_description_black_24);
            binding.tvName.Text = ViewModel.Name;
            binding.tvDesc.Text = ViewModel.Desc;

            if (BindingAdapter is IReadOnlyViewFor<ExplorerPageViewModel> i)
            {
                var pageVM = i.ViewModel;
                pageVM.WhenAnyValue(x => x.IsEditMode).SubscribeInMainThread(x =>
                {
                    var visibility = x ? ViewStates.Visible : ViewStates.Gone;
                    if (binding.checkbox.Visibility != visibility)
                    {
                        binding.checkbox.Visibility = visibility;
                    }
                });
            }

            ViewModel.WhenAnyValue(x => x.Checked).SubscribeInMainThread(x =>
            {
                if (binding.checkbox.Checked != x)
                    binding.checkbox.Checked = x;
            }).AddTo(this);
        }

        public override bool ItemLongClickable => true;
    }
}