using System.Properties;
#if DEBUG
using System.Diagnostics;
using System.Security.Cryptography;
#endif

namespace System.Application
{
    public static class BuildConfig
    {
#if DEBUG
        [Obsolete("use ThisAssembly.Debuggable", true)]
        public const bool DEBUG = ThisAssembly.Debuggable;
#endif

        public const string APPLICATION_ID = "net.steampp.app";

#if DEBUG
        [Obsolete("NotImplemented", true)]
        public const string BUILD_TYPE = "";

        [Obsolete("NotImplemented", true)]
        public const string FLAVOR = "";

        [Obsolete("NotImplemented", true)]
        public const int VERSION_CODE = default;

        [Obsolete("NotImplemented", true)]
        public const string VERSION_NAME = "";
#endif

#if DEBUG
        static readonly Lazy<bool> mIsAigioPC = new(() =>
        {
            var MachineCr32 = Hashs.String.Crc32(Environment.MachineName, false);
            var UserNameCr32 = Hashs.String.Crc32(Environment.UserName, false);
            return MachineCr32 == "88DF9AB0" ||
            UserNameCr32 == "8AA383BC" ||
            MachineCr32 == "0EAAEAA5" ||
            UserNameCr32 == "CCB27AA9";
        });
        public static bool IsAigioPC => mIsAigioPC.Value;
        public static bool IsDebuggerAttached => Debugger.IsAttached;
#endif
    }
}