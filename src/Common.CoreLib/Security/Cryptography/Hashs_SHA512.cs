using System.IO;

namespace System.Security.Cryptography
{
    partial class Hashs
    {
        static SHA512 CreateSHA512()
        {
            try
            {
                return SHA512.Create();
            }
            catch
            {
                return new SHA512CryptoServiceProvider();
            }
        }

        partial class String
        {
            partial class Lengths
            {
                public const int SHA512 = 128;
            }

            /// <summary>
            /// 计算SHA512值
            /// </summary>
            /// <param name="text"></param>
            /// <param name="isLower"></param>
            /// <returns></returns>
            public static string SHA512(string text, bool isLower = def_hash_str_is_lower) => ComputeHashString(text, CreateSHA512(), isLower);

            /// <summary>
            /// 计算SHA512值
            /// </summary>
            /// <param name="buffer"></param>
            /// <param name="isLower"></param>
            /// <returns></returns>
            public static string SHA512(byte[] buffer, bool isLower = def_hash_str_is_lower) => ComputeHashString(buffer, CreateSHA512(), isLower);

            /// <summary>
            /// 计算SHA512值
            /// </summary>
            /// <param name="inputStream"></param>
            /// <param name="isLower"></param>
            /// <returns></returns>
            public static string SHA512(Stream inputStream, bool isLower = def_hash_str_is_lower) => ComputeHashString(inputStream, CreateSHA512(), isLower);
        }

        partial class ByteArray
        {
            /// <summary>
            /// 计算SHA512值
            /// </summary>
            /// <param name="buffer"></param>
            /// <returns></returns>
            public static byte[] SHA512(byte[] buffer) => ComputeHash(buffer, CreateSHA512());

            /// <summary>
            /// 计算SHA512值
            /// </summary>
            /// <param name="inputStream"></param>
            /// <returns></returns>
            public static byte[] SHA512(Stream inputStream) => ComputeHash(inputStream, CreateSHA512());
        }
    }
}