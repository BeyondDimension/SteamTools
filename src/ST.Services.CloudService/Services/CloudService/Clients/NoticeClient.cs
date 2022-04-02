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
        public Task<IApiResponse<NoticeTypeDTO[]>> Types()
         => conn.SendAsync<NoticeTypeDTO[]>(
             isPolly: true,
             isAnonymous: true,
             isSecurity: true,
             method: HttpMethod.Get,
             requestUri: "api/Notice/Types",
             cancellationToken: default);


        public Task<IApiResponse<PagedModel<NoticeDTO>>> Table(Guid typeId, int index, int? size = null)
                 => conn.SendAsync<PagedModel<NoticeDTO>>(
                     isPolly: true,
                     isAnonymous: true,
                     isSecurity: true,
                     method: HttpMethod.Post,
                     requestUri: $"api/Notice/List/{typeId}?index={index}{(size.HasValue ? $"&size={size}" : "")}",
                     cancellationToken: default);

        public Task<IApiResponse<NoticeDTO[]>> NewMsg(Guid typeId,DateTimeOffset? time)
                 => conn.SendAsync<NoticeDTO[]>(
                     isPolly: true,
                     isAnonymous: true,
                     isSecurity: true,
                     method: HttpMethod.Get,
                     requestUri: $"api/Notice/NewMsg/{typeId}{(time.HasValue?$"?time={time}":"")}",
                     cancellationToken: default);

    }
}
