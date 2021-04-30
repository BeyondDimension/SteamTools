using System.Collections.Generic;

namespace System.Application.UI.Adapters
{
    public abstract class SortedListRecycleViewAdapter<TViewHolder, TViewModel, TViewType> :
        BaseRecycleViewAdapter<TViewHolder, TViewModel, TViewType>
        where TViewHolder : BaseViewHolder
        where TViewType : struct, IConvertible
    {
        protected IComparer<TViewModel> comparer;

        public SortedListRecycleViewAdapter(IComparer<TViewModel> comparer)
        {
            this.comparer = comparer;
        }

        public override IList<TViewModel> ViewModels
        {
            set
            {
                if (value is SortedList<TViewModel> l)
                {
                    viewModels = l;
                    comparer = l.Comparer;
                }
                else
                {
                    viewModels = CreateViewModels(value);
                }
            }
        }

        protected sealed override IList<TViewModel> CreateViewModels()
            => new SortedList<TViewModel>(comparer);

        protected sealed override IList<TViewModel> CreateViewModels(IEnumerable<TViewModel> newViewModels)
            => new SortedList<TViewModel>(comparer, newViewModels);
    }

    public abstract class SortedListRecycleViewAdapter<TViewHolder, TViewModel> : SortedListRecycleViewAdapter<TViewHolder, TViewModel, int>
       where TViewHolder : BaseViewHolder
    {
        public SortedListRecycleViewAdapter(IComparer<TViewModel> comparer) : base(comparer)
        {
        }
    }
}