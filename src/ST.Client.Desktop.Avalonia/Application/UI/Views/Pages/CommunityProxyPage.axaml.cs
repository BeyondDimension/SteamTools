using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Selection;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using System.Application.UI.ViewModels;

namespace System.Application.UI.Views.Pages
{
    public class CommunityProxyPage : ReactiveUserControl<CommunityProxyPageViewModel>
    {
        //private ListBox _items;
        //private IScrollable _scroll;
        //private Button _left;
        //private Button _right;

        public CommunityProxyPage()
        {
            InitializeComponent();
            //_scroll = _items.Scroll;
            //_items.PointerWheelChanged += _items_PointerWheelChanged;

        }

        //private void _items_PointerWheelChanged(object sender, Avalonia.Input.PointerWheelEventArgs e)
        //{
        //    var Extent = _scroll.Extent;
        //    var Offset = _scroll.Offset;
        //    var Viewport = _scroll.Viewport;
        //    if (Extent.Height > Viewport.Height || Extent.Width > Viewport.Width)
        //    {
        //        double x = Offset.X;
        //        double y = Offset.Y;

        //        if (Extent.Height > Viewport.Height)
        //        {
        //            double height = 50;
        //            y += -e.Delta.Y * height;
        //            y = Math.Max(y, 0);
        //            y = Math.Min(y, Extent.Height - Viewport.Height);
        //        }
        //        if (Extent.Height == Viewport.Height + y && Extent.Width > Viewport.Width)
        //        {
        //            double width = 50;
        //            x += -e.Delta.Y * width;
        //            x = Math.Max(x, 0);
        //            x = Math.Min(x, Extent.Width - Viewport.Width);
        //        }

        //        _scroll.Offset = new Vector(x, y);
        //        e.Handled = true;
        //    }
        //}

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            //_items = this.FindControl<ListBox>("items");
            //_scroll = this.FindControl<ScrollViewer>("scroll");
            //_left = this.FindControl<Button>("left");
            //_right = this.FindControl<Button>("right");

        }
    }
}