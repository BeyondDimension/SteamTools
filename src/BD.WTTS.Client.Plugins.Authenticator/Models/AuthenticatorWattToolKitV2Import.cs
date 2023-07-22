namespace BD.WTTS.Models;

public class AuthenticatorWattToolKitV2Import : AuthenticatorFileImportBase
{
    public override string Name => Strings.LocalAuth_Import.Format(Strings.WattToolKitV2);

    public override string Description => Strings.LocalAuth_WattToolKitV2Import;
    
    public override ResIcon IconName => ResIcon.OpenFile;

    public sealed override ICommand AuthenticatorImportCommand { get; set; }

    protected override string FileExtension => FileEx.MPO;

    public AuthenticatorWattToolKitV2Import(string? password = null)
    {
        AuthenticatorImportCommand = ReactiveCommand.Create(async () =>
        {
            if (await VerifyMaxValue())
                await ImportFromWattToolKitV2(password: password);
        });
    }
    
    async Task ImportFromWattToolKitV2(string? exportPassword = null, string? password = null)
    {
        var filePath = await SelectFolderPath();
        
        if (string.IsNullOrEmpty(filePath)) return;
        
        var metadata = await IOPath.TryReadAllBytesAsync(filePath);
        if (!metadata.success || metadata.byteArray == null) return;
        var result = await AuthenticatorService.ImportAsync(exportPassword, metadata.byteArray);
        switch (result.resultCode)
        {
            case IAccountPlatformAuthenticatorRepository.ImportResultCode.Success
                or IAccountPlatformAuthenticatorRepository.ImportResultCode.PartSuccess:
                foreach (var item in result.result)
                {
                    await SaveAuthenticator(item);
                }

                Toast.Show(ToastIcon.Success, result.resultCode == IAccountPlatformAuthenticatorRepository.ImportResultCode.Success
                    ? Strings.LocalAuth_AddAuthSuccess
                    : Strings.LocalAuth_AddAuth_PartSuccess);
                break;
            case IAccountPlatformAuthenticatorRepository.ImportResultCode.SecondaryPasswordFail:
                Toast.Show(ToastIcon.Warning, Strings.LocalAuth_ProtectionAuth_PasswordErrorTip);
                var textViewModel =
                    new TextBoxWindowViewModel { InputType = TextBoxWindowViewModel.TextBoxInputType.Password };
                if (await IWindowManager.Instance.ShowTaskDialogAsync(textViewModel, Strings.ModelContent_ExportPassword, isDialog: false,
                        isCancelButton: true))
                {
                    exportPassword = textViewModel.Value;
                }

                await ImportFromWattToolKitV2(exportPassword, password);
                break;
            default:
                Toast.Show(ToastIcon.Error, Strings.LocalAuth_ExportAuth_Error.Format(result.resultCode));
                break;
        }
    }
}