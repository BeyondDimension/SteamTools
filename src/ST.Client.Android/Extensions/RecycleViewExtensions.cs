using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using AndroidX.Annotations;
using AndroidX.RecyclerView.Widget;
using System.Application;

// ReSharper disable once CheckNamespace
namespace System
{
    /// <summary>
    /// 对 <see cref="RecyclerView"/> 类的扩展函数集
    /// </summary>
    public static class RecycleViewExtensions
    {
        /// <summary>
        /// 设置使用线性布局管理列表，默认纵向，使用 <see cref="LinearLayoutManager2"/>
        /// </summary>
        /// <param name="recycleView"></param>
        /// <param name="orientation"></param>
        /// <param name="reverseLayout"></param>
        public static void SetLinearLayoutManager(this RecyclerView recycleView, int orientation = LinearLayoutManager.Vertical, bool reverseLayout = false)
        {
            var layout = new LinearLayoutManager2(recycleView.Context!, orientation, reverseLayout);
            recycleView.SetLayoutManager(layout);
        }

        /// <summary>
        /// 添加纵向列表组项间距，使用 <see cref="VerticalGroupItemDecoration(int)"/>
        /// </summary>
        /// <param name="recycleView"></param>
        /// <param name="height"></param>
        public static void AddVerticalGroupItemDecoration(this RecyclerView recycleView, int height)
        {
            VerticalGroupItemDecoration itemDecoration = new(height);
            recycleView.AddItemDecoration(itemDecoration);
        }

        /// <summary>
        /// 添加纵向列表组项间距，使用 <see cref="VerticalGroupItemDecoration(int)"/>
        /// </summary>
        /// <param name="recycleView"></param>
        /// <param name="heightResId"></param>
        public static void AddVerticalGroupItemDecorationIdRes(this RecyclerView recycleView, [IdRes] int heightResId)
        {
            var height = recycleView.Context!.GetDimensionPixelSize(heightResId);
            recycleView.AddVerticalGroupItemDecoration(height);
        }

        /// <summary>
        /// 添加纵向列表项间距，使用 <see cref="VerticalItemDecoration(int)"/>
        /// </summary>
        /// <param name="recycleView"></param>
        /// <param name="height"></param>
        public static void AddVerticalItemDecoration(this RecyclerView recycleView, int height)
        {
            VerticalItemDecoration itemDecoration = new(height);
            recycleView.AddItemDecoration(itemDecoration);
        }

        /// <summary>
        /// 添加纵向列表项间距，使用 <see cref="VerticalItemDecoration(int)"/>
        /// </summary>
        /// <param name="recycleView"></param>
        /// <param name="heightResId"></param>
        public static void AddVerticalItemDecorationIdRes(this RecyclerView recycleView, [IdRes] int heightResId)
        {
            var height = recycleView.Context!.GetDimensionPixelSize(heightResId);
            recycleView.AddVerticalItemDecoration(height);
        }

        /// <summary>
        /// 添加纵向列表项间距，使用 <see cref="VerticalItemDecoration2(int, int, bool)"/>
        /// </summary>
        /// <param name="recycleView"></param>
        /// <param name="height"></param>
        /// <param name="paddingBottom"></param>
        /// <param name="noTop"></param>
        public static void AddVerticalItemDecoration(this RecyclerView recycleView, int height, int paddingBottom, bool noTop = false)
        {
            VerticalItemDecoration2 itemDecoration = new(height, paddingBottom, noTop);
            recycleView.AddItemDecoration(itemDecoration);
        }

        /// <summary>
        /// 添加纵向列表项间距，使用 <see cref="VerticalItemDecoration2.Get(Context, int, int, bool)"/>
        /// </summary>
        /// <param name="recycleView"></param>
        /// <param name="heightResId"></param>
        /// <param name="paddingBottomResId"></param>
        /// <param name="noTop"></param>
        public static void AddVerticalItemDecorationIdRes(this RecyclerView recycleView, [IdRes] int heightResId, [IdRes] int paddingBottomResId = default, bool noTop = false)
        {
            var itemDecoration = VerticalItemDecoration2.Get(recycleView.Context!, heightResId, paddingBottomResId, noTop);
            recycleView.AddItemDecoration(itemDecoration);
        }

        /// <summary>
        /// 添加分割线，由 <see cref="Drawable"/> 绘制
        /// </summary>
        /// <param name="recycleView"></param>
        /// <param name="drawable"></param>
        /// <param name="orientation"></param>
        public static void AddDividerItemDecoration(this RecyclerView recycleView, Drawable drawable, int orientation = DividerItemDecoration.Vertical)
        {
            DividerItemDecoration itemDecoration = new(recycleView.Context!, orientation);
            itemDecoration.Drawable = drawable;
            recycleView.AddItemDecoration(itemDecoration);
        }

        /// <summary>
        /// 添加分割线，由 <see cref="Color"/> 绘制
        /// </summary>
        /// <param name="recycleView"></param>
        /// <param name="color"></param>
        /// <param name="size"></param>
        /// <param name="orientation"></param>
        public static void AddDividerItemDecoration(this RecyclerView recycleView, int colorArgb, int size = 1, int orientation = DividerItemDecoration.Vertical)
        {
            GradientDrawable shape = new();
            shape.SetColor(colorArgb);
            if (orientation == DividerItemDecoration.Vertical)
            {
                shape.SetSize(0, size);
            }
            else if (orientation == DividerItemDecoration.Horizontal)
            {
                shape.SetSize(size, 0);
            }
            recycleView.AddDividerItemDecoration(shape, orientation);
        }

        /// <summary>
        /// 添加分割线，由 <see cref="Color"/> 绘制
        /// </summary>
        /// <param name="recycleView"></param>
        /// <param name="colorResId"></param>
        /// <param name="size"></param>
        /// <param name="orientation"></param>
        public static void AddDividerItemDecorationIdRes(this RecyclerView recycleView, [IdRes] int colorResId, int size = 1, int orientation = DividerItemDecoration.Vertical)
        {
            var colorArgb = recycleView.Context!.GetColorCompat(colorResId);
            recycleView.AddDividerItemDecoration(colorArgb, size, orientation);
        }

        /// <summary>
        /// 添加分割线
        /// </summary>
        /// <param name="recycleView"></param>
        /// <param name="orientation"></param>
        public static void AddDividerItemDecorationIdRes(this RecyclerView recycleView, int orientation = DividerItemDecoration.Vertical)
        {
            var colorResId = Resource.Color.bg_divider;
            recycleView.AddDividerItemDecorationIdRes(colorResId, orientation: orientation);
        }
    }
}