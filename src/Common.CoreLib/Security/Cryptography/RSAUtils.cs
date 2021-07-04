using System.Text;
using N_JsonProperty = Newtonsoft.Json.JsonPropertyAttribute;
using S_JsonProperty = System.Text.Json.Serialization.JsonPropertyNameAttribute;

namespace System.Security.Cryptography
{
    /// <summary>
    /// 非对称加密算法 - RSA
    /// </summary>
    public static partial class RSAUtils
    {
        #region FromJsonString

        static RSAParameters GetRSAParametersFromJsonString(string jsonString)
        {
            var rsaParams = Serializable.DJSON<Parameters>(jsonString);
            if (rsaParams == null) throw new NullReferenceException(nameof(rsaParams));
            return rsaParams.ToStruct();
        }

        /// <summary>
        /// 通过 JSON 字符串中的密钥信息初始化 RSA 对象。
        /// </summary>
        /// <param name="rsa"></param>
        /// <param name="jsonString"></param>
        public static void FromJsonString(RSA rsa, string jsonString)
        {
            if (rsa == null) throw new ArgumentNullException(nameof(rsa));
            var rsaParams = GetRSAParametersFromJsonString(jsonString);
            rsa.ImportParameters(rsaParams);
        }

        #endregion

        #region FromXmlString

        static string? SearchForTextOfLocalName(string str, string name)
        {
            if (name == null) return null;
            var leftStr = $"<{name}>";
            var rightStr = $"</{name}>";
            var leftIndex = str.IndexOf(leftStr, StringComparison.OrdinalIgnoreCase);
            if (leftIndex < 0) return null;
            var rightIndex = str.IndexOf(rightStr, StringComparison.OrdinalIgnoreCase);
            var startIndex = leftIndex + leftStr.Length;
            var length = rightIndex - (leftIndex + leftStr.Length);
            return str.Substring(startIndex, length);
        }

        static RSAParameters GetRSAParametersFromXmlString(string xmlString)
        {
            var rsaParams = new RSAParameters();
            var modulusString = SearchForTextOfLocalName(xmlString, "Modulus");
            if (modulusString == null)
                throw new CryptographicException("Cryptography_InvalidFromXmlString_RSA_Modulus");
            rsaParams.Modulus = modulusString.Base64DecodeToByteArray_Nullable();
            var exponentString = SearchForTextOfLocalName(xmlString, "Exponent");
            if (exponentString == null)
                throw new CryptographicException("Cryptography_InvalidFromXmlString_RSA_Exponent");
            rsaParams.Exponent = exponentString.Base64DecodeToByteArray_Nullable();
            var pString = SearchForTextOfLocalName(xmlString, "P");
            if (pString != null)
                rsaParams.P = pString.Base64DecodeToByteArray_Nullable();
            var qString = SearchForTextOfLocalName(xmlString, "Q");
            if (qString != null)
                rsaParams.Q = qString.Base64DecodeToByteArray_Nullable();
            var dpString = SearchForTextOfLocalName(xmlString, "DP");
            if (dpString != null)
                rsaParams.DP = dpString.Base64DecodeToByteArray_Nullable();
            var dqString = SearchForTextOfLocalName(xmlString, "DQ");
            if (dqString != null)
                rsaParams.DQ = dqString.Base64DecodeToByteArray_Nullable();
            var inverseQString = SearchForTextOfLocalName(xmlString, "InverseQ");
            if (inverseQString != null)
                rsaParams.InverseQ = inverseQString.Base64DecodeToByteArray_Nullable();
            var dString = SearchForTextOfLocalName(xmlString, "D");
            if (dString != null) rsaParams.D = dString.Base64DecodeToByteArray_Nullable();
            return rsaParams;
        }

        /// <summary>
        /// 通过 XML 字符串中的密钥信息初始化 RSA 对象。
        /// </summary>
        /// <param name="rsa"></param>
        /// <param name="xmlString"></param>
        public static void FromXmlString(RSA rsa, string xmlString)
        {
            if (rsa == null) throw new ArgumentNullException(nameof(rsa));
            var rsaParams = GetRSAParametersFromXmlString(xmlString);
            rsa.ImportParameters(rsaParams);
        }

        #endregion

        #region 创建 RSA 实例

        /// <summary>
        /// 创建 <see cref="RSA" /> 对象。
        /// </summary>
        /// <param name="xmlString"></param>
        /// <returns></returns>
        public static RSA CreateFromXmlString(string xmlString)
        {
            var rsaParams = GetRSAParametersFromXmlString(xmlString);
            var rsa = RSA.Create(rsaParams);
            return rsa;
        }

        /// <summary>
        /// 创建 <see cref="RSA" /> 对象。
        /// </summary>
        /// <param name="jsonString"></param>
        /// <returns></returns>
        public static RSA CreateFromJsonString(string jsonString)
        {
            var rsaParams = GetRSAParametersFromJsonString(jsonString);
            var rsa = RSA.Create(rsaParams);
            return rsa;
        }

        #endregion

        #region 从 RSA 实例中获取密钥

        /// <summary>
        /// 创建并返回包含当前 <see cref="RSA"/> 对象的密钥的 JSON 字符串。
        /// </summary>
        /// <param name="rsa"></param>
        /// <param name="includePrivateParameters"><see langword="true" /> 表示同时包含 RSA 公钥和私钥；<see langword="false" /> 表示仅包含公钥。</param>
        /// <returns>包含当前 <see cref="RSA"/> 对象的密钥的 JSON 字符串。</returns>
        public static string ToJsonString(this RSA rsa, bool includePrivateParameters)
        {
            var rsaParams = rsa.ExportParameters(includePrivateParameters).ToObject();
            var jsonString = Serializable.SJSON(rsaParams, ignoreNullValues: true);
            return jsonString;
        }

        #endregion

        #region Padding

        /// <summary>
        /// RSA 填充，不可更改此值！
        /// </summary>
        [Obsolete]
        internal static RSAEncryptionPadding Padding => RSAEncryptionPadding.OaepSHA256;

        public static RSAEncryptionPadding CreateOaep(string oaepHashAlgorithmName)
        {
            try
            {
                var hashAlgorithm = new HashAlgorithmName(oaepHashAlgorithmName);
                return RSAEncryptionPadding.CreateOaep(hashAlgorithm);
            }
            catch
            {
                return Padding;
            }
        }

        public static RSAEncryptionPadding DefaultPadding
        {
            get
            {
                if (DI.IsRunningOnMono)
                {
                    return RSAEncryptionPadding.OaepSHA1;
                }
                else
                {
                    return RSAEncryptionPadding.OaepSHA256;
                }
            }
        }

        #endregion

        #region 加密(Encrypt)

        /// <summary>
        /// RSA加密(ByteArray → ByteArray)
        /// </summary>
        /// <param name="rsa"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        [Obsolete()]
        public static byte[] Encrypt(this RSA rsa, byte[] data) => rsa.Encrypt(data, Padding);

        /// <summary>
        /// RSA加密(ByteArray → ByteArray)
        /// </summary>
        /// <param name="rsa"></param>
        /// <param name="data"></param>
        /// <param name="padding"></param>
        /// <returns></returns>
        public static byte[]? Encrypt_Nullable(this RSA rsa, byte[]? data, RSAEncryptionPadding? padding = null)
        {
            if (data == default) return default;
            padding ??= DefaultPadding;
            return rsa.Encrypt(data, padding);
        }

        /// <summary>
        /// RSA加密(String → ByteArray)
        /// </summary>
        /// <param name="rsa"></param>
        /// <param name="text"></param>
        /// <param name="padding"></param>
        /// <returns></returns>
        public static byte[] EncryptToByteArray(this RSA rsa, string text, RSAEncryptionPadding? padding = null)
        {
            var data = Encoding.UTF8.GetBytes(text);
            padding ??= DefaultPadding;
            return rsa.Encrypt(data, padding);
        }

        /// <summary>
        /// RSA加密(String → ByteArray)
        /// </summary>
        /// <param name="rsa"></param>
        /// <param name="text"></param>
        /// <param name="padding"></param>
        /// <returns></returns>
        public static byte[]? EncryptToByteArray_Nullable(this RSA rsa, string? text, RSAEncryptionPadding? padding = null)
        {
            if (text == default) return default;
            return EncryptToByteArray(rsa, text, padding);
        }

        /// <summary>
        /// RSA加密(String → String)
        /// </summary>
        /// <param name="rsa"></param>
        /// <param name="text"></param>
        /// <param name="padding"></param>
        /// <returns></returns>
        public static string Encrypt(this RSA rsa, string text, RSAEncryptionPadding? padding = null)
        {
            var bytes = EncryptToByteArray(rsa, text, padding);
            return bytes.Base64UrlEncode();
        }

        /// <summary>
        /// RSA加密(String → String)
        /// </summary>
        /// <param name="rsa"></param>
        /// <param name="text"></param>
        /// <param name="padding"></param>
        /// <returns></returns>
        public static string? Encrypt_Nullable(this RSA rsa, string? text, RSAEncryptionPadding? padding = null)
        {
            if (text == default) return default;
            return Encrypt(rsa, text, padding);
        }

        /// <summary>
        /// RSA加密(ByteArray → String)
        /// </summary>
        /// <param name="rsa"></param>
        /// <param name="data"></param>
        /// <param name="padding"></param>
        /// <returns></returns>
        public static string EncryptToString(this RSA rsa, byte[] data, RSAEncryptionPadding? padding = null)
        {
            padding ??= DefaultPadding;
            var bytes = rsa.Encrypt(data, padding);
            return bytes.Base64UrlEncode();
        }

        /// <summary>
        /// RSA加密(ByteArray → String)
        /// </summary>
        /// <param name="rsa"></param>
        /// <param name="data"></param>
        /// <param name="padding"></param>
        /// <returns></returns>
        public static string? EncryptToString_Nullable(this RSA rsa, byte[]? data, RSAEncryptionPadding? padding = null)
        {
            if (data == default) return default;
            return EncryptToString(rsa, data, padding);
        }

        #endregion

        #region 解密(Decrypt)

        /// <summary>
        /// RSA解密(ByteArray → ByteArray)
        /// </summary>
        /// <param name="rsa"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        [Obsolete]
        public static byte[] Decrypt(this RSA rsa, byte[] data) => rsa.Decrypt(data, Padding);

        /// <summary>
        /// RSA解密(ByteArray → ByteArray)
        /// </summary>
        /// <param name="rsa"></param>
        /// <param name="data"></param>
        /// <param name="padding"></param>
        /// <returns></returns>
        public static byte[]? Decrypt_Nullable(this RSA rsa, byte[]? data, RSAEncryptionPadding? padding = null)
        {
            if (data == default) return default;
            padding ??= DefaultPadding;
            return rsa.Decrypt(data, padding);
        }

        /// <summary>
        /// RSA解密(String → ByteArray)
        /// </summary>
        /// <param name="rsa"></param>
        /// <param name="text"></param>
        /// <param name="padding"></param>
        /// <returns></returns>
        public static byte[] DecryptToByteArray(this RSA rsa, string text, RSAEncryptionPadding? padding = null)
        {
            var bytes = text.Base64UrlDecodeToByteArray();
            padding ??= DefaultPadding;
            return rsa.Decrypt(bytes, padding);
        }

        /// <summary>
        /// RSA解密(String → ByteArray)
        /// </summary>
        /// <param name="rsa"></param>
        /// <param name="text"></param>
        /// <param name="padding"></param>
        /// <returns></returns>
        public static byte[]? DecryptToByteArray_Nullable(this RSA rsa, string? text, RSAEncryptionPadding? padding = null)
        {
            if (text == default) return default;
            return DecryptToByteArray(rsa, text, padding);
        }

        /// <summary>
        /// RSA解密(ByteArray → String)
        /// </summary>
        /// <param name="rsa"></param>
        /// <param name="data"></param>
        /// <param name="padding"></param>
        /// <returns></returns>
        public static string DecryptToString(this RSA rsa, byte[] data, RSAEncryptionPadding? padding = null)
        {
            padding ??= DefaultPadding;
            var bytes = rsa.Decrypt(data, padding);
            return Encoding.UTF8.GetString(bytes);
        }

        /// <summary>
        /// RSA解密(ByteArray → String)
        /// </summary>
        /// <param name="rsa"></param>
        /// <param name="data"></param>
        /// <param name="padding"></param>
        /// <returns></returns>
        public static string? DecryptToString_Nullable(this RSA rsa, byte[]? data, RSAEncryptionPadding? padding = null)
        {
            if (data == default) return default;
            return DecryptToString(rsa, data, padding);
        }

        /// <summary>
        /// RSA解密(String → String)
        /// </summary>
        /// <param name="rsa"></param>
        /// <param name="text"></param>
        /// <param name="padding"></param>
        /// <returns></returns>
        public static string Decrypt(this RSA rsa, string text, RSAEncryptionPadding? padding = null)
        {
            var result = DecryptToByteArray(rsa, text, padding);
            return Encoding.UTF8.GetString(result);
        }

        /// <summary>
        /// RSA解密(String → String)
        /// </summary>
        /// <param name="rsa"></param>
        /// <param name="text"></param>
        /// <param name="padding"></param>
        /// <returns></returns>
        public static string? Decrypt_Nullable(this RSA rsa, string? text, RSAEncryptionPadding? padding = null)
        {
            if (text == default) return default;
            return Decrypt(rsa, text, padding);
        }

        #endregion

        /// <summary>
        /// <see cref="RSA"/> 密钥 模型类
        /// <para>可使用 JSON 序列化此模型，不支持 MessagePack</para>
        /// </summary>
        internal sealed class Parameters
        {
            /// <summary>
            /// Represents the D parameter for the <see cref="T:System.Security.Cryptography.RSA"></see> algorithm.
            /// </summary>
            [N_JsonProperty("z")]
            [S_JsonProperty("z")]
            public string? D { get; set; }

            /// <summary>
            /// Represents the DP parameter for the <see cref="T:System.Security.Cryptography.RSA"></see> algorithm.
            /// </summary>
            [N_JsonProperty("x")]
            [S_JsonProperty("x")]
            public string? DP { get; set; }

            /// <summary>
            /// Represents the DQ parameter for the <see cref="T:System.Security.Cryptography.RSA"></see> algorithm.
            /// </summary>
            [N_JsonProperty("c")]
            [S_JsonProperty("c")]
            public string? DQ { get; set; }

            /// Represents the Exponent parameter for the <see cref="T:System.Security.Cryptography.RSA"></see> algorithm.
            /// </summary>
            [N_JsonProperty("v")]
            [S_JsonProperty("v")]
            public string? Exponent { get; set; }

            /// <summary>
            /// Represents the InverseQ parameter for the <see cref="T:System.Security.Cryptography.RSA"></see> algorithm.
            /// </summary>
            [N_JsonProperty("b")]
            [S_JsonProperty("b")]
            public string? InverseQ { get; set; }

            /// <summary>
            /// Represents the Modulus parameter for the <see cref="T:System.Security.Cryptography.RSA"></see> algorithm.
            /// </summary>
            [N_JsonProperty("n")]
            [S_JsonProperty("n")]
            public string? Modulus { get; set; }

            /// <summary>
            /// Represents the P parameter for the <see cref="T:System.Security.Cryptography.RSA"></see> algorithm.
            /// </summary>
            [N_JsonProperty("m")]
            [S_JsonProperty("m")]
            public string? P { get; set; }

            /// <summary>
            /// Represents the Q parameter for the <see cref="T:System.Security.Cryptography.RSA"></see> algorithm.
            /// </summary>
            [N_JsonProperty("a")]
            [S_JsonProperty("a")]
            public string? Q { get; set; }

            public bool Equals(Parameters? other)
            {
                if (other == null) return false;
                return string.Equals(D, other.D, StringComparison.Ordinal) &&
                    string.Equals(DP, other.DP, StringComparison.Ordinal) &&
                    string.Equals(DQ, other.DQ, StringComparison.Ordinal) &&
                    string.Equals(Exponent, other.Exponent, StringComparison.Ordinal) &&
                    string.Equals(InverseQ, other.InverseQ, StringComparison.Ordinal) &&
                    string.Equals(Modulus, other.Modulus, StringComparison.Ordinal) &&
                    string.Equals(P, other.P, StringComparison.Ordinal) &&
                    string.Equals(Q, other.Q, StringComparison.Ordinal);
            }
        }

        static RSAParameters ToStruct(this Parameters parms) => new RSAParameters
        {
            D = parms.D.Base64UrlDecodeToByteArray_Nullable(),
            DP = parms.DP.Base64UrlDecodeToByteArray_Nullable(),
            DQ = parms.DQ.Base64UrlDecodeToByteArray_Nullable(),
            Exponent = parms.Exponent.Base64UrlDecodeToByteArray_Nullable(),
            InverseQ = parms.InverseQ.Base64UrlDecodeToByteArray_Nullable(),
            Modulus = parms.Modulus.Base64UrlDecodeToByteArray_Nullable(),
            P = parms.P.Base64UrlDecodeToByteArray_Nullable(),
            Q = parms.Q.Base64UrlDecodeToByteArray_Nullable()
        };

        static Parameters ToObject(this RSAParameters parms) => new Parameters
        {
            D = parms.D.Base64UrlEncode_Nullable(),
            DP = parms.DP.Base64UrlEncode_Nullable(),
            DQ = parms.DQ.Base64UrlEncode_Nullable(),
            Exponent = parms.Exponent.Base64UrlEncode_Nullable(),
            InverseQ = parms.InverseQ.Base64UrlEncode_Nullable(),
            Modulus = parms.Modulus.Base64UrlEncode_Nullable(),
            P = parms.P.Base64UrlEncode_Nullable(),
            Q = parms.Q.Base64UrlEncode_Nullable()
        };
    }
}