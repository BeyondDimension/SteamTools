using System.Threading.Tasks;
using Xamarin.Essentials;

namespace System.Application.Services
{
    public interface IEmailPlatformService : IService<IEmailPlatformService>
    {
        Task PlatformComposeAsync(EmailMessage? message);
    }
}