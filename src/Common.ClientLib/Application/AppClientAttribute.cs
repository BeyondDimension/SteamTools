using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Properties;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using _ThisAssembly = System.Properties.ThisAssembly;

namespace System.Application
{
    [DebuggerDisplay("{DebuggerDisplay(),nq}")]
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public sealed class AppClientAttribute : Attribute
    {
        string DebuggerDisplay() => $"type: {type}";

        readonly Type? type;
        string? str;

        [NotNull, DisallowNull] // C# 8 not null
        public object? Value { get; private set; }

        public AppClientAttribute(Type type, string str)
        {
            this.type = type;
            this.str = str;
            init();
            if (Value == null) throw new ArgumentNullException(nameof(Value));
        }

#pragma warning disable IDE1006 // 命名样式

        void init()
#pragma warning restore IDE1006 // 命名样式
        {
            ctor();

            void ctor()
            {
#if DEBUG
                var stopwatch = Stopwatch.StartNew();
#endif
                Value = Serializable.DMPB64U(type.ThrowIsNull(nameof(type)), str)
                    .ThrowIsNull(nameof(Value));
                str = null;
#if DEBUG
                stopwatch.Stop();
                Debug.WriteLine($"AppClientAttr init 耗时：{stopwatch.ElapsedMilliseconds}ms");
#endif
            }
        }

        [return: MaybeNull]
        public static T Get<T>(Assembly? assembly = null)
        {
#if DEBUG
            var stopwatch = Stopwatch.StartNew();
#endif
            assembly ??= Assembly.GetCallingAssembly();
            var attrs = assembly.GetCustomAttributes<AppClientAttribute>();
            var type = typeof(T);
            var attr = attrs.FirstOrDefault(x => x.Value != null && x.type == type);
            var t = (attr != default && attr.Value is T value) ? value : default;
#if DEBUG
            stopwatch.Stop();
            Debug.WriteLine($"AppClientAttr Get<{type.Name}> 耗时：{stopwatch.ElapsedMilliseconds}ms");
#endif
            return t;
        }

        public enum ResValueFormat : byte
        {
            String,
            StringGuidD,
            StringGuidN,
        }

        public static string? GetResValue(Func<string, Stream?> func, string name, bool isSingle, string namespacePrefix, ResValueFormat format)
        {
            static string GetResFileName(string name, bool isSingle) => isSingle ? $"{name}.pfx" : $"{name}-{(_ThisAssembly.Debuggable ? "debug" : "release")}.pfx";
            static string GetResFileNameH(string name) => Hashs.String.Crc32(name, isLower: false);
            static string GetResFullName(string name, string namespacePrefix) => string.IsNullOrEmpty(namespacePrefix) ? name : namespacePrefix + name;
            static MemoryStream GetMemoryStream(Stream stream)
            {
                if (stream is not MemoryStream ms)
                {
                    ms = new MemoryStream();
                    stream.CopyTo(ms);
                }
                return ms;
            }
            static string ReadStream(Stream stream, ResValueFormat format)
            {
                if (format == ResValueFormat.String)
                {
                    using var reader = new StreamReader(stream, Encoding.UTF8);
                    return reader.ReadToEnd();
                }
                if (format == ResValueFormat.StringGuidD || format == ResValueFormat.StringGuidN)
                {
                    using var ms = GetMemoryStream(stream);
                    var guid = new Guid(ms.ToArray());
                    return format switch
                    {
                        ResValueFormat.StringGuidD => guid.ToString("D"),
                        ResValueFormat.StringGuidN => guid.ToString("N"),
                        _ => throw new ArgumentOutOfRangeException(nameof(format), format, null),
                    };
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(format), format, null);
                }
            }
            name = GetResFileName(name, isSingle);
            name = GetResFileNameH(name);
            name = GetResFullName(name, namespacePrefix);
            using var stream = func(name);
            if (stream != null) return ReadStream(stream, format);
            return null;
        }
    }
}