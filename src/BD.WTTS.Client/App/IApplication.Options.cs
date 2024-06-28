// ReSharper disable once CheckNamespace
namespace BD.WTTS;

partial interface IApplication
{
    [Mobius(
"""
MobiusHost.EnableDevTools
""")]
    public static bool EnableDevtools { get; set; } =
#if DEBUG
        true
#else
        false
#endif
        ;

    /// <summary>
    /// 禁用 GPU 硬件加速
    /// </summary>
    [Mobius(
"""
MobiusHost.DisableGPU
""")]
    public static bool DisableGPU { get; set; }

    /// <summary>
    /// 使用 native OpenGL(仅 Windows)
    /// </summary>
    [Mobius(
"""
MobiusHost.UseWgl
""")]
    public static bool UseWgl { get; set; }
}