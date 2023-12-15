using Avalonia.Controls;
using Avalonia.Layout;
using System.Reactive;

namespace BD.WTTS.UI.Views.Controls;

public class CarouselItems : TemplatedControl
{
    /// <summary>
    /// Defines the <see cref="AutoScroll"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> AutoScrollProperty =
        AvaloniaProperty.Register<CarouselItems, bool>(nameof(AutoScroll), true);

    //
    // 摘要:
    //     Defines the Avalonia.Controls.ItemsControl.ItemCount property.
    public static readonly DirectProperty<ItemsControl, int> ItemCountProperty =
        AvaloniaProperty.RegisterDirect("ItemCount", (ItemsControl o) => o.ItemCount, null, 0);

    /// <summary>
    /// Defines the <see cref="AutoScrollInterval"/> property.
    /// </summary>
    public static readonly StyledProperty<int> AutoScrollIntervalProperty =
        AvaloniaProperty.Register<CarouselItems, int>(nameof(AutoScrollInterval), 6000);

    /// <summary>
    /// Defines the Avalonia.Controls.ItemsControl.Items property.
    /// </summary>
    public static readonly StyledProperty<IEnumerable?> ItemsSourceProperty =
        ItemsControl.ItemsSourceProperty.AddOwner<CarouselItems>();

    /// <summary>
    /// Defines the Avalonia.Controls.ItemsControl.ItemTemplate property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> ItemTemplateProperty =
        ItemsControl.ItemTemplateProperty.AddOwner<CarouselItems>();

    /// <summary>
    /// Defines the <see cref="ItemsPerPage"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> ItemsPerPageProperty =
        AvaloniaProperty.Register<CarouselItems, bool>(nameof(ItemsPerPage), true);

    /// <summary>
    ///  Gets or sets the items to display.
    /// </summary>
    public IEnumerable? ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    /// <summary>
    /// Gets or sets the data template used to display the items in the control.
    /// </summary>
    public IDataTemplate? ItemTemplate
    {
        get => GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

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
    public bool ItemsPerPage
    {
        get => GetValue(ItemsPerPageProperty);
        set => SetValue(ItemsPerPageProperty, value);
    }

    /// <summary>
    /// Gets the number of items being displayed by the Avalonia.Controls.ItemsControl.
    /// </summary>
    public int ItemCount
    {
        get
        {
            return _itemCount;
        }

        private set
        {
            if (SetAndRaise(ItemCountProperty, ref _itemCount, value))
            {

            }
        }
    }

    private Carousel? _carouselControl;
    private int _itemCount;

    public ICommand? CarouselBannerIndexCommand { get; }

    public CarouselItems()
    {
        CarouselBannerIndexCommand = ReactiveCommand.Create<int>(CarouselBannerIndex);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _carouselControl = e.NameScope.Find<Carousel>("LayoutRoot");
        if (_carouselControl != null)
        {

        }
    }

    private void CarouselBannerIndex(int index)
    {
        if (_carouselControl != null)
            _carouselControl.SelectedIndex = index;
    }
}
