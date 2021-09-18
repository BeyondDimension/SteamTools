using Avalonia.Controls;
using Avalonia.Markup.Xaml;

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

            var logoPanel = this.FindControl<StackPanel>("LogoPanel");
            if (logoPanel != null)
            {
                logoPanel.PointerPressed += (_, _) => AboutAppInfoPopup.OnClick();
            }
        }
    }
}