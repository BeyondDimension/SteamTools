using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.Application.UI.Resx;
using System.Application.UI.ViewModels;
using System.ComponentModel;
using System.Windows;

namespace System.Application.UI.Views.Windows
{
    public class ChangeBindPhoneNumberWindow : FluentWindow
    {
        public ChangeBindPhoneNumberWindow()
        {
            InitializeComponent();
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
            }
        }

        protected override async void OnClosing(CancelEventArgs e)
        {
            if (DataContext is ChangeBindPhoneNumberWindowViewModel vm && vm.CurrentStepIsNew && !vm.IsComplete)
            {
                // 进入第二步时，如果关闭窗口，则需二次确认
                e.Cancel = true;
                var r = await MessageBoxCompat.ShowAsync(AppResources.UnsavedEditingWillBeDiscarded, AppResources.Warn, MessageBoxButtonCompat.OKCancel);
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