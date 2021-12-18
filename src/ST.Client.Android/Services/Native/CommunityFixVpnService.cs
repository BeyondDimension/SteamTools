using Android;
using Android.App;
using Android.Content;
using Android.Net;
using Android.Runtime;
using System.Collections.Generic;
using System.Text;

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

    }
}
