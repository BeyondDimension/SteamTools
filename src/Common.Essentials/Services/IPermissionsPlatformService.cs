namespace System.Application.Services;

public interface IPermissionsPlatformService
{
    static IPermissionsPlatformService Instance => DI.Get<IPermissionsPlatformService>();

    /// <summary>
    /// 检查并申请一组权限
    /// </summary>
    /// <param name="permission"></param>
    /// <returns></returns>
    Task<PermissionStatus> CheckAndRequestAsync(object permission);

    /// <summary>
    /// 检查并申请一组权限
    /// </summary>
    /// <typeparam name="TPermission"></typeparam>
    /// <returns></returns>
    Task<PermissionStatus> CheckAndRequestAsync<TPermission>() where TPermission : notnull
    {
        TPermission permission;
        var typePermission = typeof(TPermission);
        if (typePermission.IsInterface)
        {
            permission = DI.Get<TPermission>();
        }
        else
        {
            permission = Activator.CreateInstance<TPermission>();
            if (permission == null) throw new ArgumentNullException(nameof(permission));
        }
        return CheckAndRequestAsync(permission);
    }
}
