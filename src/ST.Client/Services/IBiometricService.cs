using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Application.Services;
using System.Application.Services.Implementation;
using System.Threading.Tasks;

namespace System.Application.Services
{
    /// <summary>
    /// 生物识别，例如指纹，人脸，虹膜等
    /// </summary>
    public interface IBiometricService
    {
        static IBiometricService Instance => DI.Get<IBiometricService>();

        /// <summary>
        /// 当前设备是否支持生物识别
        /// </summary>
        ValueTask<bool> IsSupportedAsync() => new(false);
    }
}

namespace System.Application.Services.Implementation
{
    sealed class EmptyBiometricServiceImpl : IBiometricService
    {
    }
}

namespace Microsoft.Extensions.DependencyInjection
{
    public static partial class IdentityServiceCollectionExtensions
    {
        public static IServiceCollection TryAddBiometricService(this IServiceCollection services)
        {
            services.TryAddSingleton<IBiometricService, EmptyBiometricServiceImpl>();
            return services;
        }
    }
}