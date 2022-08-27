#if ANDROID || __ANDROID__
using Android.Content;
using Android.Net;
#endif
using System.Application.Services;
using System.Application.Services.Implementation;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
#if NET6_0_OR_GREATER
using XEPlatform = Microsoft.Maui.ApplicationModel.Platform;
#else
using XEPlatform = Xamarin.Essentials.Platform;
#endif

namespace System.Application.Services.Implementation
{
    internal sealed class DnsAnalysisServiceImpl : IDnsAnalysisService
    {
        ///// <summary>
        ///// https://developer.android.google.cn/reference/android/net/ConnectivityManager?hl=zh-cn
        ///// </summary>
        //static ConnectivityManager ConnectivityManager
        //{
        //    get
        //    {
        //        var connectivityManager = XEPlatform.CurrentActivity.GetSystemService<ConnectivityManager>(Context.ConnectivityService);
        //        return connectivityManager;
        //    }
        //}

        //public static List<IPEndPoint> GetDnsServers()
        //{
        //    List<IPEndPoint> endPoints = new();
        //    var connectivityManager = ConnectivityManager;

        //    var activeConnection = connectivityManager.ActiveNetwork;
        //    var linkProperties = connectivityManager.GetLinkProperties(activeConnection);

        //    if (linkProperties != null)
        //    {
        //        foreach (var currentAddress in linkProperties.DnsServers)
        //        {
        //            var endPoint = new IPEndPoint(IPAddress.Parse(currentAddress.HostAddress), 53);
        //            endPoints.Add(endPoint);
        //        }
        //    }

        //    return endPoints;
        //}

        public int AnalysisHostnameTime(string url)
        {
            throw new NotImplementedException();
        }
    }
}

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    partial class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDnsAnalysisService(this IServiceCollection services)
        {
            services.AddSingleton<IDnsAnalysisService, DnsAnalysisServiceImpl>();
            return services;
        }
    }
}