// ReSharper disable once CheckNamespace
using dotnetCampus.Ipc.CompilerServices.Attributes;

namespace BD.WTTS.Services;

/// <summary>
/// 证书管理(安装/卸载)
/// </summary>
[IpcPublic(Timeout = AssemblyInfo.IpcTimeout, IgnoresIpcException = false)]
public interface ICertificateManager
{
    static class Constants
    {
        public static ICertificateManager Instance => Ioc.Get<ICertificateManager>(); // 因为 Ipc 服务接口的原因，不能将此属性放在非嵌套类上
    }

    /// <summary>
    /// 证书密码的 Utf8String
    /// </summary>
    byte[]? PfxPassword { get; }

    #region Path

    /// <summary>
    /// PFX 证书文件路径
    /// </summary>
    string PfxFilePath => CertificateConstants.DefaultPfxFilePath;

    /// <summary>
    /// CER 证书文件路径
    /// </summary>
    string CerFilePath => CertificateConstants.DefaultCerFilePath;

    #endregion

    /// <summary>
    /// 获取当前 Root 证书，<see cref="X509CertificatePackable"/> 类型可隐式转换为 <see cref="X509Certificate2"/>
    /// </summary>
    X509CertificatePackable RootCertificatePackable { get; set; }

    /// <summary>
    /// 获取 Cer 证书路径，当不存在时生成文件后返回路径
    /// </summary>
    /// <returns></returns>
    string? GetCerFilePathGeneratedWhenNoFileExists();

    /// <summary>
    /// 由平台实现的信任根证书引导，有 Root 权限将尝试执行信任，否则则 UI 引导，跳转网页或弹窗
    /// </summary>
    ValueTask PlatformTrustRootCertificateGuideAsync();

    /// <summary>
    /// 安装根证书，如果没有证书将生成一个新的
    /// </summary>
    /// <returns>返回根证书是否受信任</returns>
    ValueTask<bool> SetupRootCertificateAsync();

    /// <summary>
    /// 删除根证书，如果没有证书将返回 <see langword="true"/>
    /// </summary>
    /// <returns></returns>
    bool DeleteRootCertificate();

    /// <summary>
    /// 当前根证书是否已安装并信任
    /// </summary>
    bool IsRootCertificateInstalled { get; }
}