using Avalonia.Controls;
using Avalonia.ReactiveUI;
using BD.SteamClient.Enums.SteamGridDB;
using FluentAvalonia.UI.Controls;

namespace BD.WTTS.UI.Views.Pages;

public partial class EditAppInfoPage : ReactiveUserControl<EditAppInfoPageViewModel>
{
    public EditAppInfoPage()
    {
        InitializeComponent();
    }

    public async void ShowGridDialog_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (SteamGridDBDialog != null && ViewModel != null)
        {
            var type = (SteamGridItemType?)(sender as Control)?.Tag ?? SteamGridItemType.Grid;
            ViewModel.RefreshSteamGridItemList(type);
            var r = await SteamGridDBDialog.ShowAsync();
            if (r == ContentDialogResult.Primary)
            {
                ViewModel.ApplyCustomImageToApp(type);
            }
        }
    }
}
