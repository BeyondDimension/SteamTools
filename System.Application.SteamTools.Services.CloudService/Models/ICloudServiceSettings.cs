using System.Security.Cryptography;

namespace System.Application.Models
{
    public interface ICloudServiceSettings
    {
        string? ApiBaseUrl { get; set; }

        Guid AppVersion { get; set; }

        RSA RSA { get; }
    }
}