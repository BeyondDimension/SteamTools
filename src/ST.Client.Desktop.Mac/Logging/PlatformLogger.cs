using Microsoft.Extensions.Logging;
#if MONO_MAC
using MonoMac.Foundation;
#else
using Foundation;
#endif
using System.Runtime.InteropServices;
using static System.Properties.ThisAssembly;

// ReSharper disable once CheckNamespace
namespace System.Logging
{
    /// <inheritdoc cref="ClientLogger"/>
    public class PlatformLogger : ClientLogger
    {
        public PlatformLogger(string name) : base(name)
        {
        }

        public override bool IsEnabled(LogLevel logLevel)
        {
            if (Debuggable) return true;
#pragma warning disable CS0162 // 检测到无法访问的代码
            return logLevel >= LogLevel.Error;
#pragma warning restore CS0162 // 检测到无法访问的代码
        }

        public override void WriteMessage(LogLevel logLevel, string message)
        {
            NSLog(message);
        }

        [DllImport("/System/Library/Frameworks/Foundation.framework/Foundation")]
        extern static void NSLog(IntPtr format, [MarshalAs(UnmanagedType.LPWStr)] string s);

        static void NSLog(string format, params object[]? args)
        {
            var fmt = NSString.CreateNative("%s");
            var val = (args == null || args.Length == 0) ? format : string.Format(format, args);

            NSLog(fmt, val);
            NSString.ReleaseNative(fmt);
        }
    }
}