using System;
using System.Numerics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Rendering.Composition;
using Avalonia.Rendering.Composition.Animations;

namespace BD.WTTS.UI.Views.Controls;

public partial class AuthenticatorItem : UserControl
{
    public AuthenticatorItem()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
    
    public static readonly DirectProperty<AuthenticatorItem, double> ValueProperty =
        AvaloniaProperty.RegisterDirect<AuthenticatorItem, double>(
            nameof(Value),
            o => o.Value,
            (o, v) => o.Value = v,
            defaultBindingMode: BindingMode.OneWay,
            enableDataValidation: true);
    
    public static readonly StyledProperty<int> HeightProperty =
        AvaloniaProperty.Register<AuthenticatorItem, int>(nameof(Height), defaultValue: 200);
    
    public static readonly StyledProperty<string> FirstTextProperty =
        AvaloniaProperty.Register<AuthenticatorItem, string>(
            nameof(FirstText));

    public static readonly StyledProperty<string> SecondTextProperty =
        AvaloniaProperty.Register<AuthenticatorItem, string>(
            nameof(SecondText));

    public static readonly StyledProperty<int> WidthProperty =
        AvaloniaProperty.Register<AuthenticatorItem, int>(nameof(Width), defaultValue: 200);

    public static readonly StyledProperty<int> StrokeWidthProperty =
        AvaloniaProperty.Register<AuthenticatorItem, int>(nameof(StrokeWidth), defaultValue: 10);

    public static readonly StyledProperty<IBrush> StrokeColorProperty =
        AvaloniaProperty.Register<AuthenticatorItem, IBrush>(
            nameof(StrokeColor), Brush.Parse("#61a4f0"));

    public static readonly StyledProperty<double> FirstTextSizeProperty =
        AvaloniaProperty.Register<AuthenticatorItem, double>(
            nameof(FirstTextSize), 30);

    public static readonly StyledProperty<double> SecondTextSizeProperty =
        AvaloniaProperty.Register<AuthenticatorItem, double>(
            nameof(SecondTextSize), 14);

    public static readonly StyledProperty<int> BorderWidthProperty = AvaloniaProperty.Register<AuthenticatorItem, int>(
        nameof(BorderWidth), 210);

    public static readonly StyledProperty<int> BorderHeightProperty = AvaloniaProperty.Register<AuthenticatorItem, int>(
        nameof(BorderHeight), 210);

    public int BorderHeight
    {
        get => GetValue(BorderHeightProperty);
        set => SetValue(BorderHeightProperty, value);
    }

    public int BorderWidth
    {
        get => GetValue(BorderWidthProperty);
        set => SetValue(BorderWidthProperty, value);
    }

    private double _value = 30;
    
    public double Value
    {
        get => _value;
        set
        {
            SetAndRaise(ValueProperty, ref _value, (double)(value * 12.00d));
        }
    }
    
    public int Width
    {
        get => GetValue(WidthProperty);
        set => SetValue(WidthProperty, value);
    }
    
    public int Height
    {
        get => GetValue(HeightProperty);
        set => SetValue(HeightProperty, value);
    }
    
    public string FirstText
    {
        get => GetValue(FirstTextProperty);
        set => SetValue(FirstTextProperty, value);
    }
    
    public string SecondText
    {
        get => GetValue(SecondTextProperty);
        set => SetValue(SecondTextProperty, value);
    }
    
    public double SecondTextSize
    {
        get => GetValue(SecondTextSizeProperty);
        set => SetValue(SecondTextSizeProperty, value);
    }

    public double FirstTextSize
    {
        get => GetValue(FirstTextSizeProperty);
        set => SetValue(FirstTextSizeProperty, value);
    }

    public IBrush StrokeColor
    {
        get => GetValue(StrokeColorProperty);
        set => SetValue(StrokeColorProperty, value);
    }

    public int StrokeWidth
    {
        get => GetValue(StrokeWidthProperty);
        set => SetValue(StrokeWidthProperty, value);
    }

    protected override void OnPointerEntered(PointerEventArgs e)
    {
        base.OnPointerEntered(e);

        var ec = ElementComposition.GetElementVisual(this);
        
        if (ec == null) return;
        
        var comp = ec.Compositor;
        
        var ani = comp.CreateVector3KeyFrameAnimation();
        ani.Duration = TimeSpan.FromMilliseconds(200);
        ani.StopBehavior = AnimationStopBehavior.SetToFinalValue;
        ani.InsertKeyFrame(1f, ec.Scale);
        ani.InsertKeyFrame(1.0f, ec.Scale + new Vector3(0.1f, 0.1f, 0));
        ani.Target = "Scale";
        
        ec.StartAnimation("Scale", ani);
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);

        var ec = ElementComposition.GetElementVisual(this);
        if (ec == null) return;
        
        var comp = ec.Compositor;
        
        var ani = comp.CreateVector3KeyFrameAnimation();
        ani.Duration = TimeSpan.FromMilliseconds(200);
        ani.StopBehavior = AnimationStopBehavior.SetToFinalValue;
        ani.InsertKeyFrame(1.0f, ec.Scale);
        ani.InsertKeyFrame(1.0f, ec.Scale);
        ani.Target = "Scale";

        ec.StartAnimation("Scale", ani);
    }

    private void AvaloniaObject_OnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (sender is TextBlock textBlock && e.Property==TextBlock.TextProperty)
        {
            // var keyframe1 = new KeyFrame()
            // {
            //     Setters = { new Setter(TextBlock.OpacityProperty, 0) }
            // };
            // var keyframe2 = new KeyFrame()
            // {
            //     Setters = { new Setter(TextBlock.OpacityProperty, 1) }
            // };
            // var ani = new Animation()
            // {
            //     Duration = TimeSpan.FromMilliseconds(500),
            //     Children = { keyframe1, keyframe2 }
            // };
            // ani.RunAsync(control: textBlock, null);
        }
    }
}