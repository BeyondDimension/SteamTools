using AndroidX.RecyclerView.Widget;

// ReSharper disable once CheckNamespace
namespace System.Application.UI.Adapters
{
    partial class BaseRecycleViewAdapter<TViewHolder, TViewModel, TViewType>
    {
        public abstract void OnBindViewHolder(TViewHolder holder, TViewModel item, int position);

        public sealed override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            if (holder is TViewHolder viewHolder)
            {
                (var isSuccess, var viewModel) = TryGetViewModelByPosition(position);
                if (isSuccess)
                {
                    OnBindViewHolder(viewHolder, viewModel!, position);
                }
            }
        }
    }

    partial class BaseReactiveRecycleViewAdapter<TViewHolder, TViewModel, TViewType>
    {
        //public abstract void OnBindViewHolder(TViewHolder holder, TViewModel item, int position);

        public sealed override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            base.OnBindViewHolder(holder, position);
            if (holder is TViewHolder viewHolder)
            {
                //OnBindViewHolder(viewHolder, viewHolder.ViewModel, position);
                viewHolder.OnBind();
            }
        }
    }
}