using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace System.Application.UI.Views.Controls
{
    public class TitleBar : UserControl
    {
        public TitleBar()
        {
            InitializeComponent();

            if (DI.Platform == System.Platform.Apple)
            {
                var title = this.FindControl<StackPanel>("title");

                title.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center;
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
