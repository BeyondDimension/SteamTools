namespace Mobius;

partial class XunYouSDK // 11.接口 xunyou_uninstall
{
    /// <summary>
    /// 静默卸载对应渠道 id 的迅游客户端。
    /// </summary>
    /// <param name="id">合作id，由迅游给出明确值。</param>
    /// <returns>0 卸载成功, -1 卸载失败</returns>
    [LibraryImport(libraryName, EntryPoint = "xunyou_uninstall")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    public static partial XunYouUninstallCode Uninstall(int id = appId);
}