using Android.Graphics;
using Android.Views;
using System.Application.UI.Adapters;
using System.Application.UI.ViewModels;

// ReSharper disable once CheckNamespace
namespace AndroidX.RecyclerView.Widget
{
    /// <summary>
    /// 列表项的垂直间距
    /// </summary>
    public sealed class VerticalItemViewDecoration : RecyclerView.ItemDecoration
    {
        readonly int height;

        public VerticalItemViewDecoration(int height)
        {
            this.height = height;
        }

        public override void GetItemOffsets(Rect outRect, View view, RecyclerView parent, RecyclerView.State state)
        {
            base.GetItemOffsets(outRect, view, parent, state);

            var holder = parent.GetChildViewHolder(view);
            var position = holder.BindingAdapterPosition;
            if (parent.GetAdapter() is IReadOnlyViewModels<object> viewModels)
            {
                if (position < viewModels.ViewModels.Count - 1)
                {
                    // 除了末尾项之外，底部添加相同的间距
                    outRect.Bottom = height;
                }
            }
        }
    }
}