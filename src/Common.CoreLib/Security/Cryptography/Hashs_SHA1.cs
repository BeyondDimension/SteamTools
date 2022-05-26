namespace System.Security.Cryptography;

partial class Hashs
{
    static SHA1 CreateSHA1()
    {
        try
        {
            return SHA1.Create();
        }
        catch
        {
            return new SHA1CryptoServiceProvider();
        }
    }

    partial class String
    {
        partial class Lengths
        {
            public const int SHA1 = 40;
        }

        /// <summary>
        /// 计算SHA1值
        /// </summary>
        /// <param name="text"></param>
        /// <param name="isLower"></param>
        /// <returns></returns>
        public static string SHA1(string text, bool isLower = def_hash_str_is_lower) => ComputeHashString(text, CreateSHA1(), isLower);

        /// <summary>
        /// 计算SHA1值
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="isLower"></param>
        /// <returns></returns>
        public static string SHA1(byte[] buffer, bool isLower = def_hash_str_is_lower) => ComputeHashString(buffer, CreateSHA1(), isLower);

        /// <summary>
        /// 计算SHA1值
        /// </summary>
        /// <param name="inputStream"></param>
        /// <param name="isLower"></param>
        /// <returns></returns>
        public static string SHA1(Stream inputStream, bool isLower = def_hash_str_is_lower) => ComputeHashString(inputStream, CreateSHA1(), isLower);
    }

    partial class ByteArray
    {
        /// <summary>
        /// 计算SHA1值
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static byte[] SHA1(byte[] buffer) => ComputeHash(buffer, CreateSHA1());

        /// <summary>
        /// 计算SHA1值
        /// </summary>
        /// <param name="inputStream"></param>
        /// <returns></returns>
        public static byte[] SHA1(Stream inputStream) => ComputeHash(inputStream, CreateSHA1());
    }
}