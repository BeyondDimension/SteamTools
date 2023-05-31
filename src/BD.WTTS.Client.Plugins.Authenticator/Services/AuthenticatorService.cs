using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BD.WTTS.Services;

public sealed class AuthenticatorService
{
    static IAccountPlatformAuthenticatorRepository repository = Ioc.Get<IAccountPlatformAuthenticatorRepository>();

    public static async void AddOrUpdateSaveAuthenticators(IAuthenticatorDTO authenticatorDTO, bool isLocal, string? password)
    {
        await repository.InsertOrUpdateAsync(authenticatorDTO, isLocal, password);
    }

    public static async Task<List<IAuthenticatorDTO>> GetAllAuthenticators()
    {
        return await repository.GetAllAsync();
    }

    public static async void DeleteAllAuthenticators()
    {
        var list = await repository.GetAllSourceAsync();
        foreach (var item in list)
        {
            await repository.DeleteAsync(item.Id);
        }
    }
}
