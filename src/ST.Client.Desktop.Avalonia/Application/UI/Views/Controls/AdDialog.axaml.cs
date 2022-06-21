using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace System.Application.UI.Views.Controls
{
    public partial class AdDialog : UserControl
    {
        private Button? closeButton;

        public AdDialog()
        {
            InitializeComponent();

            closeButton = this.FindControl<Button>("CloseAdBtn");

            if (closeButton != null)
            {
                closeButton.Click += (s, e) =>
                {
                    this.IsVisible = false;
                };
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
