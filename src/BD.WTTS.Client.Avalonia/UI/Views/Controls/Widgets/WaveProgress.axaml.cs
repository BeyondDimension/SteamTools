namespace BD.WTTS.UI.Views.Controls;

public sealed partial class WaveProgress : UserControl
{
    public static readonly DirectProperty<WaveProgress, double> ValueProperty =
        AvaloniaProperty.RegisterDirect<WaveProgress, double>(
            nameof(Value),
            o => o.Value,
            (o, v) => o.Value = v,
            defaultBindingMode: BindingMode.TwoWay,
            enableDataValidation: true);

    public static readonly StyledProperty<bool> IsTextVisibleProperty =
        AvaloniaProperty.Register<WaveProgress, bool>(nameof(IsTextVisible), defaultValue: true);

    public WaveProgress()
    {
        InitializeComponent();
        //var theme = App.Current.thm;
        //theme.OnBaseThemeChanged += _ =>
        //{
        //    Value++;
        //    Value--;
        //};
        //theme.OnColorThemeChanged += _ =>
        //{
        //    Value++;
        //    Value--;
        //};
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private double _value = 65;

    public double Value
    {
        get => _value;
        set
        {
            if (value is < 0 or > 100)
                return;

            SetAndRaise(ValueProperty, ref _value, value);
        }
    }

    public bool IsTextVisible
    {
        get
        {
            return GetValue(IsTextVisibleProperty);
        }

        set
        {
            SetValue(IsTextVisibleProperty, value);
        }
    }
}