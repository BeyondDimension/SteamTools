using Android.Graphics;
using Android.Views;
using ReactiveUI;
using System.Application.UI.Adapters;
using System.Application.UI.ViewModels;

// ReSharper disable once CheckNamespace
namespace AndroidX.RecyclerView.Widget
{
    /// <summary>
    /// 列表组项的垂直间距
    /// </summary>
    public sealed class VerticalItemViewGroupDecoration : RecyclerView.ItemDecoration
    {
        readonly int height;

        public VerticalItemViewGroupDecoration(int height)
        {
            this.height = height;
        }

        public override void GetItemOffsets(Rect outRect, View view, RecyclerView parent, RecyclerView.State state)
        {
            base.GetItemOffsets(outRect, view, parent, state);

            var holder = parent.GetChildViewHolder(view);
            if (holder is IReadOnlyViewFor<IReadOnlyItemViewGroup> vf)
            {
                var lastPosition = holder.BindingAdapterPosition - 1;
                if (lastPosition >= 0 && parent.GetAdapter() is
                    IReadOnlyViewModels<IReadOnlyItemViewGroup> viewModels)
                {
                    var list = viewModels.ViewModels;
                    if (lastPosition < list.Count)
                    {
                        var nextItem = list[lastPosition];
                        if (vf.ViewModel?.ItemViewGroup != nextItem?.ItemViewGroup)
                        {
                            // 根据视图模型中的组判断上一个元素是否和当前元素的组值相同，不同则在顶部添加高度实现间距
                            outRect.Top = height;
                        }
                    }
                }
            }
        }
    }
}