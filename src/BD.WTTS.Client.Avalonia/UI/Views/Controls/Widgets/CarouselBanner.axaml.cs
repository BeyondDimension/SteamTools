using Avalonia.Controls.Shapes;
using FluentAvalonia.Core;

namespace BD.WTTS.UI.Views.Controls
{
    public partial class CarouselBanner : UserControl
    {
        /// <summary>
        /// Defines the <see cref="AutoScroll"/> property.
        /// </summary>
        public static readonly StyledProperty<bool> AutoScrollProperty =
            AvaloniaProperty.Register<CarouselBanner, bool>(nameof(AutoScroll), true);

        /// <summary>
        /// Defines the <see cref="AutoScrollInterval"/> property.
        /// </summary>
        public static readonly StyledProperty<int> AutoScrollIntervalProperty =
            AvaloniaProperty.Register<CarouselBanner, int>(nameof(AutoScrollInterval), 6000);

        /// <summary>
        /// Defines the Avalonia.Controls.ItemsControl.Items property.
        /// </summary>
        public static readonly StyledProperty<IEnumerable?> ItemsSourceProperty =
            Carousel.ItemsSourceProperty.AddOwner<CarouselBanner>();

        /// <summary>
        /// Defines the Avalonia.Controls.ItemsControl.ItemTemplate property.
        /// </summary>
        public static readonly StyledProperty<IDataTemplate?> ItemTemplateProperty =
            Carousel.ItemTemplateProperty.AddOwner<CarouselBanner>();

        ///// <summary>
        ///// Defines the <see cref="GroupView"/> property.
        ///// </summary>
        //public static readonly StyledProperty<bool> GroupViewProperty =
        //    AvaloniaProperty.Register<CarouselBanner, bool>(nameof(GroupView), true);

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

        ///// <summary>
        ///// GroupView
        ///// </summary>
        //public bool GroupView
        //{
        //    get => GetValue(GroupViewProperty);
        //    set => SetValue(GroupViewProperty, value);
        //}

        private Timer? _timer;

        public CarouselBanner()
        {
            InitializeComponent();
            Left.Command = ReactiveCommand.Create(SwiperPrevious);
            Right.Command = ReactiveCommand.Create(SwiperNext);

            //CarouselControl[!Carousel.Items] = this[!Items];
            CarouselControl[!Carousel.ItemsSourceProperty] = this[!ItemsSourceProperty];
            CarouselControl[!Carousel.ItemTemplateProperty] = this[!ItemTemplateProperty];

            CarouselControl.GetObservable(Carousel.ItemCountProperty)
                    .Subscribe(_ => SwipersLoad());

            CarouselControl.GetObservable(Carousel.SelectedIndexProperty)
                    .Subscribe(_ => SwipersLoad());

            this.GetObservable(AutoScrollProperty)
                .Subscribe(x =>
                {
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
                });
        }

        //protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        //{
        //    if (change.Property == ItemsSourceProperty)
        //    {
        //        SwipersLoad();
        //    }
        //    else if (change.Property == AutoScrollProperty)
        //    {
        //        if (change.NewValue is bool x)
        //        {
        //            if (x && _timer == null)
        //            {
        //                _timer = new Timer(_ =>
        //                {
        //                    if (!this.IsPointerOver)
        //                    {
        //                        Dispatcher.UIThread.Post(SwiperNext, DispatcherPriority.Background);
        //                    }
        //                }, nameof(AutoScroll), AutoScrollInterval, AutoScrollInterval);
        //            }
        //            else
        //            {
        //                if (_timer != null)
        //                {
        //                    _timer.Dispose();
        //                    _timer = null;
        //                }
        //            }
        //        }
        //    }

        //    base.OnPropertyChanged(change);
        //}

        private void CarouselBannerIndexButton_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int index)
            {
                CarouselControl.SelectedIndex = index;
            }
        }

        private void SwipersLoad()
        {
            if (CarouselControl.ItemCount <= 0)
            {
                Swiper.ItemsSource = null;
                return;
            }
            if (CarouselControl.ItemCount == 1)
            {
                Left.IsVisible = Right.IsVisible = Swiper.IsVisible = false;
                return;
            }
            else
            {
                Left.IsVisible = Right.IsVisible = Swiper.IsVisible = true;
                var arr = new Dictionary<int, string>();
                for (var i = 0; i < CarouselControl.ItemCount; i++)
                {
                    arr.Add(i, "#ADADAD");
                }
                var index = CarouselControl.SelectedIndex < 0 ? 0 : CarouselControl.SelectedIndex;
                arr[index] = "#FFFFFF";

                Swiper.ItemsSource = arr;
            }
        }

        private void SwiperNext()
        {
            if (CarouselControl.ItemCount < 1)
                return;
            if (CarouselControl.SelectedIndex < CarouselControl.ItemCount - 1)
            {
                CarouselControl.Next();
            }
            else
            {
                CarouselControl.SelectedIndex = 0;
            }
        }

        private void SwiperPrevious()
        {
            if (CarouselControl.ItemCount < 1)
                return;
            if (CarouselControl.SelectedIndex > 0)
            {
                CarouselControl.Previous();
            }
            else
            {
                CarouselControl.SelectedIndex = CarouselControl.ItemCount - 1;
            }
        }
    }
}
