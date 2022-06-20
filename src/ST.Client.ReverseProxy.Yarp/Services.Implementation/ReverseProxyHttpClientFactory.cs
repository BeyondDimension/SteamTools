// https://github.com/dotnetcore/FastGithub/blob/2.1.4/FastGithub.Http/HttpClientFactory.cs

using System.Application.Models;

namespace System.Application.Services.Implementation;

sealed class ReverseProxyHttpClientFactory : IReverseProxyHttpClientFactory
{
    public HttpClient CreateHttpClient(string domain, AccelerateProjectDTO domainConfig)
    {
        throw new NotImplementedException();
    }
}
