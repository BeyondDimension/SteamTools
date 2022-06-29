// https://github.com/dotnetcore/FastGithub/blob/58f79ddc19410c92b18e8d4de1c4b61376e97be7/FastGithub.HttpServer/TlsMiddlewares/FakeTlsConnectionFeature.cs

using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Http.Features;

namespace System.Application.Services.Implementation.HttpServer.Middleware;

sealed class FakeTlsConnectionFeature : ITlsConnectionFeature
{
    public static FakeTlsConnectionFeature Instance { get; } = new();

    public X509Certificate2? ClientCertificate
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }

    public Task<X509Certificate2?> GetClientCertificateAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
