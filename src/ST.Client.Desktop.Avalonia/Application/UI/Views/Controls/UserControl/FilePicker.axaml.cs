using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Metadata;

namespace System.Application.UI.Views.Controls
{
    public partial class FilePicker : UserControl
    {
        ///// <summary>
        ///// Defines the <see cref="Content"/> property
        ///// </summary>
        //public static readonly StyledProperty<object> ContentProperty =
        //    AvaloniaProperty.Register<Flyout, object>(nameof(Content));

        ///// <summary>
        ///// Gets or sets the content to display in this flyout
        ///// </summary>
        //[Content]
        //public object Content
        //{
        //    get => GetValue(ContentProperty);
        //    set => SetValue(ContentProperty, value);
        //}

        public FilePicker()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
