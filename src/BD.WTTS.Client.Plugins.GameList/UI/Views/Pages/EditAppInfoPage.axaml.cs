using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.ReactiveUI;
using Avalonia.VisualTree;
using BD.SteamClient.Enums.SteamGridDB;
using BD.SteamClient.Models.SteamGridDB;
using BD.WTTS.UI.Views.Controls;

namespace BD.WTTS.UI.Views.Pages;

public partial class EditAppInfoPage : ReactiveUserControl<EditAppInfoPageViewModel>
{
    SteamGridItemType gridItemType;

    public EditAppInfoPage()
    {
        InitializeComponent();
    }

    public void ResetGridImage_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (ViewModel != null)
        {
            gridItemType = (SteamGridItemType?)(sender as Control)?.Tag ?? SteamGridItemType.Grid;
            ViewModel.SelectGrid = null;
            ViewModel.ApplyCustomImageToApp(gridItemType);
        }
    }

    public void HideGridDialog_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        SteamGridDBContent.IsVisible = false;
        MediaContent.IsVisible = true;
    }

    public void ShowGridDialog_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (ViewModel != null)
        {
            gridItemType = (SteamGridItemType?)(sender as Control)?.Tag ?? SteamGridItemType.Grid;
            ViewModel.RefreshSteamGridItemList(gridItemType);
            SteamGridDBContent.IsVisible = true;
            MediaContent.IsVisible = false;
            //var dialog = new ContentDialog
            //{
            //    Title = Strings.SteamGridDBTitle,
            //    CloseButtonText = Strings.Cancel,
            //    DefaultButton = ContentDialogButton.Primary,
            //    PrimaryButtonText = Strings.Confirm,
            //    Content = SteamGridDBContent,
            //};
            //var r = await dialog.ShowAsync();
            //if (r == ContentDialogResult.Primary)
            //{
            //    ViewModel.ApplyCustomImageToApp(type);
            //}
        }
    }

    private void SteamGridDBItem_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
    {
        if (sender is Control c && c.DataContext is SteamGridItem gridItem)
        {
            ViewModel!.SelectGrid = gridItem;
            ViewModel.ApplyCustomImageToApp(gridItemType);
            SteamGridDBContent.IsVisible = false;
            MediaContent.IsVisible = true;
        }
    }
}
