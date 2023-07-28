namespace BD.WTTS.UI.ViewModels;

public partial class AuthenticatorGeneralImportPageViewModel
{
    readonly Func<IAuthenticatorDTO, Task> _saveAuth;

    readonly Func<string, Task<IAuthenticatorValueDTO?>> _createAuthenticatorValueDto;

    IAuthenticatorValueDTO? _importAuthenticatorValueDto;

    public AuthenticatorGeneralImportPageViewModel()
    {
        _saveAuth = (authenticatorDto) => Task.CompletedTask;
        _createAuthenticatorValueDto = (secretCode) => null!;
    }

    public AuthenticatorGeneralImportPageViewModel(Func<IAuthenticatorDTO, Task> saveAuthFunc,
        Func<string, Task<IAuthenticatorValueDTO?>> createAuthenticatorValueDto)
    {
        _saveAuth = saveAuthFunc;
        _createAuthenticatorValueDto = createAuthenticatorValueDto;
    }

    public async Task GenerateCode()
    {
        if (string.IsNullOrEmpty(SecretCode))
        {
            Toast.Show(ToastIcon.Info, Strings.Info_PleaseEnterImportText);
            return;
        }

        _importAuthenticatorValueDto = await _createAuthenticatorValueDto.Invoke(SecretCode);

        if (_importAuthenticatorValueDto != null)
            CurrentCode = _importAuthenticatorValueDto.CurrentCode;
    }

    public async Task Import()
    {
        if (_importAuthenticatorValueDto == null)
        {
            Toast.Show(ToastIcon.Info, Strings.Info_PleaseVerifyFirstAuthCode);
            return;
        }

        if (string.IsNullOrEmpty(AuthenticatorName))
        {
            Toast.Show(ToastIcon.Warning, Strings.Warning_PleaseEnterAuthName);
            return;
        }

        var iAuthenticatorDtoDto = new AuthenticatorDTO()
        {
            Name = $"{_importAuthenticatorValueDto.Platform}({AuthenticatorName})",
            Value = _importAuthenticatorValueDto,
            Created = DateTimeOffset.Now,
        };
        await _saveAuth.Invoke(iAuthenticatorDtoDto);
        Toast.Show(ToastIcon.Success, Strings.ModelContent_ImportSuccessful_.Format(iAuthenticatorDtoDto.Name));
    }
}