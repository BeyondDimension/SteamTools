using Android;
using Android.App;
using Android.Content;
using Android.Net;
using Android.OS;
using Android.Runtime;
using AndroidX.Activity.Result;
using AndroidX.Activity.Result.Contract;
using System.Application.Services.Native;
using System.Application.UI;
using System.Application.UI.Activities;
using System.Net;
using JObject = Java.Lang.Object;

namespace System.Application.Services.Native
{
    /// <summary>
    /// 使用 虚拟专用网 (VPN) 进行网络加速
    /// <para>https://developer.android.google.cn/guide/topics/connectivity/vpn?hl=zh_cn</para>
    /// </summary>
    [Register(JavaPackageConstants.Services + nameof(CommunityFixVpnService))]
    [Service(Permission = Manifest.Permission.BindVpnService)]
    [IntentFilter(new[] { "android.net.VpnService" })]
    internal sealed class CommunityFixVpnService : VpnService
    {
        static Intent GetServiceIntent(Context context)
            => new(context, typeof(CommunityFixVpnService));

        /// <summary>
        /// 启动 VPN 服务
        /// </summary>
        /// <param name="activity"></param>
        /// <param name="startOrStop"></param>
        static void Start(Activity activity, bool startOrStop)
           => activity.StartService(GetServiceIntent(activity).SetAction(startOrStop ? ACTION_CONNECT : ACTION_DISCONNECT));

        /// <summary>
        /// 调用 VpnService.prepare() 以询问权限（需要时）与 启动 VPN 服务
        /// </summary>
        /// <param name="activity"></param>
        public static void Start(IActivity activity, bool startOrStop = true)
        {
            if (startOrStop)
            {
                var intent = Prepare(activity.Activity);
                if (intent != null)
                {
                    // 调用 VpnService.prepare() 以询问权限（需要时）
                    activity.Launcher.Launch(intent);
                    return;
                }
            }
            // 不需要授权则直接启动
            Start(activity.Activity, startOrStop);
        }

        public interface IActivity : IViewHost
        {
            ActivityResultLauncher Launcher { get; }

            protected static ActivityResultLauncher GetActivityResultLauncher(IActivity activity)
            {
                Contract contract = new(activity.Activity);
                var launcher = activity.Activity.RegisterForActivityResult(contract, contract);
                return launcher;
            }
        }

        sealed class Contract : ActivityResultContract, IActivityResultCallback
        {
            // https://developer.android.google.cn/training/basics/intents/result?hl=zh-cn

            readonly Activity activity;

            public Contract(Activity activity) => this.activity = activity;

            public override Intent CreateIntent(Context context, JObject? input)
            {
                if (input is Intent intent) return intent;
                throw new NotSupportedException();
            }

            public override JObject? ParseResult(int resultCode, Intent? intent)
            {
                if (resultCode == (int)Result.Ok)
                {
                    Start(activity, true);
                }
                return null;
            }

            void IActivityResultCallback.OnActivityResult(JObject? result)
            {

            }
        }

        const string ACTION_CONNECT = JavaPackageConstants.Services + nameof(CommunityFixVpnService) + ".START";
        const string ACTION_DISCONNECT = JavaPackageConstants.Services + nameof(CommunityFixVpnService) + ".STOP";

        ParcelFileDescriptor? localTunnel;

        //public override void OnCreate()
        //{
        //    base.OnCreate();
        //}

        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand(Intent? intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {
            if (intent != null && ACTION_DISCONNECT.Equals(intent.Action))
            {
                Disconnect();
                return StartCommandResult.NotSticky;
            }
            else
            {
                Connect();
                return StartCommandResult.Sticky;
            }
        }

        void Connect()
        {
            // Configure a new interface from our VpnService instance. This must be done
            // from inside a VpnService.
            var builder = new Builder(this);
            // Create a local TUN interface using predetermined addresses. In your app,
            // you typically use values returned from the VPN gateway during handshaking.
            localTunnel = builder
              .AddAddress("192.168.2.2", 24)
              .AddRoute("0.0.0.0", 0)
              .AddDnsServer("192.168.1.1")
              .Establish();
        }

        void Disconnect()
        {
            if (localTunnel != null)
            {
                try
                {
                    localTunnel.Close();
                }
                catch (Java.IO.IOException)
                {

                }
                localTunnel.Dispose();
                localTunnel = null;
            }
        }

        public override void OnRevoke()
        {
            base.OnRevoke();
            Disconnect();
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            Disconnect();
        }
    }
}

namespace System.Application.Services.Implementation
{
    partial class AndroidPlatformServiceImpl
    {
        bool IPlatformService.SetAsSystemProxy(bool state, IPAddress? ip, int port)
        {
            var activity = MainActivity.Instance;
            CommunityFixVpnService.Start(activity, state);
            return true;
        }
    }
}

namespace System.Application.UI.Activities
{
    partial class MainActivity : CommunityFixVpnService.IActivity
    {
        ActivityResultLauncher? _CommunityFixVpnServiceActivityResultLauncher;

        ActivityResultLauncher CommunityFixVpnService.IActivity.Launcher => _CommunityFixVpnServiceActivityResultLauncher ?? throw new ArgumentNullException(nameof(_CommunityFixVpnServiceActivityResultLauncher));

        void InitCommunityFixVpnServiceActivityResultLauncher()
        {
            _CommunityFixVpnServiceActivityResultLauncher = CommunityFixVpnService.IActivity.GetActivityResultLauncher(this);
        }
    }
}