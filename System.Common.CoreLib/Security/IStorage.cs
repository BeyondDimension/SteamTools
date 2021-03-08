using System.Text;
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
    ///     <term>UWP / Windows 10 Desktop：数据使用 DataProtectionProvider 加密并存储在命名的 ApplicationDataContainer</term>
    ///   </item>
    ///   <item>
    ///     <term>AspNetCore / Other Desktop：数据使用仓储实现</term>
    ///   </item>
    /// </list>
    /// <para>注意：在运行 Android 23（6.0/M）以下的 Android 设备上，KeyStore中没有AES可用。作为最佳实践，此API将生成存储在KeyStore中的RSA/ECB/PKCS7Padding密钥对（KeyStore中这些较低的API级别支持的唯一类型），用于包装运行时生成的AES密钥。这个包装好的密钥存储在首选项中</para>
    /// </remarks>
    public partial interface IStorage
    {
        #region Public API

        /// <summary>
        /// 获取给定键的值
        /// </summary>
        /// <param name="key">键</param>
        /// <returns>如果键不存在，则返回null</returns>
        async Task<string?> GetAsync(string key)
        {
            if (IsNativeSupportedBytes)
            {
                var bytesValue = await GetBytesAsync(key);
                if (bytesValue == null) return null;
                try
                {
                    return Encoding.UTF8.GetString(bytesValue);
                }
                catch
                {
                    return null;
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// 存储给定键的值
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <returns></returns>
        Task SetAsync(string key, string? value)
        {
            if (IsNativeSupportedBytes)
            {
                var bytesValue = value == null ? null : Encoding.UTF8.GetBytes(value);
                return SetAsync(key, bytesValue);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// 移除给定键的键/值对
        /// </summary>
        /// <param name="key">要移除的键</param>
        /// <returns>如果删除了键值对，则返回true</returns>
        Task<bool> RemoveAsync(string key);

        /// <inheritdoc cref="GetAsync(string)"/>
        async Task<TValue?> GetAsync<TValue>(string key) where TValue : notnull
        {
            if (IsNativeSupportedBytes)
            {
                var bytesValue = await GetBytesAsync(key);
                if (bytesValue == null) return default;
                var value = Serializable.DMP<TValue?>(bytesValue);
                return value;
            }
            else
            {
                var strValue = await GetAsync(key);
                if (strValue == null) return default;
                var value = Serializable.DMPB64U<TValue?>(strValue);
                return value;
            }
        }

        /// <inheritdoc cref="SetAsync(string, string?)"/>
        Task SetAsync<TValue>(string key, TValue value)
        {
            if (IsNativeSupportedBytes)
            {
                var bytesValue = SMP(value);
                return SetAsync(key, bytesValue);
            }
            else
            {
                var strValue = SMPB64U(value);
                return SetAsync(key, strValue);
            }
        }

        static IStorage Instance => DI.Get<IStorage>();

        #endregion

        protected bool IsNativeSupportedBytes { get; }

        /// <inheritdoc cref="GetAsync(string)"/>
        protected async Task<byte[]?> GetBytesAsync(string key)
        {
            if (IsNativeSupportedBytes)
            {
                throw new NotImplementedException();
            }
            else
            {
                var strValue = await GetAsync(key);
                try
                {
                    return strValue.Base64UrlDecodeToByteArray_Nullable();
                }
                catch
                {
                    return null;
                }
            }
        }

        /// <inheritdoc cref="SetAsync(string, string?)"/>
        protected Task SetAsync(string key, byte[]? value)
        {
            if (IsNativeSupportedBytes)
            {
                throw new NotImplementedException();
            }
            else
            {
                var strValue = value.Base64UrlEncode_Nullable();
                return SetAsync(key, strValue);
            }
        }

        private static string? SMPB64U<TValue>(TValue value)
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

        private static byte[]? SMP<TValue>(TValue value)
        {
            byte[]? bytesValue;
            if (value is byte[] bytes)
            {
                bytesValue = bytes;
            }
            else
            {
                if (value == null)
                {
                    bytesValue = null;
                }
                else
                {
                    bytesValue = Serializable.SMP(value);
                }
            }
            return bytesValue;
        }
    }
}