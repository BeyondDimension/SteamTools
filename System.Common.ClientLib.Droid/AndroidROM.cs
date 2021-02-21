using Android.OS;
using Java.Lang;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xamarin.Essentials;

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

        #region Emulator

        static int rating = -1;
        static readonly Lazy<bool> mIsEmulator = new Lazy<bool>(GetIsEmulator);

        static bool GetIsEmulator()
        {
            // 参考 https://github.com/gingo/android-emulator-detector/blob/master/EmulatorDetectorProject/EmulatorDetector/src/main/java/net/skoumal/emulatordetector/EmulatorDetector.java
            return _() || DeviceInfo.DeviceType == DeviceType.Virtual;
            static bool contains(string l, string r)
            {
                return l.Contains(r, StringComparison.OrdinalIgnoreCase);
            }
            static bool equals(string l, string r) => string.Equals(l, r, StringComparison.OrdinalIgnoreCase);
            static bool _()
            {
                var newRating = 0;
                if (rating < 0)
                {
                    var PRODUCT = Build.Product;
                    if (PRODUCT == null ||
                        contains(PRODUCT, "sdk") ||
                        contains(PRODUCT, "Andy") ||
                        contains(PRODUCT, "ttVM_Hdragon") ||
                        contains(PRODUCT, "google_sdk") ||
                        contains(PRODUCT, "Droid4X") ||
                        contains(PRODUCT, "nox") ||
                        contains(PRODUCT, "sdk_x86") ||
                        contains(PRODUCT, "sdk_google") ||
                        contains(PRODUCT, "vbox86p"))
                    {
                        newRating++;
                    }

                    var MANUFACTURER = Build.Manufacturer;
                    if (MANUFACTURER == null ||
                        equals(MANUFACTURER, "unknown") ||
                        equals(MANUFACTURER, "Genymotion") ||
                        contains(MANUFACTURER, "VS Emulator") ||
                        contains(MANUFACTURER, "Andy") ||
                        contains(MANUFACTURER, "MIT") ||
                        contains(MANUFACTURER, "nox") ||
                        contains(MANUFACTURER, "TiantianVM"))
                    {
                        newRating++;
                    }

                    var BRAND = Build.Brand;
                    if (BRAND == null ||
                        equals(BRAND, "generic") ||
                        equals(BRAND, "generic_x86") ||
                        equals(BRAND, "TTVM") ||
                        contains(BRAND, "Andy"))
                    {
                        newRating++;
                    }

                    var DEVICE = Build.Device;
                    if (DEVICE == null ||
                        contains(DEVICE, "generic") ||
                        contains(DEVICE, "generic_x86") ||
                        contains(DEVICE, "Andy") ||
                        contains(DEVICE, "ttVM_Hdragon") ||
                        contains(DEVICE, "Droid4X") ||
                        contains(DEVICE, "nox") ||
                        contains(DEVICE, "generic_x86_64") ||
                        contains(DEVICE, "vbox86p"))
                    {
                        newRating++;
                    }

                    var MODEL = Build.Model;
                    if (MODEL == null ||
                        equals(MODEL, "sdk") ||
                        equals(MODEL, "google_sdk") ||
                        contains(MODEL, "Droid4X") ||
                        contains(MODEL, "TiantianVM") ||
                        contains(MODEL, "Andy") ||
                        equals(MODEL, "Android SDK built for x86_64") ||
                        equals(MODEL, "Android SDK built for x86"))
                    {
                        newRating++;
                    }

                    var HARDWARE = Build.Hardware;
                    if (HARDWARE == null ||
                        equals(HARDWARE, "goldfish") ||
                        equals(HARDWARE, "vbox86") ||
                        contains(HARDWARE, "nox") ||
                        contains(HARDWARE, "ttVM_x86"))
                    {
                        newRating++;
                    }

                    var FINGERPRINT = Build.Fingerprint;
                    if (FINGERPRINT == null ||
                        contains(FINGERPRINT, "generic/sdk/generic") ||
                        contains(FINGERPRINT, "generic_x86/sdk_x86/generic_x86") ||
                        contains(FINGERPRINT, "Andy") ||
                        contains(FINGERPRINT, "ttVM_Hdragon") ||
                        contains(FINGERPRINT, "generic_x86_64") ||
                        contains(FINGERPRINT, "generic/google_sdk/generic") ||
                        contains(FINGERPRINT, "vbox86p") ||
                        contains(FINGERPRINT, "generic/vbox86p/vbox86p"))
                    {
                        newRating++;
                    }

                    try
                    {
                        var opengl = Android.Opengl.GLES20.GlGetString(Android.Opengl.GLES20.GlRenderer);
                        if (!string.IsNullOrWhiteSpace(opengl))
                        {
#pragma warning disable CS8604 // 可能的 null 引用参数。
                            if (contains(opengl, "Bluestacks") ||
                                contains(opengl, "Translator")
                            )
#pragma warning restore CS8604 // 可能的 null 引用参数。
                                newRating += 10;
                        }
                    }
                    catch
                    {
                    }

                    try
                    {
#pragma warning disable CS0618 // 类型或成员已过时
                        var esDir = Android.OS.Environment.ExternalStorageDirectory;
#pragma warning restore CS0618 // 类型或成员已过时
                        if (esDir != null)
                        {
                            var path = Path.Combine(esDir.ToString(), "windows", "BstSharedFolder");
                            var sharedFolder = new Java.IO.File(path);
                            if (sharedFolder.Exists())
                            {
                                newRating += 10;
                            }
                        }
                    }
                    catch
                    {
                    }
                    rating = newRating;
                }
                return rating > 3;
            }
        }

        /// <summary>
        /// 当前设备是否为模拟器
        /// </summary>
        public static bool IsEmulator => mIsEmulator.Value;

        #endregion

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