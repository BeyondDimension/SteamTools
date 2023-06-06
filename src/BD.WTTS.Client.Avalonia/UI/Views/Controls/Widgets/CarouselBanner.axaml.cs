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
            AvaloniaProperty.Register<CarouselBanner, int>(nameof(AutoScrollInterval), 8000);

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

        private Timer? _timer;

        public CarouselBanner()
        {
            InitializeComponent();
            Left.Click += (s, e) => SwiperPrevious();
            Right.Click += (s, e) => SwiperNext();

            //Carousel[!Carousel.Items] = this[!Items];
            Carousel[!Carousel.ItemsSourceProperty] = this[!ItemsSourceProperty];
            Carousel[!Carousel.ItemTemplateProperty] = this[!ItemTemplateProperty];

            Carousel.GetObservable(Carousel.ItemsSourceProperty)
                    .Subscribe(_ => SwipersLoad());

            Carousel.GetObservable(Carousel.SelectedIndexProperty)
                    .Subscribe(x => SwipersLoad());

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

        private void SwipersLoad()
        {
            if (Carousel.ItemCount <= 0)
                return;
            if (Carousel.ItemCount == 1)
            {
                Left.IsVisible = Right.IsVisible = Swiper.IsVisible = false;
                return;
            }
            else
            {
                Left.IsVisible = Right.IsVisible = Swiper.IsVisible = true;
                var arr = new string[Carousel.ItemCount];
                for (var i = 0; i < arr.Length; i++)
                {
                    arr[i] = "#ADADAD";
                }
                var index = Carousel.SelectedIndex < 0 ? 0 : Carousel.SelectedIndex;
                arr[index] = "#FFFFFF";

                Swiper.ItemsSource = arr;
            }
        }

        private void SwiperNext()
        {
            if (Carousel.ItemCount < 1)
                return;
            if (Carousel.SelectedIndex < Carousel.ItemCount - 1)
            {
                Carousel.Next();
            }
            else
            {
                Carousel.SelectedIndex = 0;
            }
        }

        private void SwiperPrevious()
        {
            if (Carousel.ItemCount < 1)
                return;
            if (Carousel.SelectedIndex > 0)
            {
                Carousel.Previous();
            }
            else
            {
                Carousel.SelectedIndex = Carousel.ItemCount - 1;
            }
        }
    }
}
