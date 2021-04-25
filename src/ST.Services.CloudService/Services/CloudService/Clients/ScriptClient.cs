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
                $"api/acript/basics/";
            return conn.SendAsync<ScriptResponse>(
                isAnonymous: true,
                method: HttpMethod.Get,
                requestUri: url,
                cancellationToken: default,
                responseContentMaybeNull: true);
        }
    }
}
