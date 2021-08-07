namespace System.Properties
{
    static partial class ThisAssembly
    {
        const string alpha = "-alpha";
        const string beta = "-beta";

        public static string DynamicVersion
        {
            get
            {
                switch (DI.Platform)
                {
                    case Platform.Windows:
                        return DI.IsDesktopBridge ? Version + beta : Version;
                    case Platform.Linux:
                        return Version + alpha;
                    case Platform.Android:
                        return Version + alpha;
                    case Platform.Apple:
                        switch (DI.DeviceIdiom)
                        {
                            case DeviceIdiom.Phone:
                                break;
                            case DeviceIdiom.Tablet:
                                break;
                            case DeviceIdiom.Desktop:
                                return Version + beta;
                            case DeviceIdiom.TV:
                                break;
                            case DeviceIdiom.Watch:
                                break;
                        }
                        break;
                    case Platform.UWP:
                        break;
                }
                return Version;
            }
        }

#if NETSTANDARD2_1 || NETCOREAPP2_1_OR_GREATER
        static readonly Lazy<bool> mIsBetaRelease = new(() => DynamicVersion.Contains(beta, StringComparison.Ordinal));
        public static bool IsBetaRelease => mIsBetaRelease.Value;

        static readonly Lazy<bool> mIsAlphaRelease = new(() => DynamicVersion.Contains(alpha, StringComparison.Ordinal));
        public static bool IsAlphaRelease => mIsAlphaRelease.Value;

        static readonly Lazy<string> mVersionDisplay = new(() =>
        {
            Version version = new(Version);
            return $"{version.ToString(3)}{(IsAlphaRelease ? " α" : (IsBetaRelease ? " β" : ""))}{(version.Revision <= 0 ? "" : " rev." + version.Revision)}";
        });
        public static string VersionDisplay => mVersionDisplay.Value;
#endif
    }
}