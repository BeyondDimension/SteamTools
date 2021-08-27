using Microsoft.Extensions.Logging;
#if MONO_MAC
using MonoMac.Foundation;
#else
using Foundation;
#endif
using System.Runtime.InteropServices;

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
#if DEBUG
            return true;
#else
            return logLevel >= LogLevel.Error;
#endif
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