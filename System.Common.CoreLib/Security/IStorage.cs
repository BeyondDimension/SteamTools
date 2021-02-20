using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace System.Security
{
    /// <summary>
    /// 为键/值对提供简单的安全存储
    /// </summary>
    /// <remarks>
    /// <para>每个平台使用平台提供的本机API来安全存储数据：</para>
    /// <list type="bullet">
    ///   <item>
    ///     <term>iOS：数据存储在KeyChain中。有关SecAccessible的其他信息</term>
    ///   </item>
    ///   <item>
    ///     <term>Android：加密密钥存储在密钥库中，加密数据存储在命名的共享首选项容器中</term>
    ///   </item>
    ///   <item>
    ///     <term>UWP：数据使用 DataProtectionProvider 加密并存储在命名的 ApplicationDataContainer</term>
    ///   </item>
    ///   <item>
    ///     <term>AspNetCore：数据使用仓储实现</term>
    ///   </item>
    /// </list>
    /// <para>注意：在运行 Android 23（6.0/M）以下的 Android 设备上，KeyStore中没有AES可用。作为最佳实践，此API将生成存储在KeyStore中的RSA/ECB/PKCS7Padding密钥对（KeyStore中这些较低的API级别支持的唯一类型），用于包装运行时生成的AES密钥。这个包装好的密钥存储在首选项中</para>
    /// </remarks>
    public interface IStorage
    {
        /// <summary>
        /// 获取给定键的值
        /// </summary>
        /// <param name="key">键</param>
        /// <returns>如果键不存在，则返回null</returns>
        string? Get(string key);

        /// <inheritdoc cref="Get(string)"/>
        Task<string?> GetAsync(string key);

        /// <summary>
        /// 存储给定键的值
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        void Set(string key, string? value);

        /// <inheritdoc cref="Set(string, string?)"/>
        Task SetAsync(string key, string? value);

        /// <summary>
        /// 移除给定键的键/值对
        /// </summary>
        /// <param name="key">要移除的键</param>
        /// <returns>如果删除了键值对，则返回true</returns>
        bool Remove(string key);

        /// <inheritdoc cref="Remove(string)"/>
        Task<bool> RemoveAsync(string key);

        [return: MaybeNull]
        public static TValue Deserialize<TValue>(string? strValue)
        {
            return Serializable.DMPB64U<TValue>(strValue);
        }

        public static string? Serialize<TValue>(TValue value)
        {
            string? strValue;
            if (value is string str)
            {
                strValue = str;
            }
            else
            {
                if (value == null)
                {
                    strValue = null;
                }
                else
                {
                    strValue = Serializable.SMPB64U(value);
                }
            }
            return strValue;
        }

        /// <inheritdoc cref="Get(string)"/>
        [return: MaybeNull]
        public TValue Get<TValue>(string key)
        {
            var strValue = Get(key);
            var value = Deserialize<TValue>(strValue);
            return value;
        }

#nullable disable

        /// <inheritdoc cref="Get(string)"/>
        public async Task<TValue> GetAsync<TValue>(string key)
        {
            var strValue = await GetAsync(key);
            var value = Deserialize<TValue>(strValue);
            return value;
        }

#nullable restore

        public void Set<TValue>(string key, TValue value)
        {
            var strValue = Serialize(value);
            Set(key, strValue);
        }

        public Task SetAsync<TValue>(string key, TValue value)
        {
            var strValue = Serialize(value);
            return SetAsync(key, strValue);
        }

        public static IStorage Instance => DI.Get<IStorage>();
    }
}
