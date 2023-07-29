namespace BD.WTTS.UI.Views.Windows;

public partial class DebugWindow : Window
{
    public DebugWindow()
    {
        InitializeComponent();

        TitleBar.PointerPressed += (i, e) =>
        {
            this.BeginMoveDrag(e);
        };

        TitleBar.DoubleTapped += (i, e) =>
        {
            this.WindowState = this.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        };

        SetupSide(Left, StandardCursorType.LeftSide, WindowEdge.West);
        SetupSide(Right, StandardCursorType.RightSide, WindowEdge.East);
        SetupSide(Top, StandardCursorType.TopSide, WindowEdge.North);
        SetupSide(Bottom, StandardCursorType.BottomSide, WindowEdge.South);
        SetupSide(TopLeft, StandardCursorType.TopLeftCorner, WindowEdge.NorthWest);
        SetupSide(TopRight, StandardCursorType.TopRightCorner, WindowEdge.NorthEast);
        SetupSide(BottomLeft, StandardCursorType.BottomLeftCorner, WindowEdge.SouthWest);
        SetupSide(BottomRight, StandardCursorType.BottomRightCorner, WindowEdge.SouthEast);
    }

    void SetupSide(Control ctl, StandardCursorType cursor, WindowEdge edge)
    {
        if (ctl != null)
        {
            ctl.Cursor = new Cursor(cursor);
            ctl.PointerPressed += (i, e) =>
            {
                this.BeginResizeDrag(edge, e);
            };
        }
    }
}
