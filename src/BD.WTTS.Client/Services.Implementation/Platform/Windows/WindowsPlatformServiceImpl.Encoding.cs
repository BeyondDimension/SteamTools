#if WINDOWS

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

partial class WindowsPlatformServiceImpl
{
    [Mobius(
"""
Encoding2.Default
""")]
    Encoding IPlatformService.Default => Encoding2.Default;
}
#endif