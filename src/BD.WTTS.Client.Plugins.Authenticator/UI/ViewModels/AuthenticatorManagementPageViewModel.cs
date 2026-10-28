using WinAuth;

namespace BD.WTTS.UI.ViewModels;

public sealed class AuthenticatorManagementPageViewModel : WindowViewModel
{
    public AuthenticatorManagementPageViewModel(
        AuthenticatorItemModel item,
        Func<AuthenticatorItemModel, Task> exportToken,
        Func<AuthenticatorItemModel, Task> exportMaFile)
    {
        Item = item;
        var details = ReactiveCommand.CreateFromTask(item.ShowDetailData);
        var qrCode = ReactiveCommand.Create(item.ShowQrCode);
        var export = ReactiveCommand.CreateFromTask(() => exportToken(item));
        var maFile = ReactiveCommand.CreateFromTask(async () =>
        {
            if (item.IsSteamAuthenticator)
                await exportMaFile(item);
        });
        details.AddTo(this);
        qrCode.AddTo(this);
        export.AddTo(this);
        maFile.AddTo(this);
        ShowDetailsCommand = details;
        ShowQrCodeCommand = qrCode;
        ExportTokenCommand = export;
        ExportMaFileCommand = maFile;
    }

    public AuthenticatorItemModel Item { get; }
    public string Platform => Item.AuthData.Platform.ToString();
    public string? AccountName => (Item.AuthData.Value as SteamAuthenticator)?.AccountName;
    public bool HasAccountName => !string.IsNullOrEmpty(AccountName);
    public ICommand ShowDetailsCommand { get; }
    public ICommand ShowQrCodeCommand { get; }
    public ICommand ExportTokenCommand { get; }
    public ICommand ExportMaFileCommand { get; }
}
