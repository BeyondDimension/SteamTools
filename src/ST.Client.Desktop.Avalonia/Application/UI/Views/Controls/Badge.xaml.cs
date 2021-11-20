using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Metadata;
using System.Collections;

namespace System.Application.UI.Views.Controls
{
    public class Badge : ContentControl
    {
        public static readonly StyledProperty<string> TextProperty =
            AvaloniaProperty.Register<Badge, string>(nameof(Text), string.Empty);

        public static readonly StyledProperty<Control> IconProperty =
            AvaloniaProperty.Register<Badge, Control>(nameof(Icon));

        public string Text
        {
            get => GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public Control Icon
        {
            get => GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }
    }
}
