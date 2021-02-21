using System.IO;
using System.Linq;
using System.Text;

namespace System.Security.Cryptography
{
    /// <summary>
    /// 哈希算法
    /// </summary>
    public static partial class Hashs
    {
        internal const bool def_hash_str_is_lower = true;

        internal static string ToString(byte[] bytes, bool isLower = def_hash_str_is_lower) => string.Join(null, bytes.Select(x => x.ToString($"{(isLower ? "x" : "X")}2")));

        internal static byte[] ComputeHash<T>(byte[] buffer, T hashAlgorithm) where T : HashAlgorithm
        {
            var bytes = hashAlgorithm.ComputeHash(buffer);
            hashAlgorithm.Dispose();
            return bytes;
        }

        internal static byte[] ComputeHash<T>(Stream inputStream, T hashAlgorithm) where T : HashAlgorithm
        {
            var bytes = hashAlgorithm.ComputeHash(inputStream);
            hashAlgorithm.Dispose();
            return bytes;
        }

        internal static string ComputeHashString<T>(byte[] buffer, T hashAlgorithm, bool isLower = def_hash_str_is_lower) where T : HashAlgorithm
        {
            var bytes = ComputeHash(buffer, hashAlgorithm);
            return ToString(bytes, isLower);
        }

        internal static string ComputeHashString<T>(Stream inputStream, T hashAlgorithm, bool isLower = def_hash_str_is_lower) where T : HashAlgorithm
        {
            var bytes = ComputeHash(inputStream, hashAlgorithm);
            return ToString(bytes, isLower);
        }

        internal static string ComputeHashString<T>(string str, T hashAlgorithm, bool isLower = def_hash_str_is_lower) where T : HashAlgorithm
        {
            var bytes = Encoding.UTF8.GetBytes(str);
            return ComputeHashString(bytes, hashAlgorithm, isLower);
        }

        public static partial class String
        {
            public static partial class Lengths { }
        }

        public static partial class ByteArray { }
    }
}