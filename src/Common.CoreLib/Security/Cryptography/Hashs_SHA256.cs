namespace System.Security.Cryptography;

partial class Hashs
{
    static SHA256 CreateSHA256()
    {
        try
        {
            return SHA256.Create();
        }
        catch
        {
            return new SHA256CryptoServiceProvider();
        }
    }

    partial class String
    {
        partial class Lengths
        {
            public const int SHA256 = 64;
        }

        /// <summary>
        /// 计算SHA256值
        /// </summary>
        /// <param name="text"></param>
        /// <param name="isLower"></param>
        /// <returns></returns>
        public static string SHA256(string text, bool isLower = def_hash_str_is_lower) => ComputeHashString(text, CreateSHA256(), isLower);

        /// <summary>
        /// 计算SHA256值
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="isLower"></param>
        /// <returns></returns>
        public static string SHA256(byte[] buffer, bool isLower = def_hash_str_is_lower) => ComputeHashString(buffer, CreateSHA256(), isLower);

        /// <summary>
        /// 计算SHA256值
        /// </summary>
        /// <param name="inputStream"></param>
        /// <param name="isLower"></param>
        /// <returns></returns>
        public static string SHA256(Stream inputStream, bool isLower = def_hash_str_is_lower) => ComputeHashString(inputStream, CreateSHA256(), isLower);
    }

    partial class ByteArray
    {
        /// <summary>
        /// 计算SHA256值
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static byte[] SHA256(byte[] buffer) => ComputeHash(buffer, CreateSHA256());

        /// <summary>
        /// 计算SHA256值
        /// </summary>
        /// <param name="inputStream"></param>
        /// <returns></returns>
        public static byte[] SHA256(Stream inputStream) => ComputeHash(inputStream, CreateSHA256());
    }
}