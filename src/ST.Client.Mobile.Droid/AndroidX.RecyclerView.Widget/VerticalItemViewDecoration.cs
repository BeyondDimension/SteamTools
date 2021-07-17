using Android.Content;
using Android.Graphics;
using Android.Views;
using AndroidX.Annotations;
using System.Application.UI.Adapters;

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

    public sealed class VerticalItemViewDecoration2 : RecyclerView.ItemDecoration
    {
        readonly int height;
        readonly int paddingBottom;

        public bool PaddingBottomWithHeight { get; set; }

        public VerticalItemViewDecoration2(int height, int paddingBottom)
        {
            this.height = height;
            this.paddingBottom = paddingBottom;
        }

        public static VerticalItemViewDecoration2 Get(Context context, [IdRes] int heightResId, [IdRes] int paddingBottomResId)
        {
            var height = context.Resources!.GetDimensionPixelSize(heightResId);
            var paddingBottom = context.Resources!.GetDimensionPixelSize(paddingBottomResId);
            return new(height, paddingBottom);
        }

        public override void GetItemOffsets(Rect outRect, View view, RecyclerView parent, RecyclerView.State state)
        {
            base.GetItemOffsets(outRect, view, parent, state);

            var holder = parent.GetChildViewHolder(view);
            var position = holder.BindingAdapterPosition;
            if (parent.GetAdapter() is IReadOnlyViewModels<object> viewModels)
            {
                var count = viewModels.ViewModels.Count;
                if (count == 0)
                {
                    return;
                }
                else if (position == 0)
                {
                    outRect.Top = height;
                }
                if (position < viewModels.ViewModels.Count - 1)
                {
                    outRect.Bottom = height;
                }
                else
                {
                    outRect.Bottom = PaddingBottomWithHeight ? paddingBottom + height : paddingBottom;
                }
            }
        }
    }
}