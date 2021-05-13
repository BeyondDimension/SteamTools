using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace System.Application.UI.Views.Windows
{
    public partial class BindPhoneNumberWindow : FluentWindow
    {
        public BindPhoneNumberWindow()
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
