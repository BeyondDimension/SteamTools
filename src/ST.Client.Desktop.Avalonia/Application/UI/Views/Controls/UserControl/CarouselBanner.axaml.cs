using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using FluentAvalonia.Core;
using System.Linq;
using System.Threading;

namespace System.Application.UI.Views.Controls
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
            AvaloniaProperty.Register<CarouselBanner, int>(nameof(AutoScrollInterval), 5000);

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

        private Carousel _carousel;
        private Button _left;
        private Button _right;
        private ItemsRepeater _swipers;
        private Timer? _timer;

        public CarouselBanner()
        {
            InitializeComponent();
            _carousel = this.FindControl<Carousel>("carousel");
            _left = this.FindControl<Button>("left");
            _right = this.FindControl<Button>("right");
            _swipers = this.FindControl<ItemsRepeater>("swiper");
            _left.Click += (s, e) => SwiperPrevious();
            _right.Click += (s, e) => SwiperNext();

            _carousel.GetObservable(Carousel.ItemsProperty)
                    .Subscribe(_ => SwipersLoad());

            _carousel.GetObservable(Carousel.SelectedIndexProperty)
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

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void SwipersLoad()
        {
            var arr = new bool[_carousel.ItemCount];
            arr[_carousel.SelectedIndex] = true;
            _swipers.Items = arr;
        }

        private void SwiperNext()
        {
            if (_carousel.SelectedIndex < _carousel.ItemCount - 1)
            {
                _carousel.Next();
            }
            else
            {
                _carousel.SelectedIndex = 0;
            }
        }

        private void SwiperPrevious()
        {
            if (_carousel.SelectedIndex > 0)
            {
                _carousel.Previous();
            }
            else
            {
                _carousel.SelectedIndex = _carousel.ItemCount - 1;
            }
        }
    }
}
