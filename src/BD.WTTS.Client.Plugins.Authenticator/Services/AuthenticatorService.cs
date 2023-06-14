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

    public static async Task<List<IAuthenticatorDTO>> GetAllAuthenticatorsAsync()
    {
        var allSourceList = await repository.GetAllSourceAsync();
        return await repository.ConvertToListAsync(allSourceList);
    }

    public static async void DeleteAllAuthenticatorsAsync()
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
}
