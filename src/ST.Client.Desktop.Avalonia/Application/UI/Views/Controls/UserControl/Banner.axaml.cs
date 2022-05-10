using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace System.Application.UI.Views.Controls
{
    public partial class Banner : UserControl
    {
        /// <summary>
        /// Defines the <see cref="Url"/> property.
        /// </summary>
        public static readonly StyledProperty<string> UrlProperty =
            AvaloniaProperty.Register<Banner, string>(nameof(Url), string.Empty);

        /// <summary>
        /// Defines the <see cref="ImageSource"/> property.
        /// </summary>
        public static readonly StyledProperty<object?> ImageSourceProperty =
            AvaloniaProperty.Register<Banner, object?>(nameof(ImageSource), null);

        /// <summary>
        /// Url
        /// </summary>
        public string Url
        {
            get => GetValue(UrlProperty);
            set => SetValue(UrlProperty, value);
        }

        /// <summary>
        /// ImageSource
        /// </summary>
        public object? ImageSource
        {
            get => GetValue(ImageSourceProperty);
            set => SetValue(ImageSourceProperty, value);
        }

        public Banner()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
