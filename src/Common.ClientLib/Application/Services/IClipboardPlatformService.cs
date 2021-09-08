using System.Threading.Tasks;

namespace System.Application.Services
{
    public interface IClipboardPlatformService
    {
        Task PlatformSetTextAsync(string text);

        Task<string> PlatformGetTextAsync();

        bool PlatformHasText { get; }

        static IClipboardPlatformService Instance => DI.Get<IClipboardPlatformService>();
    }
}