using Avalonia.Controls;
using ReactiveUI.Avalonia.Reactive;
using BD.SteamClient.Enums.SteamGridDB;
using BD.SteamClient.Models.SteamGridDB;

namespace BD.WTTS.UI.Views.Pages;

public partial class EditAppInfoPage : ReactiveUserControl<EditAppInfoPageViewModel>
{
    SteamGridItemType gridItemType;

    public EditAppInfoPage()
    {
        InitializeComponent();
    }

    protected override Avalonia.Size MeasureOverride(Avalonia.Size availableSize)
    {
        var size = new Avalonia.Size(Math.Min(560, availableSize.Width), Math.Min(520, availableSize.Height));
        base.MeasureOverride(size);
        return size;
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
        }
    }

    private async void SteamGridDBItem_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
    {
        if (ViewModel is { } vm && sender is Control { DataContext: SteamGridItem gridItem })
        {
            SteamGridDBContent.IsVisible = false;
            MediaContent.IsVisible = true;
            await vm.ApplyCustomImageToApp(gridItemType, gridItem);
        }
    }
}
