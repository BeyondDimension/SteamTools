using Microsoft.Extensions.Options;
using System.Application.Models;
using System.Security.Cryptography;

namespace System.Application.Services.Implementation
{
    /// <inheritdoc cref="ISecurityService"/>
    public class SecurityService : ISecurityService
    {
        protected readonly AppSettings settings;

        public SecurityService(IOptions<AppSettings> options)
        {
            settings = options.Value;
        }

        public virtual byte[]? E(string? value) => AESUtils.EncryptToByteArray_Nullable(settings.Aes, value);

        public virtual byte[]? EB(byte[]? value) => AESUtils.Encrypt_Nullable(settings.Aes, value);

        public virtual string? D(byte[]? value) => AESUtils.DecryptToString_Nullable(settings.Aes, value);

        public virtual byte[]? DB(byte[]? value) => AESUtils.Decrypt_Nullable(settings.Aes, value);
    }
}