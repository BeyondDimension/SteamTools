using System.Threading.Tasks;

namespace System.Application.Services
{
    public interface IClipboardPlatformService : IService<IClipboardPlatformService>
    {
        Task PlatformSetTextAsync(string text);

        Task<string> PlatformGetTextAsync();

        bool PlatformHasText { get; }
    }
}