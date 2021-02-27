using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.Application.UI.ViewModels;
using System.ComponentModel;

namespace System.Application.UI.Views
{
    public class MainWindow : FluentWindow
    {
        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        protected override void OnClosing(CancelEventArgs e) 
        {
            e.Cancel = true;
            this.Hide();
            base.OnClosed(e);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}