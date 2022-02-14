using System.Security.Cryptography;

namespace System.Application.Models
{
    public interface ICloudServiceSettings
    {
        string? ApiBaseUrl { get; set; }

        [Obsolete("Delete", true)]
        Guid AppVersion { get; set; }

        RSA RSA { get; }

        [Obsolete("Delete", true)]
        string AppVersionStr => throw new NotImplementedException();
        //=> AppVersion.ToStringN();
    }
}