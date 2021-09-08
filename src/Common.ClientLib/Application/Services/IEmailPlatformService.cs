using System.Threading.Tasks;
using Xamarin.Essentials;

namespace System.Application.Services
{
    public interface IEmailPlatformService
    {
        Task PlatformComposeAsync(EmailMessage? message);

        static IEmailPlatformService? Instance => DI.Get_Nullable<IEmailPlatformService>();
    }
}