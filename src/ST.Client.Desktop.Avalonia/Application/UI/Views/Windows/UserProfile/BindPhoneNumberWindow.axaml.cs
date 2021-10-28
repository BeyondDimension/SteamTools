using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using System.Application.UI.ViewModels;

namespace System.Application.UI.Views.Windows
{
    public partial class BindPhoneNumberWindow : FluentWindow<BindPhoneNumberWindowViewModel>
    {
        readonly TextBox TbPhoneNumber;
        readonly TextBox TbSmsCode;

        public BindPhoneNumberWindow()
        {
            InitializeComponent();
            TbSmsCode = this.FindControl<TextBox>(nameof(TbSmsCode));
            TbPhoneNumber = this.FindControl<TextBox>(nameof(TbPhoneNumber));
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
                    if (DataContext is BindPhoneNumberWindowViewModel vm)
                    {
                        vm.Submit();
                    }
                }
            };
#if DEBUG
            this.AttachDevTools();
#endif
        }

        protected override void OnDataContextChanged(EventArgs e)
        {
            base.OnDataContextChanged(e);
            if (DataContext is BindPhoneNumberWindowViewModel vm)
            {
                vm.Close = Close;
                vm.TbPhoneNumberFocus = TbPhoneNumber.Focus;
                vm.TbSmsCodeFocus = TbSmsCode.Focus;
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            if (DataContext is BindPhoneNumberWindowViewModel vm)
            {
                vm.RemoveAllDelegate();
            }
            if (DataContext is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}