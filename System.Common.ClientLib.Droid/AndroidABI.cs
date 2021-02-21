using System.Collections.Generic;
using System.Linq;

namespace System
{
    /// <summary>
    /// 不同的 Android 设备使用不同的 CPU，而不同的 CPU 支持不同的指令集。
    /// <para>CPU 与指令集的每种组合都有专属的应用二进制接口 (ABI)。</para>
    /// <para>https://developer.android.google.cn/ndk/guides/abis.html</para>
    /// <para>在比较版本支持的值与设备支持的值中的交集中，值越大的，优先选取。</para>
    /// </summary>
    public static class AndroidABI
    {
        /// <inheritdoc cref="AndroidABI"/>
        [Flags]
        public enum Value
        {
            [Obsolete("Deprecated")]
            ARM = 0x1,

            [Obsolete("Deprecated")]
            MIPS = 0x2,

            /// <summary>
            /// https://developer.android.google.cn/ndk/guides/abis.html#x86
            /// </summary>
            X86 = 0x4,

            /// <summary>
            /// https://developer.android.google.cn/ndk/guides/abis.html#v7a
            /// </summary>
            ARM32 = 0x8,

            [Obsolete("Deprecated")]
            MIPS64 = 0x10,

            /// <summary>
            /// https://developer.android.google.cn/ndk/guides/abis.html#86-64
            /// </summary>
            X64 = 0x20,

            /// <summary>
            /// https://developer.android.google.cn/ndk/guides/abis.html#arm64-v8a
            /// </summary>
            ARM64 = 0x40,
        }

        static readonly IReadOnlyDictionary<string, Value> mapping = new Dictionary<string, Value>
        {
            { "armeabi-v7a", Value.ARM32 },
            { "arm64-v8a", Value.ARM64 },
            { "x86", Value.X86 },
            { "x86_64", Value.X64 },
#pragma warning disable CS0618 // 类型或成员已过时
            { "armeabi", Value.ARM },
            { "mips", Value.MIPS },
            { "mips64", Value.MIPS64 },
#pragma warning restore CS0618 // 类型或成员已过时
        };

        static readonly Lazy<IReadOnlyDictionary<Value, string>> mapping_reverse
            = new Lazy<IReadOnlyDictionary<Value, string>>(() => mapping.ReverseKeyValue());

        /// <summary>
        /// 将多个 AndroidABI 字符串 转换为 枚举
        /// </summary>
        /// <param name="abis"></param>
        /// <returns></returns>
        public static Value Convert(IEnumerable<string>? abis)
        {
            Value value = 0;
            if (abis != null)
            {
                foreach (var abi in abis)
                {
                    if (!string.IsNullOrWhiteSpace(abi) && mapping.ContainsKey(abi))
                    {
                        value |= mapping[abi];
                    }
                }
            }
            return value;
        }

        /// <summary>
        /// 将多个 AndroidABI 字符串 转换为 枚举
        /// </summary>
        /// <param name="abis"></param>
        /// <returns></returns>
        public static Value Convert(params string[] abis) => Convert(abis.AsEnumerable());

        /// <summary>
        /// 将 AndroidABI 枚举(单值) 转换为字符串
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ToStringSingle(this Value value)
        {
            var pairs = mapping_reverse.Value;
            if (pairs.ContainsKey(value)) return pairs[value];
            return value.ToString();
        }

        /// <summary>
        /// 将 AndroidABI 枚举(多值) 转换为字符串
        /// </summary>
        /// <param name="value"></param>
        /// <param name="separator"></param>
        /// <returns></returns>
        public static string ToStringMultiple(this Value value, string separator = ", ")
        {
            var enumType = typeof(Value);
            if (Enum.IsDefined(enumType, value)) return ToStringSingle(value);
            var all_values = (Value[])Enum.GetValues(enumType);
            var values = from x in all_values let has = value.HasFlag(x) where has select ToStringSingle(x);
            return string.Join(separator, value);
        }
    }
}