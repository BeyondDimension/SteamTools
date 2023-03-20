using Avalonia.Controls;
using System.Reactive.Disposables;

namespace BD.WTTS.UI.Views.Windows;

public partial class DebugWindow : CoreWindow
{
    public DebugWindow()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
    }

    void SetupSide(string name, StandardCursorType cursor, WindowEdge edge)
    {
        var ctl = this.Get<Control>(name);
        ctl.Cursor = new Cursor(cursor);
        ctl.PointerPressed += (i, e) =>
        {
            PlatformImpl?.BeginResizeDrag(edge, e);
        };
    }

    void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);

        var titleBar = this.Get<Control>("TitleBar");

        titleBar.PointerPressed += (i, e) =>
        {
            PlatformImpl?.BeginMoveDrag(e);
        };

        titleBar.DoubleTapped += (i, e) =>
        {
            this.WindowState = this.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        };

        //SetupSide("Left", StandardCursorType.LeftSide, WindowEdge.West);
        //SetupSide("Right", StandardCursorType.RightSide, WindowEdge.East);
        //SetupSide("Top", StandardCursorType.TopSide, WindowEdge.North);
        //SetupSide("Bottom", StandardCursorType.BottomSide, WindowEdge.South);
        //SetupSide("TopLeft", StandardCursorType.TopLeftCorner, WindowEdge.NorthWest);
        //SetupSide("TopRight", StandardCursorType.TopRightCorner, WindowEdge.NorthEast);
        //SetupSide("BottomLeft", StandardCursorType.BottomLeftCorner, WindowEdge.SouthWest);
        //SetupSide("BottomRight", StandardCursorType.BottomRightCorner, WindowEdge.SouthEast);
    }

}
