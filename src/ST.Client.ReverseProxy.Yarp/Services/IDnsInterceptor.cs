// https://github.com/dotnetcore/FastGithub/blob/2.1.4/FastGithub.PacketIntercept/IDnsInterceptor.cs

#if WINDOWS

namespace System.Application.Services;

/// <summary>
/// DNS 拦截器
/// </summary>
interface IDnsInterceptor : IInterceptor
{
}

#endif