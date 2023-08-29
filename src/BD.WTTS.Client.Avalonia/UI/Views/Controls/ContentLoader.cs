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
    public static readonly StyledProperty<object?> NoResultMessageProperty =
        AvaloniaProperty.Register<ContentLoader, object?>(nameof(NoResultMessage), null);

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
    /// Defines the <see cref="CustomLoadingText"/> property
    /// </summary>
    public static readonly StyledProperty<string?> CustomLoadingTextProperty =
        AvaloniaProperty.Register<ContentLoader, string?>(nameof(CustomLoadingText), null);

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
    /// 自定义加载提示
    /// </summary>
    public string? CustomLoadingText
    {
        get => GetValue(CustomLoadingTextProperty);
        set => SetValue(CustomLoadingTextProperty, value);
    }

    /// <summary>
    /// 无结果时提示
    /// </summary>
    public object? NoResultMessage
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

    //ContentPresenter? contentPresenter;

    //protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    //{
    //    base.OnApplyTemplate(e);
    //    contentPresenter = e.NameScope.Find<ContentPresenter>("ContentPresenter");
    //}

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        //if (e.Property == IsLoadingProperty || e.Property == IsShowNoResultTextProperty)
        //{
        //    if (!IsLoading && !IsShowNoResultText && contentPresenter != null && contentPresenter.Content is ItemsPresenter itemsPresenter)
        //    {
        //        itemsPresenter.InvalidateMeasure();
        //        itemsPresenter.InvalidateArrange();
        //        itemsPresenter.UpdateLayout();
        //    }
        //}
        //if (e.Property == ProgressValueProperty)
        //{
        //    if (e.NewValue is double value)
        //    {
        //        if (value > 0)
        //        {
        //            IsIndeterminate = false;
        //            IsLoading = true;
        //        }
        //        else if (value == Maximum)
        //        {
        //            IsLoading = false;
        //        }
        //    }
        //}
    }
}
