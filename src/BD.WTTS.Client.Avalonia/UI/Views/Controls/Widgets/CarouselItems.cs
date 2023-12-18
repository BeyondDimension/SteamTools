using Avalonia.Controls;
using Avalonia.Layout;
using FluentAvalonia.Core;
using System.Reactive;

namespace BD.WTTS.UI.Views.Controls;

public class CarouselItems : ItemsControl
{
    /// <summary>
    /// Defines the <see cref="AutoScroll"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> AutoScrollProperty =
        AvaloniaProperty.Register<CarouselItems, bool>(nameof(AutoScroll), true);

    /// <summary>
    /// Defines the <see cref="AutoScrollInterval"/> property.
    /// </summary>
    public static readonly StyledProperty<int> AutoScrollIntervalProperty =
        AvaloniaProperty.Register<CarouselItems, int>(nameof(AutoScrollInterval), 6000);

    /// <summary>
    /// Defines the <see cref="ItemsPerPage"/> property.
    /// </summary>
    public static readonly StyledProperty<int> ItemsPerPageProperty =
        AvaloniaProperty.Register<CarouselItems, int>(nameof(ItemsPerPage), 4);

    /// <summary>
    /// AutoScroll
    /// </summary>
    public bool AutoScroll
    {
        get => GetValue(AutoScrollProperty);
        set => SetValue(AutoScrollProperty, value);
    }

    /// <summary>
    /// 自动播放间隔时间（单位毫秒）
    /// </summary>
    public int AutoScrollInterval
    {
        get => GetValue(AutoScrollIntervalProperty);
        set => SetValue(AutoScrollIntervalProperty, value);
    }

    /// <summary>
    /// ItemsPerPage
    /// </summary>
    public int ItemsPerPage
    {
        get => GetValue(ItemsPerPageProperty);
        set => SetValue(ItemsPerPageProperty, value);
    }

    private Carousel? _carouselControl;

    public ICommand? CarouselBannerIndexCommand { get; }

    public CarouselItems()
    {
        CarouselBannerIndexCommand = ReactiveCommand.Create<int>(CarouselBannerIndex);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _carouselControl = e.NameScope.Find<Carousel>("CarouselControl");
        if (_carouselControl != null)
        {

        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        if (change.Property == ItemCountProperty)
        {
            if (_carouselControl != null)
            {
                ItemsSource?.Batch(ItemsPerPage);
            }
        }
        base.OnPropertyChanged(change);
    }

    private void CarouselBannerIndex(int index)
    {
        if (_carouselControl != null)
            _carouselControl.SelectedIndex = index;
    }
}
