using Microsoft.Extensions.Options;
using System.Application.Models;
using System.Application.Security;
using System.Security.Cryptography;

namespace System.Application.Services.Implementation
{
    /// <inheritdoc cref="ISecurityService"/>
    public class SecurityService : SecurityServiceBase
    {
        protected readonly AppSettings settings;

        public SecurityService(IOptions<AppSettings> options)
        {
            settings = options.Value;
        }

        public override Aes? Aes
        {
            get
            {
                try
                {
                    return settings.Aes;
                }
                catch (IsNotOfficialChannelPackageException e)
                {
                    Log.Error(nameof(SecurityService), e, nameof(ApiResponseCode.IsNotOfficialChannelPackage));
                    return null;
                }
            }
        }
    }
}