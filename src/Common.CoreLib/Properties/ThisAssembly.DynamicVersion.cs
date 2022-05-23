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
                if (OperatingSystem2.IsLinux() || OperatingSystem2.IsAndroid())
                {
                    return Version + alpha;
                }
                else if (OperatingSystem2.IsMacOS())
                {
                    return Version + beta;
                }
                else
                {
                    return Version;
                }
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