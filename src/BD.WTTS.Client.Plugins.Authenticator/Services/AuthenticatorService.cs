using AppResources = BD.WTTS.Client.Resources.Strings;

using Avalonia.Media.Imaging;
using BD.WTTS.Client.Resources;
using WinAuth;

namespace BD.WTTS.Services;

public sealed partial class AuthenticatorService
{
    static IAccountPlatformAuthenticatorRepository repository = Ioc.Get<IAccountPlatformAuthenticatorRepository>();

    public static async Task ShowCaptchaUrl(string? url)
    {
        if (string.IsNullOrEmpty(url))
            Toast.Show(ToastIcon.Warning, AppResources.Warning_CodeNullPleaseLogin);
        else
        {
            if (await Browser2.OpenAsync(url)) return;
            await Clipboard2.SetTextAsync(url);
            Toast.Show(ToastIcon.Success, Strings.CopyToClipboard);
        }
    }

    public async Task<Bitmap> GetUrlImage(string url)
    {
        using var client = new HttpClient();

        var imageBytes = await client.GetByteArrayAsync(
            new Uri(url));
        using var stream = new MemoryStream(imageBytes);

        return new Bitmap(stream);
    }

    public static async Task AddOrUpdateSaveAuthenticatorsAsync(IAuthenticatorDTO authenticatorDto, string? password, bool isLocal)
    {
        //var isLocal = await repository.HasLocalAsync();
        // var sourceList = await GetAllSourceAuthenticatorAsync();
        // if (sourceList.Length >= IAccountPlatformAuthenticatorRepository.MaxValue)
        // {
        //     if (sourceList.FirstOrDefault(i => i.Id == authenticatorDto.Id) == null)
        //     {
        //         Toast.Show(ToastIcon.Error, "操作失败：超出令牌最大数量限制");
        //         return false;
        //     }
        // }s
        // if (await repository.Exists(sourceList, authenticatorDto, isLocal, password))
        // {
        //     Toast.Show(ToastIcon.Error, "操作失败：令牌已存在重复数据");
        //     return false;
        // }
        await repository.InsertOrUpdateAsync(authenticatorDto, isLocal, password);
    }

    // public static async Task<bool> AddOrUpdateSaveAndSyncAuthenticatorAsync(string? password,
    //     params IAuthenticatorDTO[] authenticatorDto)
    // {
    //     foreach (var auth in authenticatorDto)
    //     {
    //         await AddOrUpdateSaveAuthenticatorsAsync(auth, password);
    //     }
    //     
    //     var pushItems = authenticatorDto.Select(item => new UserAuthenticatorPushItem()
    //     {
    //         Id = item.ServerId,
    //         TokenType = item.Platform == AuthenticatorPlatform.HOTP
    //             ? UserAuthenticatorTokenType.HOTP
    //             : UserAuthenticatorTokenType.TOTP,
    //         Name = item.Name,
    //         Order = item.Index,
    //         Token = MemoryPackSerializer.Serialize(item.ToExport()),
    //     }).ToArray();
    //     
    //     return true;
    // }

    public static async Task<AccountPlatformAuthenticator[]> GetAllSourceAuthenticatorAsync()
    {
        return await repository.GetAllSourceAsync();
    }

    public static async Task<List<IAuthenticatorDTO>> GetAllAuthenticatorsAsync(AccountPlatformAuthenticator[] source,
        string? password = null)
    {
        return await repository.ConvertToListAsync(source, password);
    }

    public static async Task DeleteAllAuthenticatorsAsync()
    {
        var list = await repository.GetAllSourceAsync();
        foreach (var item in list)
            await repository.DeleteAsync(item.Id);
    }

    public static async void DeleteAuth(IAuthenticatorDTO authenticatorDto)
    {
        if (authenticatorDto.ServerId.HasValue)
            await repository.DeleteAsync(authenticatorDto.ServerId.Value);
        await repository.DeleteAsync(authenticatorDto.Id);
    }

    public static async Task SaveEditAuthNameAsync(IAuthenticatorDTO authenticatorDto, string newName)
    {
        var isLocal = await repository.HasLocalAsync();
        await repository.RenameAsync(authenticatorDto.Id, newName, isLocal);
    }

    public static async Task<(IAccountPlatformAuthenticatorRepository.ImportResultCode resultCode, IReadOnlyList<IAuthenticatorDTO> result, int sourcesCount)>
        ImportAsync(string? exportPassword, byte[] data)
    {
        return await repository.ImportAsync(exportPassword, data);
    }

    public static (bool haslocal, bool haspassword) HasEncrypt(AccountPlatformAuthenticator[] sourceData)
    {
        var hasLocal = repository.HasLocal(sourceData);

        var hasPassword = repository.HasSecondaryPassword(sourceData);

        return (hasLocal, hasPassword);
    }

    // public static async Task<bool> HasPassword()
    // {
    //     return await repository.HasSecondaryPasswordAsync();
    // }
    //
    // public static async Task<bool> HasLocal()
    // {
    //     return await repository.HasLocalAsync();
    // }

    public static async Task<bool> ValidatePassword(AccountPlatformAuthenticator sourceData, string password)
    {
        return (await repository.ConvertToListAsync(new[] { sourceData }, password)).Any_Nullable();
    }

    public static async Task<bool> SwitchEncryptionAuthenticators(bool hasLocal, IEnumerable<IAuthenticatorDTO>? auths,
        string? password = null
    )
    {
        try
        {
            await repository.SwitchEncryptionModeAsync(hasLocal, password, auths);
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(nameof(AuthenticatorService), ex, nameof(SwitchEncryptionAuthenticators));
            return false;
        }
    }

    public static async Task ExportAsync(Stream stream, bool isLocal,
        IEnumerable<IAuthenticatorDTO> items, string? password = null)
    {
        await repository.ExportAsync(stream, isLocal, password, items);
    }

    public static IAuthenticatorDTO ConvertToAuthenticatorDto(
        UserAuthenticatorResponse authenticatorResponse)
    {
        var exportDto = MemoryPackSerializer.Deserialize<AuthenticatorExportDTO>(authenticatorResponse.Token);
        exportDto.ThrowIsNull();
        var valueDto = ConvertToAuthenticatorValueDto(exportDto);
        AuthenticatorDTO dto = new AuthenticatorDTO()
        {
            ServerId = authenticatorResponse.Id,
            Value = valueDto,
            Name = exportDto.Name,
            Index = (int)authenticatorResponse.Order,
            //LastUpdate = DateTimeOffset.Now,
        };
        return dto;
    }

    static IAuthenticatorValueDTO? ConvertToAuthenticatorValueDto(AuthenticatorExportDTO authenticatorExportDto)
    {
        IAuthenticatorValueDTO? valueDto = null;

        switch (authenticatorExportDto.Platform)
        {
            case AuthenticatorPlatform.Steam:
                valueDto = new SteamAuthenticator()
                {
                    DeviceId = authenticatorExportDto.DeviceId,
                    SteamData = authenticatorExportDto.SteamData,
                };
                break;
            case AuthenticatorPlatform.Microsoft:
                valueDto = new MicrosoftAuthenticator();
                break;
            case AuthenticatorPlatform.BattleNet:
                valueDto = new BattleNetAuthenticator() { Serial = authenticatorExportDto.Serial };
                break;
            case AuthenticatorPlatform.Google:
                valueDto = new GoogleAuthenticator();
                break;
            case AuthenticatorPlatform.HOTP:
                valueDto = new HOTPAuthenticator() { Counter = authenticatorExportDto.Counter };
                break;
        }
        if (valueDto == null) return null;

        valueDto.Issuer = authenticatorExportDto.Issuer;
        valueDto.HMACType = authenticatorExportDto.HMACType;
        valueDto.SecretKey = authenticatorExportDto.SecretKey;
        valueDto.Period = authenticatorExportDto.Period;
        if (valueDto.Period == 0) valueDto.Period = 30;
        valueDto.CodeDigits = authenticatorExportDto.CodeDigits;

        return valueDto;
    }

    public static async Task<IAuthenticatorValueDTO?> GeneralAuthenticatorImport(AuthenticatorPlatform platform,
        string secretCode)
    {

        var privateKey = await DecodePrivateKey(secretCode);

        if (string.IsNullOrEmpty(privateKey)) return null;

        switch (platform)
        {
            case AuthenticatorPlatform.Microsoft:
                try
                {
                    var auth = new MicrosoftAuthenticator();
                    auth.Enroll(privateKey);

                    if (auth.ServerTimeDiff == 0L)
                        Toast.Show(ToastIcon.Error, AppResources.Error_CannotConnectTokenVerificationServer);

                    return auth;
                }
                catch (Exception ex)
                {
                    Log.Error(nameof(AuthenticatorService), ex, nameof(GeneralAuthenticatorImport));
                    return null;
                }

                break;
            case AuthenticatorPlatform.Google:
                try
                {
                    var auth = new GoogleAuthenticator();
                    auth.Enroll(privateKey);

                    //string key = WinAuth.Base32.GetInstance().Encode(auth.SecretKey!);
                    //var text1 = Regex.Replace(key, ".{3}", "$0 ").Trim();
                    //var code = auth.CurrentCode;

                    if (auth.ServerTimeDiff == 0L)
                        Toast.Show(ToastIcon.Error, AppResources.Error_CannotConnectGoogleServer);

                    return auth;
                }
                catch (Exception ex)
                {
                    Log.Error(nameof(AuthenticatorService), ex, nameof(GeneralAuthenticatorImport));
                    return null;
                }
            case AuthenticatorPlatform.HOTP:
                try
                {
                    var auth = new HOTPAuthenticator();
                    auth.Enroll(privateKey);

                    if (auth.ServerTimeDiff == 0L)
                        Toast.Show(ToastIcon.Error, AppResources.Error_CannotConnectTokenVerificationServer);

                    return auth;
                }
                catch (Exception ex)
                {
                    Log.Error(nameof(AuthenticatorService), ex, nameof(GeneralAuthenticatorImport));
                    return null;
                }
                break;
        }
        return null;
    }

    //待完善
    public static async Task<string?> DecodePrivateKey(string secretCode)
    {
        if (SecretCodeHttpRegex().Match(secretCode) is { Success: true })
        {
            //url图片二维码解码
            HttpClientHandler handler = new HttpClientHandler
            {
                AllowAutoRedirect = true,
                MaxAutomaticRedirections = 1000,
            };
            using HttpClient httpClient = new HttpClient(handler);
            httpClient.DefaultRequestHeaders.Add("User-Agent", UserAgent.Default);
            httpClient.Timeout = new TimeSpan(0, 0, 20);
            try
            {
                var response = await httpClient.GetAsync(secretCode);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    //response.Content.Headers.ContentType.;
#if WINDOWS
                    using (System.Drawing.Bitmap bitmap = (System.Drawing.Bitmap)System.Drawing.Image.FromStream(await response.Content.ReadAsStreamAsync()))
                    {
                        //二维码解析
                        // IBarcodeReader reader = new BarcodeReader();
                        // var result = reader.Decode(bitmap);
                        // if (result != null)
                        // {
                        //     privatekey = HttpUtility.UrlDecode(result.Text);
                        // }
                    }
#endif
                }

            }
            catch (Exception e)
            {
                Toast.Show(ToastIcon.Error, AppResources.Error_ScanQRCodeFailed_.Format(e.Message));
                Log.Error(nameof(AuthenticatorService), e, nameof(DecodePrivateKey));
            }

        }
        else if (SecretCodeDataImageRegex().Match(secretCode) is { Success: true } dataImageMatch)
        {
            var imageData = Convert.FromBase64String(dataImageMatch.Groups[2].Value);
            using (var ms = new MemoryStream(imageData))
            {
#if WINDOWS
                using (var bitmap = (System.Drawing.Bitmap)System.Drawing.Image.FromStream(ms))
                {
                    //二维码解析
                    
                }
#endif
            }
        }
        // else if (SecretCodeOptAuthRegex().Match(secretCode) is { Success: true } optMatch)
        // {
        //     var authType = optMatch.Groups[1].Value;
        //     var label = optMatch.Groups[2].Value;
        //     var p = label.IndexOf(":", StringComparison.Ordinal);
        //     string? issuer;
        //     string? serial;
        //     if (p != -1)
        //     {
        //         issuer = label[..p];
        //         label = label[(p + 1)..];
        //     }
        //
        //     var qs = HttpUtility.ParseQueryString(optMatch.Groups[3].Value);
        //     secretCode = qs["secret"] ?? secretCode;
        //     int queryDigits;
        //     if (int.TryParse(qs["digits"], out queryDigits) && queryDigits != 0)
        //     {
        //         
        //     }
        //     switch (authType)
        //     {
        //         case "totp":
        //             break;
        //         case "hotp":
        //             break;
        //         default:
        //             return null;
        //     }
        // }
        else
        {
            var privateKey = SecretHexCodeAuthRegex().Replace(secretCode, "");
            return privateKey.Length < 1 ? null : privateKey;
        }

        return null;
    }

    [GeneratedRegex("https?://.*")]
    private static partial Regex SecretCodeHttpRegex();

    [GeneratedRegex(@"data:image/([^;]+);base64,(.*)", RegexOptions.IgnoreCase)]
    private static partial Regex SecretCodeDataImageRegex();

    [GeneratedRegex(@"otpauth://([^/]+)/([^?]+)\?(.*)", RegexOptions.IgnoreCase)]
    private static partial Regex SecretCodeOptAuthRegex();

    [GeneratedRegex(@"[^0-9a-z]", RegexOptions.IgnoreCase)]
    private static partial Regex SecretHexCodeAuthRegex();
}
