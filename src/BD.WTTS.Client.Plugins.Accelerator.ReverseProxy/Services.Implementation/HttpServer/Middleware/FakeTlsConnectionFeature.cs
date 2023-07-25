// https://github.com/dotnetcore/FastGithub/blob/58f79ddc19410c92b18e8d4de1c4b61376e97be7/FastGithub.HttpServer/TlsMiddlewares/FakeTlsConnectionFeature.cs

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

sealed class FakeTlsConnectionFeature : ITlsConnectionFeature
{
    public static FakeTlsConnectionFeature Instance { get; } = new();

    public X509Certificate2? ClientCertificate
    {
        get => default;
        set { }
    }

    public Task<X509Certificate2?> GetClientCertificateAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult<X509Certificate2?>(default);
    }
}