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

        protected override void OnClosing(CancelEventArgs e)
        {
            if (DataContext is UserProfileWindowViewModel vm)
            {
                e.Cancel = vm.OnClose();
            }

            base.OnClosing(e);
        }
    }
}