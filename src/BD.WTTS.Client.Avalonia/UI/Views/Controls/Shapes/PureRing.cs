namespace BD.WTTS.UI.Views.Controls;

public class PureRing : PureCircle
{
    public static readonly StyledProperty<double> InnerDiameterProperty = AvaloniaProperty.Register<PureRing, double>(
        nameof(InnerDiameter));

    public double InnerDiameter
    {
        get => GetValue(InnerDiameterProperty);
        set => SetValue(InnerDiameterProperty, value);
    }

    static PureRing()
    {
        AffectsRender<PureCircle>(InnerDiameterProperty);
    }

    public override void Render(DrawingContext context)
    {
        double outer = Diameter / 2;
        double inner = InnerDiameter / 2;
        double thickness = outer - inner;
        context.DrawEllipse(null,
            new Pen(Background, outer - inner),
            new Point(outer, outer),
            outer - thickness / 2,
            outer - thickness / 2);
    }
}
