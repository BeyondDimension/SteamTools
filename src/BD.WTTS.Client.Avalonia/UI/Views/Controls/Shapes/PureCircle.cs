namespace BD.WTTS.UI.Views.Controls;

public class PureCircle : Control
{
    public static readonly StyledProperty<double> DiameterProperty = AvaloniaProperty.Register<PureCircle, double>(
        nameof(Diameter));

    public double Diameter
    {
        get => GetValue(DiameterProperty);
        set => SetValue(DiameterProperty, value);
    }

    public static readonly StyledProperty<IBrush?>
        BackgroundProperty = Border.BackgroundProperty.AddOwner<PureCircle>();

    public IBrush? Background
    {
        get => GetValue(BackgroundProperty);
        set => SetValue(BackgroundProperty, value);
    }

    static PureCircle()
    {
        AffectsMeasure<PureCircle>(DiameterProperty);
        AffectsArrange<PureCircle>(DiameterProperty);
        AffectsRender<PureCircle>(BackgroundProperty, DiameterProperty);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        double diameter = Math.Max(0, Diameter);
        return new Size(diameter, diameter);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        double diameter = Math.Max(0, Diameter);
        return new Size(diameter, diameter);
    }

    public override void Render(DrawingContext context)
    {
        double radius = Math.Max(0, Diameter) / 2;
        context.DrawEllipse(Background, null, new(radius, radius), radius, radius);
    }
}
