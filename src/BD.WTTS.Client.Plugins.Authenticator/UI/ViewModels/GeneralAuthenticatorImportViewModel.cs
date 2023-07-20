namespace BD.WTTS.UI.ViewModels;

public partial class GeneralAuthenticatorImportViewModel : ViewModelBase
{
    string? _password;
    bool _isLocal;

    IAuthenticatorValueDTO? _importAuthenticatorValueDto;
    
    public GeneralAuthenticatorImportViewModel()
    {
        
    }

    public GeneralAuthenticatorImportViewModel(string? password, bool isLocal)
    {
        _password = password;
        _isLocal = isLocal;
    }
    
    public async Task GenerateCode()
    {
        if (string.IsNullOrEmpty(SecretCode))
        {
            Toast.Show(ToastIcon.Info, "请输入「导入文本」。");
            return;
        }
        switch (ImportAuthenticatorType)
        {
            case CanImportAuthenticatorType.谷歌令牌:
                _importAuthenticatorValueDto =
                    await AuthenticatorService.GeneralAuthenticatorImport(AuthenticatorPlatform.Google, SecretCode);
                break;
            case CanImportAuthenticatorType.微软令牌:
                _importAuthenticatorValueDto =
                    await AuthenticatorService.GeneralAuthenticatorImport(AuthenticatorPlatform.Microsoft, SecretCode);
                break;
            case CanImportAuthenticatorType.其他令牌:
                _importAuthenticatorValueDto =
                    await AuthenticatorService.GeneralAuthenticatorImport(AuthenticatorPlatform.Google, SecretCode);
                break;
        }

        if (_importAuthenticatorValueDto != null)
            CurrentCode = _importAuthenticatorValueDto.CurrentCode;
    }

    public async Task Import()
    {
        if (_importAuthenticatorValueDto == null)
        {
            Toast.Show(ToastIcon.Info, "请先验证令牌 Code 正确后，再点击导入");
            return;
        }

        if (string.IsNullOrEmpty(AuthenticatorName))
        {
            Toast.Show(ToastIcon.Warning, "请输入令牌名称");
            return;
        }

        var iAuthenticatorDtoDto = new AuthenticatorDTO()
        {
            Name = $"{ImportAuthenticatorType.ToString().Replace("令牌", "")}({AuthenticatorName})",
            Value = _importAuthenticatorValueDto,
            Created = DateTimeOffset.Now,
        };
        await AuthenticatorService.AddOrUpdateSaveAuthenticatorsAsync(iAuthenticatorDtoDto, _password, _isLocal);
        await IWindowManager.Instance.ShowTaskDialogAsync(
            new MessageBoxWindowViewModel { Content = $"{ImportAuthenticatorType}导入成功" });
    }
}