using System.Application.Services;
using System.Runtime.CompilerServices;

// ReSharper disable once CheckNamespace
namespace System.Application;

public static class Permissions2
{
    /// <inheritdoc cref="IPermissionsPlatformService.CheckAndRequestAsync(object)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task<PermissionStatus> CheckAndRequestAsync(object permission)
        => IPermissionsPlatformService.Instance.CheckAndRequestAsync(permission);

    /// <inheritdoc cref="IPermissionsPlatformService.CheckAndRequestAsync{TPermission}"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task<PermissionStatus> CheckAndRequestAsync<TPermission>() where TPermission : notnull
        => IPermissionsPlatformService.Instance.CheckAndRequestAsync<TPermission>();

    /// <summary>
    /// 获取手机号码所需权限
    /// </summary>
    public interface IGetPhoneNumber : IBasePermission<IGetPhoneNumber> { }
}