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

    Carousel? _carouselControl;
    Button? _leftButton;
    Button? _rightButton;
    ItemsControl? _swiper;
    Timer? _timer;

    public ICommand? CarouselBannerIndexCommand { get; }

    public CarouselItems()
    {
        CarouselBannerIndexCommand = ReactiveCommand.Create<int>(CarouselBannerIndex);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _carouselControl = e.NameScope.Find<Carousel>("CarouselControl");
        _leftButton = e.NameScope.Find<Button>("Left");
        _rightButton = e.NameScope.Find<Button>("Right");
        _swiper = e.NameScope.Find<Avalonia.Controls.ItemsControl>("Swiper");

        if (_leftButton != null)
        {
            _leftButton.Command = ReactiveCommand.Create(SwiperPrevious);
        }
        if (_rightButton != null)
        {
            _rightButton.Command = ReactiveCommand.Create(SwiperNext);
        }

        if (_carouselControl != null)
        {
            _carouselControl.GetObservable(Carousel.ItemCountProperty)
               .Subscribe(_ => SwipersLoad());

            _carouselControl.GetObservable(Carousel.SelectedIndexProperty)
              .Subscribe(_ => SwipersLoad());

            RefreshItemsSource();
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ItemCountProperty)
        {
            RefreshItemsSource();
        }
        else if (change.Property == ItemsPerPageProperty)
        {
            RefreshItemsSource();
        }
        else if (change.Property == AutoScrollProperty)
        {
            var x = change.GetNewValue<bool>();
            if (x && _timer == null)
            {
                _timer = new Timer(_ =>
                {
                    if (!this.IsPointerOver)
                    {
                        Dispatcher.UIThread.Post(SwiperNext, DispatcherPriority.Background);
                    }
                }, nameof(AutoScroll), AutoScrollInterval, AutoScrollInterval);
            }
            else
            {
                if (_timer != null)
                {
                    _timer.Dispose();
                    _timer = null;
                }
            }
        }
    }

    protected override void OnPointerEntered(PointerEventArgs e)
    {
        base.OnPointerEntered(e);

        if (_swiper == null || _leftButton == null || _rightButton == null)
            return;

        _leftButton.IsVisible = _rightButton.IsVisible = _swiper.IsVisible;
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);

        if (_swiper == null || _leftButton == null || _rightButton == null)
            return;

        _leftButton.IsVisible = _rightButton.IsVisible = false;
    }

    void RefreshItemsSource()
    {
        if (_carouselControl != null)
        {
            var items = ItemsSource?.Batch(ItemsPerPage);
            _carouselControl.ItemsSource = items;
        }
    }

    void CarouselBannerIndex(int index)
    {
        if (_carouselControl != null)
            _carouselControl.SelectedIndex = index;
    }

    void SwipersLoad()
    {
        if (_carouselControl == null || _swiper == null || _leftButton == null || _rightButton == null)
            return;

        if (_carouselControl.ItemCount <= 0)
        {
            _swiper.ItemsSource = null;
            return;
        }
        if (_carouselControl.ItemCount == 1)
        {
            _swiper.IsVisible = false;
            return;
        }
        else
        {
            _swiper.IsVisible = true;
            var arr = new Dictionary<int, string>();
            for (var i = 0; i < _carouselControl.ItemCount; i++)
            {
                arr.Add(i, "#ADADAD");
            }
            var index = _carouselControl.SelectedIndex < 0 ? 0 : _carouselControl.SelectedIndex;
            arr[index] = "#FFFFFF";

            _swiper.ItemsSource = arr;
        }
    }

    /// <summary>
    /// 滑动到下一个
    /// </summary>
    public void SwiperNext()
    {
        if (_carouselControl == null || _carouselControl.ItemCount < 1)
            return;

        if (_carouselControl.SelectedIndex < _carouselControl.ItemCount - 1)
        {
            _carouselControl.Next();
        }
        else
        {
            _carouselControl.SelectedIndex = 0;
        }
    }

    /// <summary>
    /// 滑动到上一个
    /// </summary>
    public void SwiperPrevious()
    {
        if (_carouselControl == null || _carouselControl.ItemCount < 1)
            return;

        if (_carouselControl.SelectedIndex > 0)
        {
            _carouselControl.Previous();
        }
        else
        {
            _carouselControl.SelectedIndex = _carouselControl.ItemCount - 1;
        }
    }
}
