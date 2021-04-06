using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using System.Application.UI.ViewModels;

namespace System.Application.UI.Views.Windows
{
    public class LoginUserWindow : FluentWindow
    {
        readonly TextBox TbPhoneNumber;
        readonly TextBox TbSmsCode;

        public LoginUserWindow()
        {
            InitializeComponent();
            TbPhoneNumber = this.FindControl<TextBox>(nameof(TbPhoneNumber));
            TbSmsCode = this.FindControl<TextBox>(nameof(TbSmsCode));
            TbPhoneNumber.KeyUp += (_, e) =>
            {
                if (e.Key == Key.Return)
                {
                    TbSmsCode.Focus();
                }
            };
            TbSmsCode.KeyUp += (_, e) =>
            {
                if (e.Key == Key.Return)
                {
                    ((LoginUserWindowViewModel?)DataContext)?.LoginOrRegister();
                }
            };
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