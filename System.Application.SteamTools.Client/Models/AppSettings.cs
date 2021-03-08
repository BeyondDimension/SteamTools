using System.Security.Cryptography;
using MPIgnore = MessagePack.IgnoreMemberAttribute;
using MPKey = MessagePack.KeyAttribute;
using MPObj = MessagePack.MessagePackObjectAttribute;
using N_JsonIgnore = Newtonsoft.Json.JsonIgnoreAttribute;
using N_JsonProperty = Newtonsoft.Json.JsonPropertyAttribute;
using S_JsonIgnore = System.Text.Json.Serialization.JsonIgnoreAttribute;
using S_JsonProperty = System.Text.Json.Serialization.JsonPropertyNameAttribute;

namespace System.Application.Models
{
    /// <summary>
    /// Steam++ 应用配置项
    /// </summary>
    [MPObj]
    public sealed class AppSettings : IAppSettings, ICloudServiceSettings
    {
        [MPKey(0)]
        [N_JsonProperty("0")]
        [S_JsonProperty("0")]
        public Guid AppVersion { get; set; }

        [MPKey(1)]
        [N_JsonProperty("1")]
        [S_JsonProperty("1")]
        public string? ApiBaseUrl { get; set; }

        [MPKey(2)]
        [N_JsonProperty("2")]
        [S_JsonProperty("2")]
        public string? AesSecret { get; set; }

        Aes? aes;

        [MPIgnore]
        [N_JsonIgnore]
        [S_JsonIgnore]
        public Aes Aes
        {
            get
            {
                if (aes == null)
                {
                    if (AesSecret == null) throw new ArgumentNullException(nameof(AesSecret));
                    aes = AESUtils.Create(AesSecret);
                }
                return aes;
            }
        }

        [MPKey(3)]
        [N_JsonProperty("3")]
        [S_JsonProperty("3")]
        public string? AppSecretVisualStudioAppCenter { get; set; }

        [MPKey(4)]
        [N_JsonProperty("4")]
        [S_JsonProperty("4")]
        public string? RSASecret { get; set; }

        RSA? rsa;

        [MPIgnore]
        [N_JsonIgnore]
        [S_JsonIgnore]
        public RSA RSA
        {
            get
            {
                if (rsa == null)
                {
                    if (RSASecret == null) throw new ArgumentNullException(nameof(RSASecret));
                    rsa = RSAUtils.CreateFromJsonString(RSASecret);
                }
                return rsa;
            }
        }
    }
}