#if !NETSTANDARD

using System.Security.Authentication;

namespace System.Net.Http;

partial class GeneralHttpClientFactory
{
    public static SocketsHttpHandler CreateSocketsHttpHandler(SocketsHttpHandler? handler = null)
    {
        handler ??= new();
        if (OperatingSystem2.IsWindows7())
        {
            // https://github.com/dotnet/runtime/issues/25722
            handler.SslOptions.EnabledSslProtocols =
                SslProtocols.Tls11 | SslProtocols.Tls12 | SslProtocols.Tls13;
        }
        return handler;
    }
}

#endif