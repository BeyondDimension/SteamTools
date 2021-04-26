using System.Application.Models;
using System.Application.Services.CloudService.Clients.Abstractions;
using System.Net.Http;
using System.Threading.Tasks;

namespace System.Application.Services.CloudService.Clients
{
	internal sealed class ScriptClient : ApiClient, IScriptClient
	{
		public ScriptClient(IApiConnection conn) : base(conn)
		{
		}
		public Task<IApiResponse<ScriptResponse>> Basics()
		{
			var url =
				$"api/script/basics";
			return conn.SendAsync<ScriptResponse>(
				isAnonymous: true,
				method: HttpMethod.Get,
				requestUri: url,
				cancellationToken: default,
				responseContentMaybeNull: true);
		}
		public Task<IApiResponse<PagedModel<ScriptDTO>>> ScriptTable(string? name = null, int pageIndex = 1, int pageSize = 15)
		{
			var url =
			$"api/script/table/{pageIndex}/{pageSize}/{name ?? string.Empty}";
			return conn.SendAsync<PagedModel<ScriptDTO>>(
				//isAnonymous: true,
				method: HttpMethod.Get,
				requestUri: url,
				cancellationToken: default,
				responseContentMaybeNull: true);
		}
	}
}
