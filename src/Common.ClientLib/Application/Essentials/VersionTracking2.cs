using System.Collections.Generic;
using System.Linq;
using Xamarin.Essentials;
using _ThisAssembly = System.Properties.ThisAssembly;

// ReSharper disable once CheckNamespace
namespace System.Application
{
    /// <summary>
    /// 提供一种在设备上跟踪应用程序版本的简便方法。
    /// </summary>
    public static class VersionTracking2
    {
        // https://github.com/xamarin/Essentials/blob/1.7.0/Xamarin.Essentials/VersionTracking/VersionTracking.shared.cs
        // https://github.com/xamarin/Essentials/blob/1.7.0/Xamarin.Essentials/AppInfo/AppInfo.uwp.cs

        const string versionsKey = "VersionTracking.Versions";
        const string sharedName = "steam++.xamarinessentials.versiontracking";

        static readonly List<string>? versionTrail;

        static VersionTracking2()
        {
            if (!Essentials.IsSupported)
            {
                IsFirstLaunchEver = !Preferences2.ContainsKey(versionsKey, sharedName);
                if (IsFirstLaunchEver)
                {
                    versionTrail = new();
                }
                else
                {
                    versionTrail = ReadHistory(versionsKey).ToList();
                }

                IsFirstLaunchForCurrentVersion = !versionTrail.Contains(CurrentVersion);
                if (IsFirstLaunchForCurrentVersion)
                {
                    versionTrail.Add(CurrentVersion);
                }

                if (IsFirstLaunchForCurrentVersion)
                {
                    WriteHistory(versionsKey, versionTrail);
                }
            }
        }

        public static Func<string>? PlatformGetVersionString { private get; set; }

        static string VersionString
        {
            get
            {
                if (PlatformGetVersionString != null) return PlatformGetVersionString();
                return _ThisAssembly.Version;
            }
        }

        /// <summary>
        /// 获取应用程序的当前版本号。
        /// </summary>
        public static string CurrentVersion
        {
            get
            {
                if (Essentials.IsSupported)
                {
                    return VersionTracking.CurrentVersion;
                }
                else
                {
                    return VersionString;
                }
            }
        }

        /// <summary>
        /// 获取此设备上安装的应用程序的第一个版本的版本号。
        /// </summary>
        public static string FirstInstalledVersion
        {
            get
            {
                if (Essentials.IsSupported)
                {
                    return VersionTracking.FirstInstalledVersion;
                }
                else
                {
                    return versionTrail!.FirstOrDefault();
                }
            }
        }

        static bool mIsFirstLaunchEver;
        /// <summary>
        /// 获取一个值，该值指示此应用是否首次在此设备上启动。
        /// </summary>
        public static bool IsFirstLaunchEver
        {
            get
            {
                if (Essentials.IsSupported)
                {
                    return VersionTracking.IsFirstLaunchEver;
                }
                else
                {
                    return mIsFirstLaunchEver;
                }
            }
            private set
            {
                mIsFirstLaunchEver = value;
            }
        }

        static bool mIsFirstLaunchForCurrentVersion;
        /// <summary>
        /// 获取一个值，该值指示这是否是当前版本号的应用程序的首次启动。
        /// </summary>
        public static bool IsFirstLaunchForCurrentVersion
        {
            get
            {
                if (Essentials.IsSupported)
                {
                    return VersionTracking.IsFirstLaunchForCurrentVersion;
                }
                else
                {
                    return mIsFirstLaunchForCurrentVersion;
                }
            }
            private set
            {
                mIsFirstLaunchForCurrentVersion = value;
            }
        }

        /// <summary>
        /// 获取以前运行的版本的版本号。
        /// </summary>
        public static string? PreviousVersion
        {
            get
            {
                if (Essentials.IsSupported)
                {
                    return VersionTracking.PreviousVersion;
                }
                else
                {
                    return GetPrevious(versionTrail!);
                }
            }
        }

        /// <summary>
        /// 获取在此设备上运行的应用的版本号集合。
        /// </summary>
        public static IEnumerable<string> VersionHistory
        {
            get
            {
                if (Essentials.IsSupported)
                {
                    return VersionTracking.VersionHistory;
                }
                else
                {
                    return versionTrail!.ToArray();
                }
            }
        }

        /// <summary>
        /// 确定这是否是指定版本号的应用程序的首次启动。
        /// </summary>
        /// <param name="version"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static bool IsFirstLaunchForVersion(string version)
        {
            if (Essentials.IsSupported)
            {
                return VersionTracking.IsFirstLaunchForVersion(version);
            }
            else
            {
                return CurrentVersion == version && IsFirstLaunchForCurrentVersion;
            }
        }

        /// <summary>
        /// 开始跟踪版本信息。
        /// </summary>
        [Preserve]
        public static void Track()
        {
            if (Essentials.IsSupported)
            {
                VersionTracking.Track();
            }
            //else
            //{
            //    // 调用空函数触发当前类静态构造函数
            //}
        }

        static string[] ReadHistory(string key)
            => Preferences2.Get(key, null, sharedName)?.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries) ?? new string[0];

        static void WriteHistory(string key, IEnumerable<string> history)
            => Preferences2.Set(key, string.Join("|", history), sharedName);

        static string? GetPrevious(List<string> trail)
        {
            return (trail.Count >= 2) ? trail[^2] : null;
        }
    }
}
