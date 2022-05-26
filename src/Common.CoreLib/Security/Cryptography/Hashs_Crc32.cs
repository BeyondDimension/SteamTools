using Force.Crc32;

namespace System.Security.Cryptography;

partial class Hashs
{
    static Crc32Algorithm CreateCrc32()
    {
        return new Crc32Algorithm();
    }

    partial class String
    {
        partial class Lengths
        {
            public const int Crc32 = 8;
        }

        /// <summary>
        /// 计算Crc32值
        /// </summary>
        /// <param name="text"></param>
        /// <param name="isLower"></param>
        /// <returns></returns>
        public static string Crc32(string text, bool isLower = def_hash_str_is_lower) => ComputeHashString(text, CreateCrc32(), isLower);

        /// <summary>
        /// 计算Crc32值
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="isLower"></param>
        /// <returns></returns>
        public static string Crc32(byte[] buffer, bool isLower = def_hash_str_is_lower) => ComputeHashString(buffer, CreateCrc32(), isLower);

        /// <summary>
        /// 计算Crc32值
        /// </summary>
        /// <param name="inputStream"></param>
        /// <param name="isLower"></param>
        /// <returns></returns>
        public static string Crc32(Stream inputStream, bool isLower = def_hash_str_is_lower) => ComputeHashString(inputStream, CreateCrc32(), isLower);
    }

    partial class ByteArray
    {
        /// <summary>
        /// 计算Crc32值
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static byte[] Crc32(byte[] buffer) => ComputeHash(buffer, CreateCrc32());

        /// <summary>
        /// 计算Crc32值
        /// </summary>
        /// <param name="inputStream"></param>
        /// <returns></returns>
        public static byte[] Crc32(Stream inputStream) => ComputeHash(inputStream, CreateCrc32());
    }
}