using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using System.Application.UI.ViewModels;

namespace System.Application.UI.Views.Windows
{
    public class TextBoxWindow : FluentWindow<TextBoxWindowViewModel>
    {
        public TextBoxWindow() : base(false)
        {
            InitializeComponent();

#if DEBUG
            this.AttachDevTools();
#endif
        }

        protected override void OnOpened(EventArgs e)
        {
            var descriptionBox = this.FindControl<TextBox>("DescriptionBox");
            var passwordBox = this.FindControl<TextBox>("PasswordBox");

            switch (ViewModel?.InputType)
            {
                case TextBoxWindowViewModel.TextBoxInputType.Password:
                    passwordBox.IsVisible = true;
                    passwordBox.PasswordChar = '*';
                    passwordBox.Classes = new Classes("revealPasswordButton");
                    break;
                case TextBoxWindowViewModel.TextBoxInputType.TextBox:
                    passwordBox.IsVisible = true;
                    passwordBox.PasswordChar = default;
                    passwordBox.Classes = new Classes();
                    break;
                case TextBoxWindowViewModel.TextBoxInputType.ReadOnlyText:
                    passwordBox.IsVisible = false;
                    break;
            }

            base.OnOpened(e);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            ExtendClientAreaChromeHints = Avalonia.Platform.ExtendClientAreaChromeHints.NoChrome;
        }
    }
}
