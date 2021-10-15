using System.Threading.Tasks;
using Xamarin.Essentials;
using static Xamarin.Essentials.Permissions;

namespace System.Application.Services
{
    /// <summary>
    /// 运行时权限
    /// </summary>
    public interface IPermissions : IService<IPermissions>
    {
        interface IPermission
        {
            BasePermission Permission { get; }
        }

        interface IPermission<TPermission> : IPermission where TPermission : IPermission<TPermission>
        {
            static TPermission Instance => DI.Get<TPermission>();
        }

        /// <summary>
        /// 获取手机号码所需权限
        /// </summary>
        interface IGetPhoneNumber : IPermission<IGetPhoneNumber> { }

        /// <summary>
        /// 检查并申请一组权限
        /// </summary>
        /// <param name="permission"></param>
        /// <returns></returns>
        Task<PermissionStatus> CheckAndRequestAsync(BasePermission permission);

        /// <inheritdoc cref="CheckAndRequestAsync(BasePermission)"/>
        Task<PermissionStatus> CheckAndRequestAsync(IPermission permission)
            => CheckAndRequestAsync(permission.Permission);

        /// <inheritdoc cref="CheckAndRequestAsync(BasePermission)"/>
        Task<PermissionStatus> CheckAndRequestAsync<TPermission>() where TPermission : BasePermission, new() => CheckAndRequestAsync(new TPermission());
    }
}