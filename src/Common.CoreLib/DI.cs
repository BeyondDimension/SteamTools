#if !NOT_DI
using Microsoft.Extensions.DependencyInjection;
#endif
using System.Runtime.InteropServices;
#if !NOT_XE
using Xamarin.Essentials;
#endif

namespace System
{
    /// <summary>
    /// 依赖注入服务组(DependencyInjection)
    /// </summary>
    public static partial class DI
    {
#if !NOT_DI

        static IServiceProvider? value;

        internal static bool IsInit => value != null;

        /// <summary>
        /// 初始化依赖注入服务组(通过配置服务项的方式)
        /// </summary>
        /// <param name="configureServices"></param>
        public static void Init(Action<IServiceCollection> configureServices)
        {
            var services = new ServiceCollection();
            configureServices(services);
            Init(services.BuildServiceProvider());
        }

        /// <summary>
        /// 初始化依赖注入服务组(直接赋值)
        /// </summary>
        /// <param name="serviceProvider"></param>
        public static void Init(IServiceProvider serviceProvider)
        {
            value = serviceProvider;
        }

#endif

#if !NOT_DI

        static Exception GetDIGetFailException(Type serviceType) => new($"DI.Get* fail, serviceType: {serviceType}");

        /// <summary>
        /// 获取依赖注入服务
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T Get<T>() where T : notnull
        {
            if (value == null)
            {
                throw GetDIGetFailException(typeof(T));
            }
            return value.GetRequiredService<T>();
        }

        /// <inheritdoc cref="Get{T}"/>
        public static T? Get_Nullable<T>() where T : notnull
        {
            if (value == null)
            {
                throw GetDIGetFailException(typeof(T));
            }
            return value.GetService<T>();
        }

        /// <inheritdoc cref="Get{T}"/>
        public static object Get(Type serviceType)
        {
            if (value == null)
            {
                throw GetDIGetFailException(serviceType);
            }
            return value.GetRequiredService(serviceType);
        }

        /// <inheritdoc cref="Get_Nullable{T}"/>
        public static object? Get_Nullable(Type serviceType)
        {
            if (value == null)
            {
                throw GetDIGetFailException(serviceType);
            }
            return value.GetService(serviceType);
        }

        public static IServiceScope CreateScope()
        {
            if (value == null)
            {
                throw new Exception("DI.CreateScope fail.");
            }
            return value.CreateScope();
        }

#endif
    }
}