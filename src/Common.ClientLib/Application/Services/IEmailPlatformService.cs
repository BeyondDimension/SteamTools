using System.Threading.Tasks;
using Xamarin.Essentials;

namespace System.Application.Services
{
    public interface IEmailPlatformService
    {
        static IEmailPlatformService Instance => DI.Get<IEmailPlatformService>();

        Task PlatformComposeAsync(EmailMessage? message);
    }
}