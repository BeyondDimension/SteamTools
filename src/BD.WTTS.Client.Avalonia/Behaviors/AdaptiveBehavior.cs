using Avalonia.Reactive;
using Avalonia.Xaml.Interactions.Responsive;
using Avalonia.Xaml.Interactivity;

namespace BD.WTTS.Behaviors;

/// <summary>
/// Observes <see cref="Behavior{T}.AssociatedObject"/> control or <see cref="SourceControl"/> control <see cref="Visual.Bounds"/> property changes and if triggered sets or removes style classes when conditions from <see cref="AdaptiveClassSetter"/> are met.
/// </summary>
public class AdaptiveBehavior : Behavior<Control>
{
    private IDisposable? _disposable;
    private AvaloniaList<AdaptiveClassSetter>? _setters;

    /// <summary>
    /// Identifies the <seealso cref="SourceControl"/> avalonia property.
    /// </summary>
    public static readonly StyledProperty<Control?> SourceControlProperty =
        AvaloniaProperty.Register<AdaptiveBehavior, Control?>(nameof(SourceControl));

    /// <summary>
    /// Identifies the <seealso cref="TargetControl"/> avalonia property.
    /// </summary>
    public static readonly StyledProperty<Control?> TargetControlProperty =
        AvaloniaProperty.Register<AdaptiveBehavior, Control?>(nameof(TargetControl));

    /// <summary>
    /// Identifies the <seealso cref="Setters"/> avalonia property.
    /// </summary>
    public static readonly DirectProperty<AdaptiveBehavior, AvaloniaList<AdaptiveClassSetter>> SettersProperty =
        AvaloniaProperty.RegisterDirect<AdaptiveBehavior, AvaloniaList<AdaptiveClassSetter>>(nameof(Setters), t => t.Setters);

    /// <summary>
    /// Gets or sets the the source control that <see cref="Visual.BoundsProperty"/> property are observed from, if not set <see cref="Behavior{T}.AssociatedObject"/> is used. This is a avalonia property.
    /// </summary>
    [ResolveByName]
    public Control? SourceControl
    {
        get => GetValue(SourceControlProperty);
        set => SetValue(SourceControlProperty, value);
    }

    /// <summary>
    /// Gets or sets the target control that class name that should be added or removed when triggered, if not set <see cref="Behavior{T}.AssociatedObject"/> is used or <see cref="AdaptiveClassSetter.TargetControl"/> from <see cref="AdaptiveClassSetter"/>. This is a avalonia property.
    /// </summary>
    [ResolveByName]
    public Control? TargetControl
    {
        get => GetValue(TargetControlProperty);
        set => SetValue(TargetControlProperty, value);
    }

    /// <summary>
    /// Gets adaptive class setters collection. This is a avalonia property.
    /// </summary>
    [Content]
    public AvaloniaList<AdaptiveClassSetter> Setters => _setters ??= new AvaloniaList<AdaptiveClassSetter>();

    /// <inheritdoc/>
    protected override void OnAttachedToVisualTree()
    {
        base.OnAttachedToVisualTree();

        StopObserving();
        StartObserving();
    }

    /// <inheritdoc/>
    protected override void OnDetachedFromVisualTree()
    {
        base.OnDetachedFromVisualTree();

        StopObserving();
    }

    private void StartObserving()
    {
        var sourceControl = GetValue(SourceControlProperty) is not null
            ? SourceControl
            : AssociatedObject;

        if (sourceControl is not null)
        {
            _disposable = ObserveBounds(sourceControl);
        }
    }

    private void StopObserving()
    {
        _disposable?.Dispose();
    }

    private IDisposable ObserveBounds(Control sourceControl)
    {
        if (sourceControl is null)
        {
            throw new ArgumentNullException(nameof(sourceControl));
        }

        return sourceControl.GetObservable(Visual.BoundsProperty)
            .Subscribe(new AnonymousObserver<Rect>(bounds => ValueChanged(sourceControl, Setters, bounds)));
    }

    private void ValueChanged(Control? sourceControl, AvaloniaList<AdaptiveClassSetter>? setters, Rect bounds)
    {
        if (sourceControl is null || setters is null)
        {
            return;
        }

        foreach (var setter in setters)
        {
            var isMinOrMaxWidthSet = setter.IsSet(AdaptiveClassSetter.MinWidthProperty)
                                     || setter.IsSet(AdaptiveClassSetter.MaxWidthProperty);
            var widthConditionTriggered = GetResult(setter.MinWidthOperator, bounds.Width, setter.MinWidth)
                                          && GetResult(setter.MaxWidthOperator, bounds.Width, setter.MaxWidth);

            var isMinOrMaxHeightSet = setter.IsSet(AdaptiveClassSetter.MinHeightProperty)
                                      || setter.IsSet(AdaptiveClassSetter.MaxHeightProperty);
            var heightConditionTriggered = GetResult(setter.MinHeightOperator, bounds.Height, setter.MinHeight)
                                           && GetResult(setter.MaxHeightOperator, bounds.Height, setter.MaxHeight);

            var isAddClassTriggered = isMinOrMaxWidthSet switch
            {
                true when !isMinOrMaxHeightSet => widthConditionTriggered,
                false when isMinOrMaxHeightSet => heightConditionTriggered,
                true when isMinOrMaxHeightSet => widthConditionTriggered && heightConditionTriggered,
                _ => false
            };

            var targetControl = setter.GetValue(AdaptiveClassSetter.TargetControlProperty) is not null
                ? setter.TargetControl
                : GetValue(TargetControlProperty) is not null
                    ? TargetControl
                    : AssociatedObject;

            if (targetControl is not null)
            {
                var className = setter.ClassName;
                var isPseudoClass = setter.IsPseudoClass;

                if (isAddClassTriggered)
                {
                    Add(targetControl, className, isPseudoClass);
                }
                else
                {
                    Remove(targetControl, className, isPseudoClass);
                }
            }
        }
    }

    private bool GetResult(ComparisonConditionType comparisonConditionType, double property, double value)
    {
        return comparisonConditionType switch
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            ComparisonConditionType.Equal => property == value,
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            ComparisonConditionType.NotEqual => property != value,
            ComparisonConditionType.LessThan => property < value,
            ComparisonConditionType.LessThanOrEqual => property <= value,
            ComparisonConditionType.GreaterThan => property > value,
            ComparisonConditionType.GreaterThanOrEqual => property >= value,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private static void Add(Control targetControl, string? className, bool isPseudoClass)
    {
        if (className is null || string.IsNullOrEmpty(className) || targetControl.Classes.Contains(className))
        {
            return;
        }

        if (isPseudoClass)
        {
            ((IPseudoClasses)targetControl.Classes).Add(className);
        }
        else
        {
            targetControl.Classes.Add(className);
        }
    }

    private static void Remove(Control targetControl, string? className, bool isPseudoClass)
    {
        if (className is null || string.IsNullOrEmpty(className) || !targetControl.Classes.Contains(className))
        {
            return;
        }

        if (isPseudoClass)
        {
            ((IPseudoClasses)targetControl.Classes).Remove(className);
        }
        else
        {
            targetControl.Classes.Remove(className);
        }
    }
}
