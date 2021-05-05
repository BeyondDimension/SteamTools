using System.Collections.Generic;
using System.Linq;
using System.Reflection;

// ReSharper disable once CheckNamespace
namespace System.Runtime.Versioning
{
    /// <summary>
    /// .NET目标框架的属性
    /// <para>https://docs.microsoft.com/zh-cn/dotnet/standard/frameworks</para>
    /// </summary>
    struct TargetFramework
    {
        /// <summary>
        /// 当前主进程的 .NET目标框架
        /// </summary>
        public static TargetFramework Current => new TargetFramework(Assembly.GetCallingAssembly());

        /// <summary>
        /// 当前主进程 是否运行在 .NET Framework 上
        /// </summary>
        public bool IsNETFramework => FrameworkName == Name.NETFramework;

        /// <summary>
        /// 当前主进程 是否运行在 .NET Core 上
        /// </summary>
        public bool IsNETCore => FrameworkName == Name.NETCoreApp;

        /// <summary>
        /// 当前主进程 是否运行在 Xamarin.Android 上
        /// </summary>
        public bool IsMonoAndroid => FrameworkName == Name.MonoAndroid;

        /// <summary>
        /// 当前主进程 是否运行在 Xamarin.iOS 上
        /// </summary>
        public bool IsXamarin_iOS => FrameworkName == Name.Xamarin_iOS;

        /// <summary>
        /// 当前主进程 是否运行在 .NET for Windows Universal / .NETCore,Version=v5.0 / UWP 上
        /// </summary>
        public bool IsUniversal => FrameworkName == Name.Universal;

        /// <summary>
        /// .NET目标框架名称
        /// </summary>
        public Name FrameworkName { get; }

        /// <summary>
        /// .NET目标框架版本
        /// </summary>
        public Version? Version { get; }

        /// <summary>
        /// </summary>
        public string? DisplayName { get; }

        /// <summary>
        /// .NET目标框架显示名称是否有值
        /// </summary>
        public bool HasDisplayName => !string.IsNullOrWhiteSpace(DisplayName);

        /// <summary>
        /// .NET目标框架的版本属性构造函数
        /// </summary>
        /// <param name="frameworkName"></param>
        public TargetFramework(string frameworkName) : this(frameworkName, null)
        {
        }

        /// <summary>
        /// .NET目标框架的版本属性构造函数
        /// </summary>
        /// <param name="assembly"></param>
        public TargetFramework(Assembly assembly) : this(assembly.GetRequiredCustomAttribute<TargetFrameworkAttribute>())
        {
        }

        /// <summary>
        /// .NET目标框架的版本属性构造函数
        /// </summary>
        /// <param name="type"></param>
        public TargetFramework(Type type) : this(type.Assembly)
        {
        }

        /// <summary>
        /// .NET目标框架的版本属性构造函数
        /// </summary>
        /// <param name="attribute"></param>
        public TargetFramework(TargetFrameworkAttribute attribute) : this(attribute.FrameworkName, attribute.FrameworkDisplayName)
        {
        }

        /// <summary>
        /// .NET目标框架的版本属性构造函数
        /// </summary>
        /// <param name="frameworkName"></param>
        /// <param name="displayName"></param>
        public TargetFramework(string frameworkName, string? displayName)
        {
            const string vStartsWith = "Version=v";
            Version? version = null;
            var array = Split(frameworkName);
            if (array?.Length >= 2)
            {
                FrameworkName = NameDictionary.FirstOrDefault(x => x.Value == array[0]).Key;
                if (FrameworkName == Name.Xamarin_iOS)
                    version = SetVersionByFindSdkAssemblyVersionAttribute("Xamarin.iOS");
                if (FrameworkName == Name.MonoAndroid)
                    version = SetVersionByFindSdkAssemblyVersionAttribute("Mono.Android");
                if (version == null)
                {
                    var versionString = array[1];
                    if (versionString.StartsWith(vStartsWith))
                    {
                        version = GetVersion(versionString[vStartsWith.Length..]);
                    }
                }
            }
            else
            {
                FrameworkName = 0;
            }
            Version = version;
            DisplayName = displayName;
        }

        static string[]? Split(string? s, char separator = ',') => s?.Split(separator, StringSplitOptions.RemoveEmptyEntries);

        static Version? GetVersion(string? s) => Version.TryParse(s, out var v) ? v : null;

        static Version? SetVersionByFindSdkAssemblyVersionAttribute(string sdkAssemblyName)
        {
            var sdkAssembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == sdkAssemblyName);
            if (sdkAssembly == null) return default;
            var attr = sdkAssembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            if (attr != null)
                return GetVersion(Split(attr.InformationalVersion)?.FirstOrDefault());
            return default;
        }

        ///// <summary>
        ///// $"[assembly: TargetFramework(\"{Name.GetString()},Version=v{Version}\", FrameworkDisplayName = \"{DisplayName}\")]"
        ///// </summary>
        ///// <returns></returns>
        //public string ToAssemblyAttributeString() =>
        //    $"[assembly: TargetFramework(\"{(NameDictionary.TryGetValue(FrameworkName, out var value) ? value : null)},Version=v{Version}\", FrameworkDisplayName = \"{DisplayName}\")]";

        public override string ToString()
        {
            var name = FrameworkName switch
            {
                Name.NETFramework => ".NET Framework {0}",
                Name.NETCoreApp => ".NET Core {0}",
                Name.NETStandard => ".NET Standard {0}",
                Name.MonoAndroid => "Xamarin.Android {0}",
                Name.Xamarin_iOS => "Xamarin.iOS {0}",
                Name.Universal => "Windows Universal {0}",
                _ => "Unknown {0}",
            };
            return string.Format(name, Version);
        }

        /// <summary>
        /// .NET目标框架名称
        /// </summary>
        public enum Name : byte
        {
            /// <summary>
            /// .NET Framework / Mono
            /// </summary>
            NETFramework = 1,

            /// <summary>
            /// .NET Core
            /// </summary>
            NETCoreApp = 2,

            /// <summary>
            /// .NET Standard
            /// </summary>
            NETStandard = 3,

            /// <summary>
            /// Xamarin.Android
            /// <para>https://developer.xamarin.com/releases/android</para>
            /// </summary>
            MonoAndroid = 4,

            /// <summary>
            /// Xamarin.iOS
            /// <para>https://developer.xamarin.com/releases/ios</para>
            /// </summary>
            Xamarin_iOS = 5,

            /// <summary>
            /// .NET for Windows Universal / .NETCore,Version=v5.0 / UWP
            /// </summary>
            Universal = 6,
        }

        internal static readonly Dictionary<Name, string> NameDictionary = new Dictionary<Name, string>
        {
                { Name.Universal, ".NETCore" },
                { Name.Xamarin_iOS, "Xamarin.iOS" },
                { Name.MonoAndroid, "MonoAndroid" },
                { Name.NETFramework, ".NETFramework" },
                { Name.NETCoreApp, ".NETCoreApp" },
                { Name.NETStandard, ".NETStandard" },
        };
    }
}