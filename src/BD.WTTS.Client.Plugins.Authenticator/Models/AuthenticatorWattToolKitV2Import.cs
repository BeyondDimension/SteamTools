namespace BD.WTTS.Models;

public class AuthenticatorWattToolKitV2Import : AuthenticatorFileImportBase
{
    public override string Name => "WattToolKitV2 导入";

    public override string Description => "通过 WattToolKitV2 工具箱导出的文件，导入令牌";
    
    public sealed override ICommand AuthenticatorImportCommand { get; set; }

    protected override string FileExtension => FileEx.MPO;

    public AuthenticatorWattToolKitV2Import(string? password = null)
    {
        AuthenticatorImportCommand = ReactiveCommand.Create(async () =>
        {
            await ImportFromWattToolKitV2(password: password);
        });
    }
    
    async Task ImportFromWattToolKitV2(string? exportPassword = null, string? password = null)
    {
        var filePath = await SelectFolderPath();
        var metadata = await IOPath.TryReadAllBytesAsync(filePath);
        if (!metadata.success || metadata.byteArray == null) return;
        var result = await AuthenticatorService.ImportAsync(exportPassword, metadata.byteArray);
        switch (result.resultCode)
        {
            case IAccountPlatformAuthenticatorRepository.ImportResultCode.Success
                or IAccountPlatformAuthenticatorRepository.ImportResultCode.PartSuccess:
                foreach (var item in result.result)
                {
                    await AuthenticatorService.AddOrUpdateSaveAuthenticatorsAsync(item, password);
                }

                Toast.Show(ToastIcon.Success, result.resultCode == IAccountPlatformAuthenticatorRepository.ImportResultCode.Success
                    ? Strings.LocalAuth_AddAuthSuccess
                    : Strings.LocalAuth_AddAuth_PartSuccess);
                break;
            case IAccountPlatformAuthenticatorRepository.ImportResultCode.SecondaryPasswordFail:
                Toast.Show(ToastIcon.Warning, Strings.LocalAuth_ProtectionAuth_PasswordErrorTip);
                var textViewModel =
                    new TextBoxWindowViewModel { InputType = TextBoxWindowViewModel.TextBoxInputType.Password };
                if (await IWindowManager.Instance.ShowTaskDialogAsync(textViewModel, "请输入此令牌导出时设置的密码", isDialog: false,
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