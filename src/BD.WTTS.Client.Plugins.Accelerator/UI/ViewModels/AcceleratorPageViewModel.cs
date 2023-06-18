using BD.WTTS.Client.Resources;

namespace BD.WTTS.UI.ViewModels;

public sealed partial class AcceleratorPageViewModel
{
    public AcceleratorPageViewModel()
    {
        StartProxyCommand = ReactiveCommand.Create(async () =>
        {
            var result = await IWindowManager.Instance.ShowTaskDialogAsync<MessageBoxWindowViewModel>(new MessageBoxWindowViewModel(), $"Test", isCancelButton: true);
            Toast.Show(result.ToString());
        });
    }
}