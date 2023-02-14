#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

partial interface IArchiSteamFarmService
{
    /// <summary>
    /// 使用弹窗密码框输入自定义密钥并设置与保存
    /// </summary>
    /// <returns></returns>
    Task SetEncryptionKeyAsync();
}
#endif