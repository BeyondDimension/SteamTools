// https://github.com/dotnetcore/FastGithub/blob/2.1.4/FastGithub.PacketIntercept/ITcpInterceptor.cs

#if !NOT_WINDOWS

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services;

/// <summary>
/// TCP 拦截器
/// </summary>
interface ITcpInterceptor : IInterceptor
{
}

#endif