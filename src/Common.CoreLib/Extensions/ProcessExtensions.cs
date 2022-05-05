using System.Diagnostics;
using System.Runtime.Versioning;

// ReSharper disable once CheckNamespace
namespace System
{
    public static class ProcessExtensions
    {
        public static ProcessModule? TryGetMainModule(this Process process)
        {
            try
            {
                return process.MainModule;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 立即停止关联的进程，并停止其子/后代进程。
        /// </summary>
        /// <param name="process"></param>
        [UnsupportedOSPlatform("ios")]
        [UnsupportedOSPlatform("tvos")]
        public static void KillEntireProcessTree(this Process process)
        {
#if NETCOREAPP3_0_OR_GREATER
            process.Kill(entireProcessTree: true);
#else
            process.Kill();
#endif
        }
    }
}