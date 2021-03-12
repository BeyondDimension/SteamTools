using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.Runtime.InteropServices;

namespace System.Application.UI.Views
{
    public class MainView : UserControl
    {
        public MainView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            var sp = this.FindControl<StackPanel>("titleButton");

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                sp.Margin = new Avalonia.Thickness(0, 0, 140, 0);
            else
                sp.Margin = new Avalonia.Thickness(0, 17, 15, 0);

        }
    }
}