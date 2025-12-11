using BD.WTTS.Enums;
using BD.WTTS.Helpers;
using System.Diagnostics.Metrics;
using WinAuth;

namespace BD.WTTS.UI.ViewModels;

public partial class AuthenticatorGeneralImportPageViewModel
{
    public string Name => Strings.LocalAuth_Import.Format(Strings.HOTP);

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

    //TODO: 待完善
    public async Task<string?> DecodePrivateKey(string secretCode)
    {
        //if (AuthenticatorRegexHelper.SecretCodeHttpRegex().Match(secretCode) is { Success: true })
        //{
        //    ////url图片二维码解码
        //    //var handler = new HttpClientHandler
        //    //{
        //    //    AllowAutoRedirect = true,
        //    //    MaxAutomaticRedirections = 1000,
        //    //};
        //    //using var httpClient = new HttpClient(handler);
        //    //httpClient.DefaultRequestHeaders.Add("User-Agent", UserAgent.Default);
        //    //httpClient.Timeout = new TimeSpan(0, 0, 20);
        //    //try
        //    //{
        //    //    var response = await httpClient.GetAsync(secretCode);
        //    //    if (response.StatusCode == HttpStatusCode.OK)
        //    //    {
        //    //        response.Content.Headers.ContentType.;

        //    //        using (var bitmap = System.Drawing.Image.FromStream(await response.Content.ReadAsStreamAsync()))
        //    //        {
        //    //            //二维码解析
        //    //            //IBarcodeReader reader = new BarcodeReader();
        //    //            //var result = reader.Decode(bitmap);
        //    //            //if (result != null)
        //    //            //{
        //    //            //    privatekey = HttpUtility.UrlDecode(result.Text);
        //    //            //}
        //    //        }
        //    //    }

        //    //}
        //    //catch (Exception e)
        //    //{
        //    //    Toast.Show(ToastIcon.Error, AppResources.Error_ScanQRCodeFailed_.Format(e.Message));
        //    //    Log.Error(nameof(AuthenticatorHelper), e, nameof(DecodePrivateKey));
        //    //}

        //}
        //        else if (AuthenticatorRegexHelper.SecretCodeDataImageRegex().Match(secretCode) is { Success: true } dataImageMatch)
        //        {
        //            var imageData = Convert.FromBase64String(dataImageMatch.Groups[2].Value);
        //            using var ms = new MemoryStream(imageData);
        //#if WINDOWS
        //            using (var bitmap = (System.Drawing.Bitmap)System.Drawing.Image.FromStream(ms))
        //            {
        //                //二维码解析

        //            }
        //#endif
        //        }
        if (AuthenticatorRegexHelper.SecretCodeOptAuthRegex().Match(secretCode) is { Success: true } optMatch)
        {
            var authType = optMatch.Groups[1].Value;
            var label = optMatch.Groups[2].Value;

            switch (authType)
            {
                case "totp":
                    AuthType = AuthType.TOTP;
                    break;
                case "hotp":
                    AuthType = AuthType.HOTP;
                    break;
                default:
                    return null;
            }

            var qs = HttpUtility.ParseQueryString(optMatch.Groups[3].Value);
            var privatekey = qs["secret"];
            if (qs != null)
            {
                if (long.TryParse(qs["counter"], out var counter) == true)
                {
                    CurrentCode = counter.ToString();
                }

                var issuer = qs["issuer"];
                if (string.IsNullOrEmpty(issuer) == false)
                {
                    label = issuer + (string.IsNullOrEmpty(label) == false ? " (" + label + ")" : string.Empty);
                }
                AuthenticatorName = label;

                if (int.TryParse(qs["period"], out var period) == true && period > 0)
                {
                    Period = period;
                }

                if (int.TryParse(qs["digits"], out var digits) == true && digits > 0)
                {
                    CodeDigits = digits;
                }

                if (Enum.TryParse<HMACTypes>(qs["algorithm"], true, out var hmac) == true)
                {
                    HMACType = hmac;
                }

                return privatekey;
            }
        }
        else
        {
            var privateKey = AuthenticatorRegexHelper.SecretHexCodeAuthRegex().Replace(secretCode, "");
            return privateKey.Length < 1 ? null : privateKey;
        }

        return null;
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
            var privateKey = await DecodePrivateKey(SecretCode);

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
                            var auth = new TOTPAuthenticator();
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

        if (await AuthenticatorHelper.SaveAuthenticator(iAuthenticatorDtoDto, password))
            Toast.Show(ToastIcon.Success, Strings.ModelContent_ImportSuccessful_.Format(iAuthenticatorDtoDto.Name));
    }
}