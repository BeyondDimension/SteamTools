using Android.Views;
using Binding;
using System.Application.UI.ViewModels;
using TViewHolder = System.Application.UI.Adapters.ExplorerViewHolder;
using TViewModel = System.Application.UI.ViewModels.ExplorerPageViewModel.PathInfoViewModel;

namespace System.Application.UI.Adapters
{
    internal sealed class ExplorerAdapter : BaseReactiveRecycleViewAdapter<TViewHolder, TViewModel>
    {
        public ExplorerAdapter(ExplorerPageViewModel viewModel) : base(viewModel.PathInfos)
        {
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
        }
    }
}