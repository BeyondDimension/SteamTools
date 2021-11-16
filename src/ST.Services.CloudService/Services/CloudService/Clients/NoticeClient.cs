using System.Application.Models;
using System.Application.Services.CloudService.Clients.Abstractions;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace System.Application.Services.CloudService.Clients
{
    internal sealed class NoticeClient : ApiClient, INoticeClient
    {
        public NoticeClient(IApiConnection conn) : base(conn)
        {
        }
        public Task<IApiResponse<List<ScriptDTO>>> Types()
         => conn.SendAsync<List<ScriptDTO>>(
             isPolly: true,
             isAnonymous: true,
             isSecurity: false,
             method: HttpMethod.Get,
             requestUri: "api/Notice/Types",
             cancellationToken: default);

        public Task<IApiResponse<List<ScriptDTO>>> Table(Guid typeId, int index, int size)
                 => conn.SendAsync<List<ScriptDTO>>(
                     isPolly: true,
                     isAnonymous: true,
                     isSecurity: false,
                     method: HttpMethod.Post,
                     requestUri: $"api/Notice/Table/{typeId}?index={index}&size={size}",
                     cancellationToken: default);

        public Task<IApiResponse<List<ScriptDTO>>> NewMsg(Guid typeId)
                 => conn.SendAsync<List<ScriptDTO>>(
                     isPolly: true,
                     isAnonymous: true,
                     isSecurity: false,
                     method: HttpMethod.Get,
                     requestUri: $"api/Notice/NewMsg/{typeId}",
                     cancellationToken: default);

    }
}
