using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.Application.UI.Resx;
using System.Application.UI.ViewModels;
using System.ComponentModel;
using System.Windows;

namespace System.Application.UI.Views.Windows
{
    public class UserProfileWindow : FluentWindow<UserProfileWindowViewModel>
    {
        public UserProfileWindow() : base()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        protected override void OnDataContextChanged(EventArgs e)
        {
            base.OnDataContextChanged(e);
            if (DataContext is UserProfileWindowViewModel vm)
            {
                vm.Close = Close;
            }
        }

        protected override async void OnClosing(CancelEventArgs e)
        {
            if (DataContext is UserProfileWindowViewModel vm && vm.IsModify && !vm.IsComplete)
            {
                // 有修改时，如果关闭窗口，则需二次确认
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
    }
}