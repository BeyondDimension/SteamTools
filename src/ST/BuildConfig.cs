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
        [Obsolete("using _ThisAssembly = System.Properties.ThisAssembly;\r\n_ThisAssembly.Debuggable", true)]
        public const bool DEBUG = ThisAssembly.Debuggable;
#endif

        public const string APPLICATION_ID = "net.steampp.app";

#if DEBUG
        public const string BUILD_TYPE = ThisAssembly.Debuggable ? "debug" : "release";

        [Obsolete("NotImplemented", true)]
        public const string FLAVOR = "";

        [Obsolete("NotImplemented", true)]
        public const int VERSION_CODE = default;

        [Obsolete("using _ThisAssembly = System.Properties.ThisAssembly;\r\n_ThisAssembly.Version", true)]
        public const string VERSION_NAME = ThisAssembly.Version;
#endif

#if DEBUG
        static readonly Lazy<bool> mIsAigioPC = new(() =>
        {
            var mMachineCr32 = Hashs.String.Crc32(Environment.MachineName, false);
            var mUserNameCr32 = Hashs.String.Crc32(Environment.UserName, false);
            return mMachineCr32 == "88DF9AB0" ||
            mUserNameCr32 == "8AA383BC" ||
            mMachineCr32 == "0EAAEAA5" ||
            mUserNameCr32 == "CCB27AA9";
        });

        public static bool IsAigioPC => mIsAigioPC.Value;

        public static bool IsDebuggerAttached => Debugger.IsAttached;
#endif
    }
}