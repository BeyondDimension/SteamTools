using System.Application.Services.CloudService;

namespace System.Application.Models
{
    public interface ICloudServiceSettings
    {
        string? ApiBaseUrl { get; set; }

        Guid AppVersion { get; set; }
    }
}