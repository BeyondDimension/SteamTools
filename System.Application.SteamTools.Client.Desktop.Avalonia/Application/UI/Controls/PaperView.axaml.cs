using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace System.Application.UI.Views.Controls
{
    public class PaperView : UserControl
    {
        public PaperView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
