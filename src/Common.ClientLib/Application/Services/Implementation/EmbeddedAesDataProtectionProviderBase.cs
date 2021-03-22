using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace System.Application.Services.Implementation
{
    /// <inheritdoc cref="IEmbeddedAesDataProtectionProvider"/>
    public abstract class EmbeddedAesDataProtectionProviderBase : IEmbeddedAesDataProtectionProvider
    {
        public abstract Aes[]? Aes { get; }

        static byte[] E___(Aes[] aes, byte[] value)
        {
            if (value.Length == 0) return value;
            var len = aes.Length - 1;
            var data_e = AESUtils.Encrypt(aes[len], value);
            var data_r = BitConverter.GetBytes(len).Concat(data_e).ToArray();
            return data_r;
        }

        public byte[]? E(string? value)
        {
            var aes = Aes;
            if (aes != null && aes.Any())
            {
                var value2 = Encoding.UTF8.GetBytes(value);
                return E___(aes, value2);
            }
            else
            {
                if (value == null) return null;
                return Encoding.UTF8.GetBytes(value);
            }
        }

        public byte[]? EB(byte[]? value)
        {
            var aes = Aes;
            if (aes != null && aes.Any())
            {
                if (value == null) return null;
                return E___(aes, value);
            }
            else
            {
                return value;
            }
        }

        static byte[]? D___(Aes[] aes, byte[]? value)
        {
            if (value == null) return null;
            if (value.Length == 0) return value;
            if (value.Length <= sizeof(int)) return null;
            var len = BitConverter.ToInt32(value, 0);
            using var transform = aes[len].CreateDecryptor();
            var data_r = transform.TransformFinalBlock(value, sizeof(int), value.Length - sizeof(int));
            return data_r;
        }

        public string? D(byte[]? value)
        {
            var aes = Aes;
            if (aes != null && aes.Any())
            {
                try
                {
                    var data_r = D___(aes, value);
                    if (data_r != null)
                    {
                        return Encoding.UTF8.GetString(data_r);
                    }
                    else
                    {
                        return null;
                    }
                }
                catch
                {
                    return null;
                }
            }
            else
            {
                if (value == null) return null;
                return Encoding.UTF8.GetString(value);
            }
        }

        public byte[]? DB(byte[]? value)
        {
            var aes = Aes;
            if (aes != null && aes.Any())
            {
                try
                {
                    var data_r = D___(aes, value);
                    return data_r;
                }
                catch
                {
                    return null;
                }
            }
            else
            {
                return value;
            }
        }
    }
}