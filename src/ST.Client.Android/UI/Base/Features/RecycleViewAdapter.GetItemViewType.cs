// TViewModel 继承 IReadOnlyItemViewType 接口时 实现 RecycleView.Adapter GetItemViewType(int position)

using System.Application.UI.Adapters.Internals;
using System.Application.UI.ViewModels;

// ReSharper disable once CheckNamespace
namespace System.Application.UI.Adapters
{
    partial class BaseRecycleViewAdapter<TViewHolder, TViewModel, TViewType>
    {
        static readonly Lazy<bool> isImplItemViewType = new(RecycleViewAdapterHelper.IsImplItemViewType<TViewModel>);

        public virtual int GetItemViewType(int position, TViewModel? viewModel)
        {
            if (isImplItemViewType.Value && viewModel is IReadOnlyItemViewType itemViewType)
            {
                return itemViewType.ItemViewType;
            }
            return base.GetItemViewType(position);
        }

        public sealed override int GetItemViewType(int position)
        {
            TViewModel? viewModel_ = default;
            (var isSuccess, var viewModel) = TryGetViewModelByPosition(position);
            if (isSuccess)
            {
                viewModel_ = viewModel;
            }
            return GetItemViewType(position, viewModel_);
        }
    }

    partial class BaseReactiveRecycleViewAdapter<TViewHolder, TViewModel, TViewType>
    {
        static readonly Lazy<bool> isImplItemViewType = new(RecycleViewAdapterHelper.IsImplItemViewType<TViewModel>);

        public override int GetItemViewType(int position, TViewModel? viewModel)
        {
            if (isImplItemViewType.Value && viewModel is IReadOnlyItemViewType itemViewType)
            {
                return itemViewType.ItemViewType;
            }
            return base.GetItemViewType(position, viewModel);
        }
    }

    public static partial class RecycleViewAdapterHelper
    {
        public static bool IsImplItemViewType<TViewModel>()
        {
            var r = typeof(IReadOnlyItemViewType).IsAssignableFrom(typeof(TViewModel));
            return r;
        }
    }
}