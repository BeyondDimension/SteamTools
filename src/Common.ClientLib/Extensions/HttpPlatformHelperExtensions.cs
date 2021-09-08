using System.Application;
using System.Net.Http;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace System
{
    public static class HttpPlatformHelperExtensions
    {
        /// <summary>
        /// 是否有网络链接
        /// </summary>
        /// <param name="helper"></param>
        /// <returns></returns>
        public static Task<bool> IsConnectedAsync(this IHttpPlatformHelper helper) =>
            MainThread2.InvokeOnMainThreadAsync(() =>
            {
#pragma warning disable CS0618 // 类型或成员已过时
                return helper.IsConnected;
#pragma warning restore CS0618 // 类型或成员已过时
            });
    }
}