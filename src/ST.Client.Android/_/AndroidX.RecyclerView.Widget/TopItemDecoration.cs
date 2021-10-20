using Android.Content;
using Android.Graphics;
using Android.Views;
using AndroidX.Annotations;
using System.Application.UI.Adapters;

// ReSharper disable once CheckNamespace
namespace AndroidX.RecyclerView.Widget
{
    public abstract class TopItemDecoration<TViewModel> : RecyclerView.ItemDecoration
    {
        readonly int top;

        public TopItemDecoration(int top)
        {
            this.top = top;
        }

        public TopItemDecoration(Context context, [IdRes] int topResId) : this(context.Resources!.GetDimensionPixelSize(topResId))
        {
        }

        protected abstract bool IsHeader(TViewModel viewModel);

        public override void GetItemOffsets(Rect outRect, View view, RecyclerView parent, RecyclerView.State state)
        {
            base.GetItemOffsets(outRect, view, parent, state);

            var holder = parent.GetChildViewHolder(view);
            var position = holder.BindingAdapterPosition;

            if (position >= 0 && parent.GetAdapter() is IReadOnlyViewModels<TViewModel> viewModels && position < viewModels.ViewModels.Count)
            {
                var vm = viewModels.ViewModels[position];
                outRect.Top = IsHeader(vm) ? top : 0;
            }
        }
    }
}