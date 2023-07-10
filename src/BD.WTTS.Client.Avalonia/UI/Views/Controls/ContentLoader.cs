using Avalonia.Controls;
using DynamicData;
using FluentAvalonia.Core;

namespace BD.WTTS.UI.Views.Controls;

public class ContentLoader : ContentControl
{
    /// <summary>
    /// Defines the <see cref="IsLoading"/> property
    /// </summary>
    public static readonly StyledProperty<bool> IsLoadingProperty =
        AvaloniaProperty.Register<ContentLoader, bool>(nameof(IsLoading), true);

    /// <summary>
    /// Defines the <see cref="NoResultMessage"/> property
    /// </summary>
    public static readonly StyledProperty<string?> NoResultMessageProperty =
        AvaloniaProperty.Register<ContentLoader, string?>(nameof(NoResultMessage), null);

    /// <summary>
    /// Defines the <see cref="ProgressValue"/> property
    /// </summary>
    public static readonly StyledProperty<double> ProgressValueProperty =
        ProgressBar.ValueProperty.AddOwner<ContentLoader>();

    /// <summary>
    /// Defines the <see cref="Minimum"/> property
    /// </summary>
    public static readonly StyledProperty<double> MinimumProperty =
        ProgressBar.MinimumProperty.AddOwner<ContentLoader>();

    /// <summary>
    /// Defines the <see cref="Maximum"/> property
    /// </summary>
    public static readonly StyledProperty<double> MaximumProperty =
        ProgressBar.MaximumProperty.AddOwner<ContentLoader>();

    /// <summary>
    /// Defines the <see cref="IsIndeterminate"/> property
    /// </summary>
    public static readonly StyledProperty<bool> IsIndeterminateProperty =
        AvaloniaProperty.Register<ContentLoader, bool>(nameof(IsIndeterminate), true);

    /// <summary>
    /// Defines the <see cref="IsShowNoResultText"/> property
    /// </summary>
    public static readonly StyledProperty<bool> IsShowNoResultTextProperty =
        AvaloniaProperty.Register<ContentLoader, bool>(nameof(IsShowNoResultText), false);

    /// <summary>
    /// Defines the <see cref="ItemsSource"/> property.
    /// </summary>
    public static readonly StyledProperty<IEnumerable?> ItemsSourceProperty =
        ItemsControl.ItemsSourceProperty.AddOwner<ContentLoader>();

    /// <summary>
    /// 是否正在加载中
    /// </summary>
    public bool IsLoading
    {
        get => GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }

    public bool IsIndeterminate
    {
        get => GetValue(IsIndeterminateProperty);
        set => SetValue(IsIndeterminateProperty, value);
    }

    public bool IsShowNoResultText
    {
        get => GetValue(IsShowNoResultTextProperty);
        set => SetValue(IsShowNoResultTextProperty, value);
    }

    /// <summary>
    /// 无结果时提示
    /// </summary>
    public string? NoResultMessage
    {
        get => GetValue(NoResultMessageProperty);
        set => SetValue(NoResultMessageProperty, value);
    }

    /// <summary>
    /// 加载进度
    /// </summary>
    public double ProgressValue
    {
        get => GetValue(ProgressValueProperty);
        set => SetValue(ProgressValueProperty, value);
    }

    public double Minimum
    {
        get => GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    public double Maximum
    {
        get => GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    public IEnumerable? ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.Property == ProgressValueProperty)
        {
            if (e.NewValue is double value)
            {
                if (value > 0)
                {
                    IsIndeterminate = false;
                    IsLoading = true;
                }
                else if (value == Maximum)
                {
                    IsLoading = false;
                }
            }
        }
        else if (e.Property == ItemsSourceProperty)
        {
            var items = e.GetNewValue<IEnumerable?>();
            if (items == null)
            {
                IsLoading = true;
                IsShowNoResultText = false;
            }
            else if (items.Count() == 0)
            {
                IsLoading = false;
                IsShowNoResultText = true;
            }
            else if (items.Count() != 0)
            {
                IsLoading = false;
                IsShowNoResultText = false;
            }
            else
            {
                IsLoading = true;
                IsShowNoResultText = false;
            }
        }
    }
}
