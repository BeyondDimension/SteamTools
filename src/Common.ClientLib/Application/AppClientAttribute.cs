using MessagePack;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Properties;
using System.Reflection;
using System.Text;

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
                var bytes = str.Base64UrlDecodeToByteArray_Nullable();
                Value = MessagePackSerializer.Deserialize(type, bytes);
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
            T t = (attr != default && attr.Value is T value) ? value : default;
#if DEBUG
            stopwatch.Stop();
            Debug.WriteLine($"AppClientAttr Get<{type.Name}> 耗时：{stopwatch.ElapsedMilliseconds}ms");
#endif
            return t;
        }

        public static string? GetResValue(Assembly assembly, string name, bool isSingle, string namespacePrefix)
        {
            var resName = isSingle ? $"{namespacePrefix}{name}.pfx" : $"{namespacePrefix}{name}-{(ThisAssembly.Debuggable ? "debug" : "release")}.pfx";
            using var stream = assembly.GetManifestResourceStream(resName);
            if (stream != null)
            {
                using var reader = new StreamReader(stream, Encoding.UTF8);
                return reader.ReadToEnd();
            }
            return null;
        }
    }
}