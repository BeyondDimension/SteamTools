using System.IO;

namespace System.Security.Cryptography
{
    partial class Hashs
    {
        static SHA384 CreateSHA384()
        {
            try
            {
                return SHA384.Create();
            }
            catch
            {
                return new SHA384CryptoServiceProvider();
            }
        }

        partial class String
        {
            partial class Lengths
            {
                public const int SHA384 = 96;
            }

            /// <summary>
            /// 计算SHA384值
            /// </summary>
            /// <param name="text"></param>
            /// <param name="isLower"></param>
            /// <returns></returns>
            public static string SHA384(string text, bool isLower = def_hash_str_is_lower) => ComputeHashString(text, CreateSHA384(), isLower);

            /// <summary>
            /// 计算SHA384值
            /// </summary>
            /// <param name="buffer"></param>
            /// <param name="isLower"></param>
            /// <returns></returns>
            public static string SHA384(byte[] buffer, bool isLower = def_hash_str_is_lower) => ComputeHashString(buffer, CreateSHA384(), isLower);

            /// <summary>
            /// 计算SHA384值
            /// </summary>
            /// <param name="inputStream"></param>
            /// <param name="isLower"></param>
            /// <returns></returns>
            public static string SHA384(Stream inputStream, bool isLower = def_hash_str_is_lower) => ComputeHashString(inputStream, CreateSHA384(), isLower);
        }

        partial class ByteArray
        {
            /// <summary>
            /// 计算SHA384值
            /// </summary>
            /// <param name="buffer"></param>
            /// <returns></returns>
            public static byte[] SHA384(byte[] buffer) => ComputeHash(buffer, CreateSHA384());

            /// <summary>
            /// 计算SHA384值
            /// </summary>
            /// <param name="inputStream"></param>
            /// <returns></returns>
            public static byte[] SHA384(Stream inputStream) => ComputeHash(inputStream, CreateSHA384());
        }
    }
}