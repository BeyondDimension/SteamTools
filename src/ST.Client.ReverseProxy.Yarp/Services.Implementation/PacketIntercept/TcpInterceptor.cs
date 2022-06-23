// https://github.com/dotnetcore/FastGithub/blob/2.1.4/FastGithub.PacketIntercept/Tcp/TcpInterceptor.cs

#if WINDOWS

using Microsoft.Extensions.Logging;

namespace System.Application.Services.Implementation.PacketIntercept;

/// <inheritdoc cref="ITcpInterceptor"/>
abstract class TcpInterceptor : ITcpInterceptor
{
    public TcpInterceptor(int oldServerPort, int newServerPort, ILogger logger)
    {

    }

    public Task InterceptAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}

#endif