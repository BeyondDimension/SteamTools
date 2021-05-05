using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Animation.Animators;
using Avalonia.Threading;
using ReactiveUI;

namespace System.Application.UI.Views.Controls
{
    public class ScrollingTextBlock : TextBlock
    {
        /// <summary>
        /// Defines the <see cref="TextGap"/> property.
        /// </summary>
        public static readonly StyledProperty<double> TextGapProperty =
            AvaloniaProperty.Register<ScrollingTextBlock, double>(nameof(TextGap), 30d);

        /// <summary>
        /// Defines the <see cref="MarqueeSpeed"/> property.
        /// </summary>
        public static readonly StyledProperty<double> MarqueeSpeedProperty =
            AvaloniaProperty.Register<ScrollingTextBlock, double>(nameof(MarqueeSpeed), 1d);

        /// <summary>
        /// Defines the <see cref="DelayProperty"/> property.
        /// </summary>
        public static readonly StyledProperty<TimeSpan> DelayProperty =
            AvaloniaProperty.Register<ScrollingTextBlock, TimeSpan>(nameof(Delay), TimeSpan.FromSeconds(2));

        public ScrollingTextBlock()
        {
            this.WhenAnyValue(x => x.Text)
                .Subscribe(OnTextChanged);

            if (Clock is null) Clock = new Clock();
            Clock.Subscribe(Tick);

            this.TextWrapping = TextWrapping.NoWrap;
        }

        private void OnTextChanged(string obj)
        {
            _offset = 0;
            _waiting = true;
            _waitCounter = TimeSpan.Zero;
            Dispatcher.UIThread.Post(InvalidateVisual, DispatcherPriority.Background);
        }

        /// <summary>
        /// Gets or sets the gap between animated text.
        /// </summary>
        public double TextGap
        {
            get { return GetValue(TextGapProperty); }
            set { SetValue(TextGapProperty, value); }
        }

        /// <summary>
        /// Gets or sets the speed of text scrolling.
        /// </summary>
        public double MarqueeSpeed
        {
            get { return GetValue(MarqueeSpeedProperty); }
            set { SetValue(MarqueeSpeedProperty, value); }
        }

        /// <summary>
        /// Gets or sets the delay between text animations.
        /// </summary>
        public TimeSpan Delay
        {
            get { return GetValue(DelayProperty); }
            set { SetValue(DelayProperty, value); }
        }

        private bool _isConstrained;
        private TimeSpan _oldFrameTime;
        private TimeSpan _waitCounter;

        private bool _waiting = false;
        private bool _animate = false;
        private double _offset;

        private double _textWidth;
        private double _textHeight;
        private double[] _offsets = new double[3];

        private void Tick(TimeSpan curFrameTime)
        {
            var frameDelta = curFrameTime - _oldFrameTime;
            _oldFrameTime = curFrameTime;

            if (_waiting)
            {
                _waitCounter += frameDelta;

                if (_waitCounter >= this.Delay)
                {
                    _waitCounter = TimeSpan.Zero;
                    _waiting = false;
                    Dispatcher.UIThread.Post(InvalidateVisual, DispatcherPriority.Background);
                }
            }
            else if (_animate)
            {
                _offset += this.MarqueeSpeed;

                if (_offset >= ((_textWidth + this.TextGap) * 2))
                {
                    _offset = 0;
                    _waiting = true;
                };

                Dispatcher.UIThread.Post(InvalidateVisual, DispatcherPriority.Background);
            }
        }

        public override void Render(DrawingContext context)
        {
            var background = Background;

            if (background != null)
            {
                context.FillRectangle(background, new Rect(Bounds.Size));
            }

            var padding = Padding;

            if (TextLayout != null)
            {
                _textWidth = TextLayout.Size.Width;
                _textHeight = TextLayout.Size.Height;

                var constraints = this.Bounds.Deflate(Padding);
                var constraintsWidth = constraints.Width;

                //让文字不要在宽度合适的时候滚动
                _isConstrained = _textWidth > constraintsWidth + 1;

                if (_isConstrained & !_waiting)
                {
                    _animate = true;
                    var tOffset = padding.Left - _offset;

                    _offsets[0] = tOffset;
                    _offsets[1] = tOffset + _textWidth + this.TextGap;
                    _offsets[2] = tOffset + (_textWidth + this.TextGap) * 2;

                    foreach (var offset in _offsets)
                    {
                        var nR = new Rect(offset, padding.Top, _textWidth, _textHeight);
                        var nC = new Rect(0, padding.Top, constraintsWidth, constraints.Height);

                        if (nC.Intersects(nR))
                            using (context.PushPostTransform(Matrix.CreateTranslation(offset, padding.Top)))
                                TextLayout.Draw(context, new Point(Padding.Left, Padding.Top));
                    }
                }
                else
                {
                    _animate = false;

                    using (context.PushPostTransform(Matrix.CreateTranslation(padding.Left, padding.Top)))
                        TextLayout.Draw(context, new Point(Padding.Left, Padding.Top));
                }
            }
        }
    }
}