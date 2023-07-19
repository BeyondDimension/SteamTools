using AppResources = BD.WTTS.Client.Resources.Strings;

namespace BD.WTTS.UI.ViewModels;

public partial class GeneralAuthenticatorImportViewModel : ViewModelBase
{
    string? _password;

    IAuthenticatorValueDTO? _importAuthenticatorValueDto;
    
    public GeneralAuthenticatorImportViewModel()
    {
        
    }

    public GeneralAuthenticatorImportViewModel(string? password)
    {
        _password = password;
    }
    
    public async Task GenerateCode()
    {
        if (string.IsNullOrEmpty(SecretCode))
        {
            Toast.Show(ToastIcon.Info, AppResources.Info_PleaseEnterImportText);
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
            Toast.Show(ToastIcon.Info, AppResources.Info_PleaseVerifyFirstAuthCode);
            return;
        }

        if (string.IsNullOrEmpty(AuthenticatorName))
        {
            Toast.Show(ToastIcon.Warning, AppResources.Warning_PleaseEnterAuthName);
            return;
        }

        var iAuthenticatorDtoDto = new AuthenticatorDTO()
        {
            Name = $"{ImportAuthenticatorType.ToString().Replace("令牌", "")}({AuthenticatorName})",
            Value = _importAuthenticatorValueDto,
            Created = DateTimeOffset.Now,
        };
        await AuthenticatorService.AddOrUpdateSaveAuthenticatorsAsync(iAuthenticatorDtoDto, _password);
        await IWindowManager.Instance.ShowTaskDialogAsync(
            new MessageBoxWindowViewModel { Content = AppResources.ModelContent_ImportSuccessful_.Format(ImportAuthenticatorType) });
    }
}