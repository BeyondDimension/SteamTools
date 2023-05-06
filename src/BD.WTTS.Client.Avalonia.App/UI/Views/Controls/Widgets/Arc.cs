namespace BD.WTTS.UI.Views.Controls;

public class Arc : Control
{
    static Arc()
    {
        AffectsRender<Arc>(StrokeProperty,
            StrokeThicknessProperty,
            StartAngleProperty,
            SweepAngleProperty);
    }

    public IBrush Stroke
    {
        get => GetValue(StrokeProperty);
        set => SetValue(StrokeProperty, value);
    }

    public readonly static StyledProperty<IBrush> StrokeProperty =
        AvaloniaProperty.Register<Arc, IBrush>(nameof(Stroke));

    public double StrokeThickness
    {
        get => GetValue(StrokeThicknessProperty);
        set => SetValue(StrokeThicknessProperty, value);
    }

    public readonly static StyledProperty<double> StrokeThicknessProperty =
        AvaloniaProperty.Register<Arc, double>(nameof(StrokeThickness));

    public double StartAngle
    {
        get => GetValue(StartAngleProperty);
        set => SetValue(StartAngleProperty, value);
    }

    public readonly static StyledProperty<double> StartAngleProperty =
        AvaloniaProperty.Register<Arc, double>(nameof(StartAngle));

    public double SweepAngle
    {
        get => GetValue(SweepAngleProperty);
        set => SetValue(SweepAngleProperty, value);
    }

    public static readonly StyledProperty<double> SweepAngleProperty =
        AvaloniaProperty.Register<Arc, double>(nameof(SweepAngle));

    public override void Render(DrawingContext context)
    {
        const double offsetStroke = 0.5;
        var o = StrokeThickness + offsetStroke;

        // Create main circle for draw circle
        var mainCircle =
            new EllipseGeometry(new Rect(o / 2, o / 2, Bounds.Width - o, Bounds.Height - o));

        var paint = new Pen(Stroke, StrokeThickness);

        // Push generated clip geometry for clipping circle figure
        using (context.PushGeometryClip(GetClip()))
        {
            context.DrawGeometry(Brushes.Transparent, paint, mainCircle);
        }
    }

    // not-TO-DO: Optimal clip geometry generator
    // I gave up, using avaloniaUI default arc instead now:(
    // but I give you choice to use this "High-precision" arc if you needed it in ProgressBar btw
    // only you have to do is define a custom style with replacing default circular class of ProgressBar
    // or you can use it anywhere

    // Well I did some small changes here to not parsing figure string,
    // but build figure from stream geometry context.
    // This changes may prevent performance waste a little bit.

    // Clip geometry generator
    private StreamGeometry GetClip()
    {
        var offset = StartAngle;

        var w = Bounds.Width;
        var h = Bounds.Height;

        var halfW = w / 2;
        var halfH = h / 2;

        var geometry = new StreamGeometry();

        var sweep = SweepAngle;

        if (sweep == 0)
            return geometry;

        using var ctx = geometry.Open();

        // Center point
        ctx.BeginFigure(new Point(halfW, halfH), true);

        const int len = 24;

        for (var i = 0; i < len; i++)
        {
            var l = offset + sweep * i / len;

            ctx.LineTo(DegToPoint(l, halfW, halfH));
        }

        ctx.LineTo(DegToPoint(offset + sweep, halfW, halfH));

        ctx.EndFigure(true);
        return geometry;
    }

    private const double Rad = Math.PI / 180d;

    private static double Round(double v) => Math.Round(v);

    private static Point DegToPoint(double deg, double halfW, double halfH)
    {
        var rad = deg * Rad;

        var x = halfW + Round(halfW * Math.Cos(rad));
        var y = halfH + Round(halfH * Math.Sin(rad));

        return new Point(x, y);
    }
}