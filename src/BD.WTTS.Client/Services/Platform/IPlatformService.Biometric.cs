// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services;

partial interface IPlatformService
{
    /// <summary>
    /// 当前设备是否支持生物识别，例如指纹，人脸，虹膜等
    /// <para>Windows Hello</para>
    /// </summary>
    ValueTask<bool> IsSupportedBiometricAsync() => new(false);
}