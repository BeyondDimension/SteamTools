using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.Application.UI.Resx;
using System.Application.UI.Views.Controls;

namespace System.Application.UI.Views.Pages
{
    public class About_SoftwarePage : UserControl
    {
        public About_SoftwarePage()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}