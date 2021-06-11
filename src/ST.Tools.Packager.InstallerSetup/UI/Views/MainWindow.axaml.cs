using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.Windows;

namespace System.Application.UI.Views
{
    public class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools2();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}