using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Text;
using System.Diagnostics.CodeAnalysis;

namespace System.Application.Services.Implementation
{
    /// <inheritdoc cref="ISecurityService"/>
    public abstract class SecurityServiceBase : ISecurityService
    {
        public abstract Aes? Aes { get; }

        public byte[]? E_(string? value)
        {
            var aes = Aes;
            if (aes != null)
            {
                return AESUtils.EncryptToByteArray_Nullable(aes, value);
            }
            else
            {
                if (value == null) return null;
                return Encoding.UTF8.GetBytes(value);
            }
        }

        public byte[]? EB_(byte[]? value)
        {
            var aes = Aes;
            if (aes != null)
            {
                return AESUtils.Encrypt_Nullable(aes, value);
            }
            else
            {
                return value;
            }
        }

        public string? D_(byte[]? value)
        {
            var aes = Aes;
            if (aes != null)
            {
                try
                {
                    return AESUtils.DecryptToString_Nullable(aes, value);
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

        public byte[]? DB_(byte[]? value)
        {
            var aes = Aes;
            if (aes != null)
            {
                try
                {
                    return AESUtils.Decrypt_Nullable(aes, value);
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

        public virtual Task<byte[]?> E(string? value) => Task.FromResult(E_(value));

        public virtual Task<byte[]?> EB(byte[]? value) => Task.FromResult(EB_(value));

        public virtual Task<string?> D(byte[]? value) => Task.FromResult(D_(value));

        public virtual Task<byte[]?> DB(byte[]? value) => Task.FromResult(DB_(value));
    }
}