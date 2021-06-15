using Avalonia;
using Avalonia.Layout;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace System.Application.UI.Views.Controls
{
    public class TitleBar : UserControl
    {
        public TitleBar()
        {
            InitializeComponent();

            if (DI.Platform == Platform.Apple)
            {
                var title = this.FindControl<StackPanel>("title");
                title.HorizontalAlignment = HorizontalAlignment.Center;
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
