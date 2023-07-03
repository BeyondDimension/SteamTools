using Avalonia.Media.Imaging;
using BD.WTTS.Client.Resources;
using WinAuth;

namespace BD.WTTS.Services;

public sealed partial class AuthenticatorService
{
    public static async Task ShowCaptchaUrl(string? url)
    {
        if (string.IsNullOrEmpty(url))
            Toast.Show("验证码URL为空，请重新登录");
        else
        {
            if (await Browser2.OpenAsync(url)) return;
            await Clipboard2.SetTextAsync(url);
            Toast.Show(Strings.CopyToClipboard);
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

    static IAccountPlatformAuthenticatorRepository repository = Ioc.Get<IAccountPlatformAuthenticatorRepository>();

    public static async void AddOrUpdateSaveAuthenticatorsAsync(IAuthenticatorDTO authenticatorDto, string? password)
    {
        var isLocal = await repository.HasLocalAsync();
        await repository.InsertOrUpdateAsync(authenticatorDto, isLocal, password);
    }

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

    //待完善
    public static async Task<IAuthenticatorValueDTO?> GeneralAuthenticatorImport(string secretCode)
    {
        var privateKey = await DecodePrivateKey(secretCode);
        
        if (string.IsNullOrEmpty(privateKey)) return null;
        
        try
        {
            var auth = new GoogleAuthenticator();
            auth.Enroll(privateKey);

            //string key = WinAuth.Base32.GetInstance().Encode(auth.SecretKey!);
            //var text1 = Regex.Replace(key, ".{3}", "$0 ").Trim();
            //var code = auth.CurrentCode;

            if (auth.ServerTimeDiff == 0L)
                Toast.Show("无法连接到Google服务器");

            return auth;
        }
        catch (Exception ex)
        {
            Log.Error(nameof(AuthenticatorService), ex, nameof(GeneralAuthenticatorImport));
            return null;
        }
    }

    //待完善
    public static async Task<string?> DecodePrivateKey(string secretCode)
    {
        if (SecretCodeHttpRegex().Match(secretCode) is { Success: true } httpMatch)
        {
            //url图片二维码解码
        }
        else if (SecretCodeDataImageRegex().Match(secretCode) is { Success: true } dataImageMatch)
        {
            var imageData = Convert.FromBase64String(dataImageMatch.Groups[2].Value);
            using (var ms = new MemoryStream(imageData))
            {
#if WINDOWS
                using (var bitmap = (System.Drawing.Bitmap)System.Drawing.Image.FromStream(ms))
                {
                    //二维码解码
                }
#endif
            }
        }
        else if (SecretCodeOptAuthRegex().Match(secretCode) is { Success: true } optMatch)
        {
            var authType = optMatch.Groups[1].Value;
            switch (authType)
            {
                case "totp":
                    break;
                case "hotp":
                    break;
                default:
                    return null;
            }
        }
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
