using System.Application.Models;
using System.Application.Services.CloudService.Clients.Abstractions;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace System.Application.Services.CloudService.Clients
{
    internal sealed class AdvertisementClient : ApiClient, IAdvertisementClient
    {
        public AdvertisementClient(IApiConnection conn) : base(conn)
        {
        }

        /// <summary>
        /// 获取广告列表
        /// </summary>
        /// <param name="type">广告类型</param>
        /// <returns></returns>
        public Task<IApiResponse<List<AdvertisementDTO>>> All(EAdvertisementType? type)
            => conn.SendAsync<List<AdvertisementDTO>>(
                isPolly: true,
                isAnonymous: true,
                isSecurity: true,
                method: HttpMethod.Get,
                requestUri: $"api/Advertisement/All/{(int)DeviceInfo2.Platform()}/{(int)DeviceInfo2.Idiom()}{(type.HasValue ? $"?type={type}" : "")}",
                cancellationToken: default);
    }
}
