using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BD.WTTS.Services;

public sealed class AuthenticatorService
{

    static IAccountPlatformAuthenticatorRepository repository = Ioc.Get<IAccountPlatformAuthenticatorRepository>();

    public static async void AddOrUpdateSaveAuthenticatorsAsync(IAuthenticatorDTO authenticatorDTO, bool isLocal, string? password)
    {
        await repository.InsertOrUpdateAsync(authenticatorDTO, isLocal, password);
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
        {
            await repository.DeleteAsync(item.Id);
        }
    }

    public static async void DeleteAuth(IAuthenticatorDTO authenticatorDto)
    {
        if (authenticatorDto.ServerId.HasValue)
        {
            await repository.DeleteAsync(authenticatorDto.ServerId.Value);
        }
        await repository.DeleteAsync(authenticatorDto.Id);
    }

    public static async Task SaveEditAuthNameAsync(IAuthenticatorDTO authenticatorDto, string newname)
    {
        var isLocal = await repository.HasLocalAsync();
        await repository.RenameAsync(authenticatorDto.Id, newname, isLocal);
    }

    public static async Task<(IAccountPlatformAuthenticatorRepository.ImportResultCode resultCode, IReadOnlyList<IAuthenticatorDTO> result, int sourcesCount)> 
        ImportAsync(string? exportPassword, byte[] data)
    {
        return await repository.ImportAsync(exportPassword, data);
    }

    public static (bool haslocal, bool haspassword) HasEncrypt(AccountPlatformAuthenticator[] sourceData)
    {
        var haslocal = repository.HasLocal(sourceData);

        var haspassword = repository.HasSecondaryPassword(sourceData);

        return (haslocal, haspassword);
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

    public static async Task ExportAsync(Stream stream, bool islocal,
        IEnumerable<IAuthenticatorDTO> items, string? password = null)
    {
        await repository.ExportAsync(stream, islocal, password, items);
    }
}
