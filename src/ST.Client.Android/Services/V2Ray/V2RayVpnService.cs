using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using static System.Application.Services.Native.IServiceBase;
using System.Application.Services.V2Ray;
using Java.Lang.Ref;
using AndroidAppService = Android.App.Service;
using static System.Properties.ThisAssembly;
using System.Linq;
using System.Collections.Generic;
using Android.Net;
using JException = Java.Lang.Exception;
using Utils = System.Application.Services.V2Ray.V2RayUtils;
using File = Java.IO.File;
using KotlinX.Coroutines;

namespace System.Application.Services.Native
{
    partial class ProxyForegroundService
    {
        partial class VpnService : IServiceControl
        {
            ParcelFileDescriptor? mInterface;

            #region lazy

            /**
             * Unfortunately registerDefaultNetworkCallback is going to return our VPN interface: https://android.googlesource.com/platform/frameworks/base/+/dda156ab0c5d66ad82bdcf76cda07cbc0a9c8a2e
             *
             * This makes doing a requestNetwork with REQUEST necessary so that we don't get ALL possible networks that
             * satisfies default network capabilities but only THE default network. Unfortunately we need to have
             * android.permission.CHANGE_NETWORK_STATE to be able to call requestNetwork.
             *
             * Source: https://android.googlesource.com/platform/frameworks/base/+/2df4c7d/services/core/java/com/android/server/ConnectivityService.java#887
             */
            NetworkRequest? mDefaultNetworkRequest;
            NetworkRequest DefaultNetworkRequest
            {
                get
                {
                    if (mDefaultNetworkRequest == null)
                    {
                        mDefaultNetworkRequest = new NetworkRequest.Builder()
                            .AddCapability(NetCapability.Internet)!
                            .AddCapability(NetCapability.NotRestricted)!
                            .Build()!;
                    }
                    return mDefaultNetworkRequest;
                }
            }

            ConnectivityManager? mConnectivity;
            ConnectivityManager Connectivity
            {
                get
                {
                    if (mConnectivity == null)
                    {
                        mConnectivity = this.GetSystemService<ConnectivityManager>();
                    }
                    return mConnectivity;
                }
            }

            ConnectivityManager.NetworkCallback? mDefaultNetworkCallback;
            ConnectivityManager.NetworkCallback DefaultNetworkCallback
            {
                get
                {
                    if (mDefaultNetworkCallback == null)
                    {
                        mDefaultNetworkCallback = new DefaultNetworkCallback2(this);
                    }
                    return mDefaultNetworkCallback;
                }
            }

            sealed class DefaultNetworkCallback2 : ConnectivityManager.NetworkCallback
            {
                readonly VpnService thiz;

                public DefaultNetworkCallback2(VpnService thiz)
                {
                    this.thiz = thiz;
                }

                public override void OnAvailable(Network network)
                {
                    thiz.SetUnderlyingNetworks(new[] { network });
                }

                public override void OnCapabilitiesChanged(Network network, NetworkCapabilities networkCapabilities)
                {
                    // it's a good idea to refresh capabilities
                    thiz.SetUnderlyingNetworks(new[] { network });
                }

                public override void OnLost(Network network)
                {
                    thiz.SetUnderlyingNetworks(null);
                }
            }

            #endregion

            public override void OnCreate()
            {
                base.OnCreate();
                var policy = new StrictMode.ThreadPolicy.Builder().PermitAll().Build();
                StrictMode.SetThreadPolicy(policy);
                V2RayServiceManager.ServiceControl = new SoftReference(this);
            }

            public override void OnRevoke()
            {
                StopV2Ray();
                base.OnRevoke();
            }

            public override void OnLowMemory()
            {
                StopV2Ray();
                base.OnLowMemory();
            }

            public override void OnDestroy()
            {
                StopV2Ray();
                base.OnDestroy();
            }

            void Setup(string parameters)
            {
                var prepare = Prepare(this);
                if (prepare != null)
                {
                    return;
                }

                // If the old interface has exactly the same parameters, use it!
                // Configure a builder while parsing the parameters.
                var builder = new Builder(this).SetSession(AssemblyTrademark);
                const bool enableLocalDns = false;
                const RoutingMode routingMode = RoutingMode.绕过局域网地址;

                var parametersArray = parameters.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Split(',', StringSplitOptions.RemoveEmptyEntries)).ToArray();
                Array.ForEach(parametersArray, it =>
                {
                    _ = it.FirstOrDefault().FirstOrDefault() switch
                    {
                        'm' => it.Length >= 2 && int.TryParse(it[1], out var mtu)
                            ? builder.SetMtu(mtu) : builder,
                        's' => it.Length >= 2
                            ? builder.AddSearchDomain(it[1]) : builder,
                        'a' => it.Length >= 3 && int.TryParse(it[2], out var prefixLength)
                            ? builder.AddAddress(it[1], prefixLength) : builder,
                        'r' => routingMode switch
                        {
                            RoutingMode.绕过局域网地址 or RoutingMode.绕过局域网及大陆地址 => it.Length >= 2 &&
                                it[1] == "::" //not very elegant, should move Vpn setting in Kotlin, simplify go code
                                    ? builder.AddRoute("2000::", 3)
                                    : AddRoutes(builder, Resources!.GetStringArray(Resource.Array.bypass_private_ip_address)),
                            _ => it.Length >= 3 && int.TryParse(it[2], out var prefixLength)
                                ? builder.AddRoute(it[1], prefixLength) : builder,
                        },
                        'd' => it.Length >= 2
                            ? builder.AddDnsServer(it[1]) : builder,
                        _ => builder,
                    };
                });

                static Builder AddRoutes(Builder builder, IEnumerable<string> values)
                {
                    foreach (var value in values)
                    {
                        var array = value.Split('/', StringSplitOptions.RemoveEmptyEntries);
                        if (array.Length >= 2 && int.TryParse(array[1], out var prefixLength))
                        {
                            builder.AddRoute(array[0], prefixLength);
                        }
                    }
                    return builder;
                }

                if (!enableLocalDns)
                {
                    //Array.ForEach(Utils.VpnDnsServers, it =>
                    //{
                    //    if (Utils.IsPureIpAddress(it))
                    //    {
                    //        builder.AddDnsServer(it);
                    //    }
                    //});

                    builder.AddDnsServer(AppConfig.DNS_AGENT);
                }

                // Close the old interface since the parameters have been changed.
                try
                {
                    mInterface?.Close();
                }
                catch (Exception)
                {
                    // ignored
                }

                var sdkInt = Build.VERSION.SdkInt;

                if (sdkInt >= BuildVersionCodes.P)
                {
                    try
                    {
                        Connectivity.RequestNetwork(DefaultNetworkRequest, DefaultNetworkCallback);
                    }
                    catch (JException e)
                    {
                        e.PrintStackTrace();
                    }
                }

                if (sdkInt >= BuildVersionCodes.Q)
                {
                    builder.SetMetered(false);
                }

                // Create a new interface using the builder and save the parameters.
                try
                {
                    mInterface = builder.Establish()!;
                }
                catch (JException e)
                {
                    // non-nullable lateinit var
                    e.PrintStackTrace();
                    StopV2Ray();
                }

                SendFd();
            }

            void SendFd()
            {
                var fd = mInterface!.FileDescriptor;
                var path = new File(Utils.PackagePath(ApplicationContext!), "sock_path").AbsolutePath;

                BuildersKt.Launch(GlobalScope.Instance, Dispatchers.IO,
                    CoroutineStart.Default, null);
            }

            sealed class B : IFunction2
            {

            }

            [return: GeneratedEnum]
            public override StartCommandResult OnStartCommand(Intent? intent, [GeneratedEnum] StartCommandFlags flags, int startId)
            {
                if (intent != null)
                {
                    var action = intent.Action;
                    switch (action)
                    {
                        case START:
                            V2RayServiceManager.StartV2rayPoint();
                            return StartCommandResult.RedeliverIntent;
                        case STOP:
                        default:
                            StopV2Ray();
                            StopSelf();
                            break;
                    }
                }
                return StartCommandResult.NotSticky;
            }

            void StopV2Ray(bool isForced = true)
            {

            }

            #region IServiceControl

            AndroidAppService IServiceControl.Service => this;

            void IServiceControl.StartService(string parameters) => Setup(parameters);

            void IServiceControl.StopService()
            {
                throw new NotImplementedException();
            }

            bool IServiceControl.VpnProtect(int socket)
            {
                throw new NotImplementedException();
            }

            #endregion

            #region IServiceBase

            void IServiceBase.OnStart() => Setup("");

            void IServiceBase.OnStop() => StopV2Ray();

            #endregion
        }
    }
}
