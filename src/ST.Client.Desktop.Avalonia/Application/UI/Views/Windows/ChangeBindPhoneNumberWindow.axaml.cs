using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using System.Application.UI.Resx;
using System.Application.UI.ViewModels;
using System.ComponentModel;
using System.Windows;

namespace System.Application.UI.Views.Windows
{
    public class ChangeBindPhoneNumberWindow : FluentWindow<ChangeBindPhoneNumberWindowViewModel>
    {
        readonly TextBox TbPhoneNumber;
        readonly TextBox TbSmsCodeValidation;
        readonly TextBox TbSmsCodeNew;

        public ChangeBindPhoneNumberWindow() : base()
        {
            InitializeComponent();
            TbSmsCodeValidation = this.FindControl<TextBox>(nameof(TbSmsCodeValidation));
            TbPhoneNumber = this.FindControl<TextBox>(nameof(TbPhoneNumber));
            TbSmsCodeNew = this.FindControl<TextBox>(nameof(TbSmsCodeNew));
            TbSmsCodeValidation.KeyUp += (_, e) =>
            {
                if (e.Key == Key.Return)
                {
                    TbPhoneNumber.Focus();
                }
            };
            void Submit(object? _, KeyEventArgs e)
            {
                if (e.Key == Key.Return)
                {
                    if (DataContext is ChangeBindPhoneNumberWindowViewModel vm)
                    {
                        if (e.Source == TbPhoneNumber && vm.CurrentStepIsNew)
                        {
                            TbSmsCodeNew.Focus();
                            return;
                        }
                        vm.Submit();
                    }
                }
            }
            TbPhoneNumber.KeyUp += Submit;
            TbSmsCodeNew.KeyUp += Submit;
#if DEBUG
            this.AttachDevTools();
#endif
        }

        protected override void OnDataContextChanged(EventArgs e)
        {
            base.OnDataContextChanged(e);
            if (DataContext is ChangeBindPhoneNumberWindowViewModel vm)
            {
                vm.Close = Close;
                vm.TbSmsCodeFocusValidation = TbSmsCodeValidation.Focus;
                vm.TbSmsCodeFocusNew = TbSmsCodeNew.Focus;
            }
        }

        protected override async void OnClosing(CancelEventArgs e)
        {
            if (DataContext is ChangeBindPhoneNumberWindowViewModel vm && vm.CurrentStepIsNew && !vm.IsComplete)
            {
                // 进入第二步时，如果关闭窗口，则需二次确认
                e.Cancel = true;
                var r = await MessageBoxCompat.ShowAsync(AppResources.UnsavedEditingWillBeDiscarded, AppResources.Warning, MessageBoxButtonCompat.OKCancel);
                if (r == MessageBoxResultCompat.OK)
                {
                    vm.IsComplete = true;
                    Close();
                }
            }

            base.OnClosing(e);
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            if (DataContext is ChangeBindPhoneNumberWindowViewModel vm)
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