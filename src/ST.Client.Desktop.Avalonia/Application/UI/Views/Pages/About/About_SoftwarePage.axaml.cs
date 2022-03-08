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

            var logoPanel = this.FindControl<DockPanel>("LogoPanel");
            if (logoPanel != null)
            {
                logoPanel.Tapped += (_, _) => AboutAppInfoPopup.OnClick();
            }

            var avaloniaVersion = this.FindControl<TextBlock>("AvaloniaVersionTextBlock");
            if (avaloniaVersion != null)
            {
                avaloniaVersion.Text = typeof(Avalonia.Application).Assembly.GetName().Version?.ToString();
            }
        }
    }
}