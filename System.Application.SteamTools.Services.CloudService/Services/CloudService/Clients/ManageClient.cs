using System.Application.Models;
using System.Application.Services.CloudService.Clients.Abstractions;
using System.Net.Http;
using System.Threading.Tasks;

namespace System.Application.Services.CloudService.Clients
{
    internal sealed class ManageClient : ApiClient, IManageClient
    {
        public ManageClient(IApiConnection conn) : base(conn)
        {
        }

        public Task<IApiResponse<string>> ChangeBindPhoneNumber(ChangePhoneNumberRequest.Validation request)
            => conn.SendAsync<ChangePhoneNumberRequest.Validation, string>(
                isAnonymous: false,
                isSecurity: true,
                method: HttpMethod.Post,
                requestUri: "api/Manage/ChangeBindPhoneNumber",
                request: request,
                cancellationToken: default,
                responseContentMaybeNull: true);

        public Task<IApiResponse> ChangeBindPhoneNumber(ChangePhoneNumberRequest.New request)
            => conn.SendAsync(
                isAnonymous: false,
                isSecurity: true,
                method: HttpMethod.Put,
                requestUri: "api/Manage/ChangeBindPhoneNumber",
                request: request,
                cancellationToken: default);

        public Task<IApiResponse> DeleteAccount()
        {
            throw new NotImplementedException();
        }
    }
}