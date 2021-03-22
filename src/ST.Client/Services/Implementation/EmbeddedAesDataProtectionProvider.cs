using Microsoft.Extensions.Options;
using System.Application.Models;
using System.Application.Security;
using System.Security.Cryptography;

namespace System.Application.Services.Implementation
{
    /// <inheritdoc cref="EmbeddedAesDataProtectionProviderBase"/>
    public class EmbeddedAesDataProtectionProvider : EmbeddedAesDataProtectionProviderBase
    {
        protected readonly AppSettings settings;

        public EmbeddedAesDataProtectionProvider(IOptions<AppSettings> options)
        {
            settings = options.Value;
        }

        Aes[]? aes;
        bool isCallGetAes;

        public override Aes[]? Aes
        {
            get
            {
                if (aes != null)
                {
                    return aes;
                }
                else if (isCallGetAes)
                {
                    return null;
                }
                try
                {
                    aes = new[] { settings.Aes };
                    return aes;
                }
                catch (IsNotOfficialChannelPackageException e)
                {
                    isCallGetAes = true;
                    Log.Error(nameof(EmbeddedAesDataProtectionProvider), e,
                        nameof(ApiResponseCode.IsNotOfficialChannelPackage));
                    return null;
                }
            }
        }
    }
}