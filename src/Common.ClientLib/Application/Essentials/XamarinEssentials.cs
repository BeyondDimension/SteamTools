using System.Reflection;
using System.Runtime.Versioning;
using Xamarin.Essentials;

namespace System.Application
{
    internal static class XamarinEssentials
    {
        static readonly Lazy<bool> _IsSupported = new(() =>
        {
            // TargetFrameworkAttribute It may be deleted by link(mono) or trimmable or PublishSingleFile
            var attr = Assembly.GetAssembly(typeof(DevicePlatform)).GetCustomAttribute<TargetFrameworkAttribute>();
            return attr != null && !attr.FrameworkName.StartsWith(".NETStandard,Version=");
        });

        public static bool IsSupported => _IsSupported.Value;
    }
}