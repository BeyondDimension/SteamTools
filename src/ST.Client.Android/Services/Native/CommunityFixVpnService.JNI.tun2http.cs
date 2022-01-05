using Android.Runtime;
using System.Runtime.InteropServices;

namespace System.Application.Services.Native
{
    partial class CommunityFixVpnService
    {
        const string tun2http = "tun2http";

        [DllImport(tun2http,
            EntryPoint = "Java_tun_proxy_service_Tun2HttpVpnService_jni_1init")]
        static extern void jni_init(IntPtr env, IntPtr thiz);
        [DllImport(tun2http,
            EntryPoint = "Java_tun_proxy_service_Tun2HttpVpnService_jni_1start")]
        static extern void jni_start(IntPtr env, IntPtr thiz, IntPtr tun, IntPtr fwd53, IntPtr rcode, IntPtr proxyIp, IntPtr proxyPort);
        [DllImport(tun2http,
            EntryPoint = "Java_tun_proxy_service_Tun2HttpVpnService_jni_1stop")]
        static extern void jni_stop(IntPtr env, IntPtr thiz, IntPtr tun);
        [DllImport(tun2http,
            EntryPoint = "Java_tun_proxy_service_Tun2HttpVpnService_jni_1get_1mtu")]
        static extern int jni_get_mtu(IntPtr env, IntPtr thiz);
        [DllImport(tun2http,
            EntryPoint = "Java_tun_proxy_service_Tun2HttpVpnService_jni_1done")]
        static extern void jni_done(IntPtr env, IntPtr thiz);
        [DllImport(tun2http,
            EntryPoint = "Java_tun_utils_Util_jni_1getprop")]
        static extern string jni_getprop(IntPtr env, IntPtr thiz, IntPtr name);

#pragma warning disable IDE1006 // 命名样式
        void jni_init() => jni_init(JNIEnv.Handle, Handle);
        void jni_start(int tun, bool fwd53, int rcode, string proxyIp, int proxyPort)
        {
            var tun_ = new Java.Lang.Integer(tun);
            var fwd53_ = new Java.Lang.Boolean(fwd53);
            var rcode_ = new Java.Lang.Integer(rcode);
            var proxyIp_ = new Java.Lang.String(proxyIp);
            var proxyPort_ = new Java.Lang.Integer(proxyPort);
            jni_start(JNIEnv.Handle, Handle, tun_.Handle, fwd53_.Handle, rcode_.Handle, proxyIp_.Handle, proxyPort_.Handle);
        }
        void jni_stop(int tun)
        {
            var tun_ = new Java.Lang.Integer(tun);
            jni_stop(JNIEnv.Handle, Handle, tun_.Handle);
        }
        int jni_get_mtu() => jni_get_mtu(JNIEnv.Handle, Handle);
        void jni_done() => jni_init(JNIEnv.Handle, Handle);
        string jni_getprop(string name)
        {
            var name_ = new Java.Lang.String(name);
            return jni_getprop(JNIEnv.Handle, Handle, name_.Handle);
        }
#pragma warning restore IDE1006 // 命名样式
    }
}
