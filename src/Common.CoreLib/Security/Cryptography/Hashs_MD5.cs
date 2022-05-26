namespace System.Security.Cryptography;

partial class Hashs
{
    static MD5 CreateMD5()
    {
        try
        {
            return MD5.Create();
        }
        catch
        {
            return new MD5CryptoServiceProvider();
        }
    }

    partial class String
    {
        partial class Lengths
        {
            public const int MD5 = 32;
        }

        /// <summary>
        /// 计算MD5值
        /// </summary>
        /// <param name="text"></param>
        /// <param name="isLower"></param>
        /// <returns></returns>
        public static string MD5(string text, bool isLower = def_hash_str_is_lower) => ComputeHashString(text, CreateMD5(), isLower);

        /// <summary>
        /// 计算MD5值
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="isLower"></param>
        /// <returns></returns>
        public static string MD5(byte[] buffer, bool isLower = def_hash_str_is_lower) => ComputeHashString(buffer, CreateMD5(), isLower);

        /// <summary>
        /// 计算MD5值
        /// </summary>
        /// <param name="inputStream"></param>
        /// <param name="isLower"></param>
        /// <returns></returns>
        public static string MD5(Stream inputStream, bool isLower = def_hash_str_is_lower) => ComputeHashString(inputStream, CreateMD5(), isLower);
    }

    partial class ByteArray
    {
        /// <summary>
        /// 计算MD5值
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static byte[] MD5(byte[] buffer) => ComputeHash(buffer, CreateMD5());

        /// <summary>
        /// 计算MD5值
        /// </summary>
        /// <param name="inputStream"></param>
        /// <returns></returns>
        public static byte[] MD5(Stream inputStream) => ComputeHash(inputStream, CreateMD5());
    }
}