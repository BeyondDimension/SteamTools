using Avalonia.Animation.Easings;
using Avalonia.Controls.Shapes;

namespace BD.WTTS.UI.Views.Controls;

public class Ripple : Ellipse
{
    private static Easing _easing = new CircularEaseOut();

    public static Easing Easing
    {
        get => _easing;
        set => _easing = value;
    }

    public static readonly TimeSpan Duration = new TimeSpan(0, 0, 0, 0, 500);

    private double maxDiam;

    private readonly double endX;
    private readonly double endY;

    public Ripple(double outerWidth, double outerHeight)
    {
        Width = 0;
        Height = 0;

        maxDiam = Math.Sqrt(Math.Pow(outerWidth, 2) + Math.Pow(outerHeight, 2));
        endY = maxDiam - outerHeight;
        endX = maxDiam - outerWidth;
        HorizontalAlignment = HorizontalAlignment.Left;
        VerticalAlignment = VerticalAlignment.Top;
        Opacity = 1;
    }

    public void SetupInitialValues(PointerPressedEventArgs e, Control parent)
    {
        var pointer = e.GetPosition(parent);
        Margin = new Thickness(pointer.X, pointer.Y, 0, 0);
    }

    public void RunFirstStep()
    {
        Width = maxDiam;
        Height = maxDiam;
        Margin = new Thickness(-endX / 2, -endY / 2, 0, 0);
    }

    public void RunSecondStep()
    {
        Opacity = 0;
    }
}
