using System.Threading.Tasks;
#if NET6_0_OR_GREATER
using XEPermissions = Microsoft.Maui.ApplicationModel.Permissions;
#else
using XEPermissions = Xamarin.Essentials.Permissions;
#endif

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
