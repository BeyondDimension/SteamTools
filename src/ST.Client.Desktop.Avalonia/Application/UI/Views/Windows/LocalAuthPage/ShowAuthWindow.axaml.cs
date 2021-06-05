using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.Application.UI.ViewModels;

namespace System.Application.UI.Views.Windows
{
    public class ShowAuthWindow : FluentWindow<ShowAuthWindowViewModel>
    {
        public ShowAuthWindow() : base()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
