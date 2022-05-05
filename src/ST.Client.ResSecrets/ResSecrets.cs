using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using _ThisAssembly = System.Properties.ThisAssembly;

namespace System.Application.Security
{
    internal static class ResSecrets
    {
        public const string Prefix_UIRes = "System.Application.UI.Resources.";
        public const string Prefix_Res = "System.Application.Resources.";

        internal static string? GetResValue(string name, bool isSingle, ResValueFormat format, string namespacePrefix = Prefix_UIRes, Assembly? assembly = null)
        {
            assembly ??= Assembly.GetExecutingAssembly();
            Stream? func(string x) => assembly.GetManifestResourceStream(x);
            var r = GetResValueCore(func, name, isSingle, namespacePrefix, format);
            return r;
        }

        internal enum ResValueFormat : byte
        {
            String,
            StringGuidD,
            StringGuidN,
        }

        static string? GetResValueCore(Func<string, Stream?> func, string name, bool isSingle, string namespacePrefix, ResValueFormat format)
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
                    using var reader = new StreamReader(stream, EncodingCache.UTF8NoBOM);
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
