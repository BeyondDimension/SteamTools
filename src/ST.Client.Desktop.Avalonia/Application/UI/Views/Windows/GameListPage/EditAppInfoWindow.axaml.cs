using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using FluentAvalonia.UI.Controls;
using System.Application.Models;
using System.Application.UI.ViewModels;

namespace System.Application.UI.Views.Windows
{
    public class EditAppInfoWindow : FluentWindow<EditAppInfoWindowViewModel>
    {
        private readonly ContentDialog GridDialog;

        public EditAppInfoWindow() : base()
        {
            InitializeComponent();

            GridDialog = this.FindControl<ContentDialog>("SteamGridDBDialog");

#if DEBUG
            this.AttachDevTools();
#endif
        }

        public async void ShowGridDialog_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (GridDialog != null && ViewModel != null)
            {
                var type = (SteamGridItemType?)((sender as Control)?.Tag);
                ViewModel.RefreshSteamGridItemList(type ?? SteamGridItemType.Grid);
                var r = await GridDialog.ShowAsync();
                if (r == ContentDialogResult.Primary)
                {

                }
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
