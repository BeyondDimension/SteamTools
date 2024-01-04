using BD.WTTS.Client.Resources;

namespace BD.WTTS.UI.Views.Pages;

public partial class ScriptPage : PageBase<ScriptPageViewModel>
{
    public ScriptPage()
    {
        InitializeComponent();
        this.SetViewModel<ScriptPageViewModel>();

        //StoreButton.Click += StoreButton_Click;
    }

    //private async void StoreButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    //{
    //    var model = new ScriptStorePageViewModel();
    //    await IWindowManager.Instance.ShowTaskDialogAsync(model,
    //                   Strings.ScriptStore,
    //                   pageContent: new ScriptStorePage(),
    //                   isCancelButton: true);
    //}
}
