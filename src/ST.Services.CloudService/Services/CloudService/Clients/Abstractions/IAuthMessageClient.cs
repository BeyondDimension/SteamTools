using System.Application.Models;
using System.Threading.Tasks;

namespace System.Application.Services.CloudService.Clients.Abstractions
{
    public interface IAuthMessageClient
    {
        ValueTask<IApiResponse> SendSms(SendSmsRequest request);
    }
}