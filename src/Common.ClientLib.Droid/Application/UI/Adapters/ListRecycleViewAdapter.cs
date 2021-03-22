using System.Collections.Generic;

namespace System.Application.UI.Adapters
{
    public abstract class ListRecycleViewAdapter<TViewHolder, TViewModel, TViewType> : BaseRecycleViewAdapter<TViewHolder, TViewModel, TViewType>
        where TViewHolder : BaseViewHolder
        where TViewType : struct, IConvertible
    {
        protected sealed override IList<TViewModel> CreateViewModels()
            => new List<TViewModel>();

        protected sealed override IList<TViewModel> CreateViewModels(IEnumerable<TViewModel> newViewModels)
            => new List<TViewModel>(newViewModels);
    }

    public abstract class ListRecycleViewAdapter<TViewHolder, TViewModel> :
       ListRecycleViewAdapter<TViewHolder, TViewModel, int>
       where TViewHolder : BaseViewHolder
    {
    }
}