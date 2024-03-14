using BD.WTTS.Client.Resources;

namespace BD.WTTS.UI.Views.Pages;

public partial class HomePage : ReactiveUserControl<HomePageViewModel>
{
    public HomePage()
    {
        InitializeComponent();
        this.SetViewModel<HomePageViewModel>(useCache: true);

        //ShopsScrollViewer.PointerWheelChanged += ShopsScrollViewer_PointerWheelChanged;
    }

    //private void ShopsScrollViewer_PointerWheelChanged(object? sender, PointerWheelEventArgs e)
    //{
    //    if (sender is ScrollViewer scroll)
    //        if (scroll.IsPointerOver)
    //            if (scroll.Extent.Height > scroll.Viewport.Height || scroll.Extent.Width > scroll.Viewport.Width)
    //            {
    //                var delta = e.Delta;

    //                var isScrollStart = scroll.Offset.X == 0;
    //                var isScrollEnd = scroll.Offset.X + scroll.Bounds.Size.Width >= scroll.Extent.Width;
    //                if (isScrollStart && MathUtilities.IsOne(e.Delta.Y))
    //                {
    //                    return;
    //                }
    //                if (isScrollEnd && e.Delta.Y == -1)
    //                {
    //                    return;
    //                }
    //                if (MathUtilities.IsZero(delta.X))
    //                {
    //                    var horizontalOffset = scroll.Offset.X;
    //                    horizontalOffset -= delta.Y * 50; // 根据事件参数来调整滚动的距离
    //                    scroll.Offset = new Vector(horizontalOffset, scroll.Offset.Y);
    //                    e.Handled = true;
    //                }
    //            }
    //}
}
