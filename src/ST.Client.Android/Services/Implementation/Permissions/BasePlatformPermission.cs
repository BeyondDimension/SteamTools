using System.Threading.Tasks;
using XEPermissions = Xamarin.Essentials.Permissions;

namespace System.Application.Services.Implementation.Permissions;

public abstract class BasePlatformPermission : XEPermissions.BasePlatformPermission, IBasePermission
{
    async Task<PermissionStatus> IBasePermission.CheckStatusAsync()
    {
        return (await CheckStatusAsync()).Convert();
    }

    async Task<PermissionStatus> IBasePermission.RequestAsync()
    {
        return (await RequestAsync()).Convert();
    }
}
