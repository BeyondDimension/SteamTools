using Android.OS;
using Java.Lang;
using System.Collections.Generic;
using System.Linq;
using StringBuilder = System.Text.StringBuilder;

namespace System
{
    /// <summary>
    /// Android ROM 相关函数集
    /// </summary>
    public static class AndroidROM
    {
        /// <summary>
        /// Android ROM 信息
        /// </summary>
        public sealed class Info
        {
            public bool HasVersion { get; }

            public Version? Version { get; }

            public string? VersionString { get; }

            public bool HasVersionString { get; }

            public AndroidROMType Type { get; }

            public Info(AndroidROMType type, string? version)
            {
                Type = type;
                VersionString = version;
                HasVersionString = !string.IsNullOrWhiteSpace(version);
#pragma warning disable CS8604 // 可能的 null 引用参数。
                Version = HasVersionString ? StringExtensions.VersionTryParse(version) : null;
#pragma warning restore CS8604 // 可能的 null 引用参数。
                HasVersion = Version != null;
            }

            public override string ToString()
            {
                if (HasVersionString) return $"{Type} {VersionString}";
                return Type.ToString();
            }

            public void ToString(StringBuilder b)
            {
                if (HasVersionString)
                {
                    b.AppendFormat("{0} {1}", Type, VersionString);
                }
                if (Type != AndroidROMType.Unknown) b.Append(Type.ToString());
            }

            public static Info Unknown { get; } = new Info(AndroidROMType.Unknown, null);

            /// <summary>
            /// is MIUI (MI)
            /// </summary>
            public bool IsMIUI => Type == AndroidROMType.MIUI;

            /// <summary>
            /// is Emotion UI (HUAWEI)
            /// </summary>
            public bool IsEMUI => Type == AndroidROMType.EMUI;

            /// <summary>
            /// is ColorOS (OPPO)
            /// </summary>
            public bool IsColorOS => Type == AndroidROMType.ColorOS;

            /// <summary>
            /// is Funtouch OS (vivo)
            /// </summary>
            public bool IsFuntouchOS => Type == AndroidROMType.FuntouchOS;

            /// <summary>
            /// is Smartisan OS (Smartisan)
            /// </summary>
            public bool IsSmartisanOS => Type == AndroidROMType.SmartisanOS;
        }

        const string RO_BUILD_DISPLAY_ID = "ro.build.display.id";

        /// <summary>
        /// ROM 识别关键字
        /// </summary>
        static readonly IReadOnlyDictionary<string, AndroidROMType> identify_keywords_rule_a = new Dictionary<string, AndroidROMType>
        {
            { "ro.miui.ui.version.name", AndroidROMType.MIUI }, // https://dev.mi.com/doc/?p=254
            { "ro.build.version.emui", AndroidROMType.EMUI },
            { "ro.build.version.opporom", AndroidROMType.ColorOS },
            { "ro.vivo.os.version", AndroidROMType.FuntouchOS },
            { "ro.smartisan.version", AndroidROMType.SmartisanOS },
            { "ro.letv.release.version", AndroidROMType.EUI },
            { "ro.build.sense.version", AndroidROMType.HTCSense },
            { "ro.build.nubia.rom.code", AndroidROMType.NubiaUI },
        };

        static readonly IReadOnlyDictionary<string, AndroidROMType> identify_keywords_rule_b = new Dictionary<string, AndroidROMType>
        {
            { "flyme", AndroidROMType.Flyme },
            { "amigo", AndroidROMType.AmigoOS },
        };

        /// <summary>
        /// 当前设备是否支持 Meizu Flyme SmartBar
        /// </summary>
        public static bool HasFlymeSmartBar
        {
            get
            {
                try
                {
                    var method = Class.ForName("android.os.Build").GetMethod("hasSmartBar");
                    if (method != null)
                    {
                        var value = method.Invoke(null);
                        if (value != null)
                        {
                            return (bool)value;
                        }
                    }
                }
                catch
                {
                }
                return false;
            }
        }

        static Info? mCurrent;

        /// <summary>
        /// 获取当前 ROM 信息
        /// </summary>
        public static Info Current => mCurrent ?? throw new NullReferenceException("must call AndroidROM.Initialize() before using.");

        static bool isInit;

        /// <summary>
        /// 初始化 ROM 信息
        /// </summary>
        public static void Initialize()
        {
            if (isInit) return;
            string? version = null;
            var firstOrDefault = identify_keywords_rule_a
                .FirstOrDefault(x => SystemProperties.TryGet(x.Key, out version));
            if (firstOrDefault.Key != null)
            {
                mCurrent = new Info(firstOrDefault.Value, version);
            }
            else
            {
                if (SystemProperties.TryGet(RO_BUILD_DISPLAY_ID, out var value))
                {
                    foreach (var item in identify_keywords_rule_b)
                    {
                        if (value.Contains(item.Key, StringComparison.OrdinalIgnoreCase))
                        {
                            mCurrent = new Info(item.Value, value);
                            break;
                        }
                    }
                }
            }
            if (mCurrent == null) mCurrent = Info.Unknown;
            isInit = true;
        }
    }
}