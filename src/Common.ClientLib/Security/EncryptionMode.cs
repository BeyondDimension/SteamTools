namespace System.Security
{
    /// <summary>
    /// 加密模式
    /// </summary>
    public enum EncryptionMode : byte
    {
        /// <summary>
        /// 嵌入 AES
        /// </summary>
        EmbeddedAes = 1,

        /// <summary>
        /// 嵌入 AES + 本机
        /// </summary>
        EmbeddedAesWithLocal,

        /// <summary>
        /// 嵌入 AES + 二级密码
        /// </summary>
        EmbeddedAesWithSecondaryPassword,

        /// <summary>
        /// 嵌入 AES + 二级密码 + 本机
        /// </summary>
        EmbeddedAesWithSecondaryPasswordWithLocal,
    }
}