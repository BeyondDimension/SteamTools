using BD.WTTS.Enums;
using System.Diagnostics.Metrics;
using WinAuth;

namespace BD.WTTS.UI.ViewModels;

public partial class AuthenticatorGeneralImportPageViewModel
{
    public string Name => Strings.LocalAuth_Import.Format(Platform.ToString());

    IAuthenticatorValueDTO? importAuthenticatorValueDto;

    public AuthenticatorPlatform Platform { get; }

    public AuthenticatorGeneralImportPageViewModel()
    {
        Platform = AuthenticatorPlatform.HOTP;
    }

    public AuthenticatorGeneralImportPageViewModel(AuthenticatorPlatform platform)
    {
        Platform = platform;
    }

    public async Task GenerateCode()
    {
        if (string.IsNullOrEmpty(SecretCode))
        {
            Toast.Show(ToastIcon.Info, Strings.Info_PleaseEnterImportText);
            return;
        }

        try
        {
            var privateKey = await AuthenticatorHelper.DecodePrivateKey(SecretCode);

            if (string.IsNullOrEmpty(privateKey))
            {
                Toast.Show(ToastIcon.Error, Strings.LocalAuth_Import_DecodePrivateKeyError.Format(Platform.ToString()));
                return;
            }

            switch (Platform)
            {
                case AuthenticatorPlatform.Google:
                    {
                        var auth = new GoogleAuthenticator();
                        auth.Enroll(privateKey);

                        if (auth.ServerTimeDiff == 0L)
                        {
                            // 可以强行添加，但无法保证令牌准确性。
                            Toast.Show(ToastIcon.Error, Strings.Error_CannotConnectGoogleServer);
                            return;
                        }
                        importAuthenticatorValueDto = auth;
                        break;
                    }
                case AuthenticatorPlatform.Microsoft:
                    {
                        var auth = new MicrosoftAuthenticator();
                        auth.Enroll(privateKey);

                        if (auth.ServerTimeDiff == 0L)
                        {
                            // 可以强行添加，但无法保证令牌准确性。
                            Toast.Show(ToastIcon.Error, Strings.Error_CannotConnectGoogleServer);
                            return;
                        }
                        importAuthenticatorValueDto = auth;
                        break;
                    }
                case AuthenticatorPlatform.HOTP:
                    {

                        if (AuthType == AuthType.TOTP)
                        {
                            var auth = new GoogleAuthenticator();
                            auth.Enroll(privateKey);

                            auth.HMACType = HMACType;
                            auth.CodeDigits = CodeDigits;
                            auth.Period = Period;
                            importAuthenticatorValueDto = auth;
                        }
                        else
                        {
                            var auth = new HOTPAuthenticator();
                            long counter = 0;
                            if (CurrentCode?.Trim().Any_Nullable() == true)
                            {
                                long.TryParse(CurrentCode.Trim(), out counter);
                            }
                            auth.Enroll(privateKey, counter);

                            auth.HMACType = HMACType;
                            auth.CodeDigits = CodeDigits;
                            auth.Period = Period;
                            importAuthenticatorValueDto = auth;
                        }
                    }
                    break;
            }
            if (importAuthenticatorValueDto == null)
            {
                Toast.Show(ToastIcon.Info, "导入失败、请检查数据正确");
            }
            else
                CurrentCode = importAuthenticatorValueDto.CurrentCode;
        }
        catch (Exception ex)
        {
            ex.LogAndShowT();
        }
    }

    public async Task Import(string? password)
    {
        if (importAuthenticatorValueDto == null)
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
            Name = $"{importAuthenticatorValueDto.Platform}({AuthenticatorName})",
            Value = importAuthenticatorValueDto,
            Created = DateTimeOffset.Now,
        };
        await AuthenticatorHelper.SaveAuthenticator(iAuthenticatorDtoDto, password);
        Toast.Show(ToastIcon.Success, Strings.ModelContent_ImportSuccessful_.Format(iAuthenticatorDtoDto.Name));
    }
}