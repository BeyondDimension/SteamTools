namespace Mobius;

partial class XunYouSDK // 2.接口 xunyou_is_install
{
    /// <summary>
    /// 指定版本的迅游是否安装。
    /// </summary>
    /// <param name="id">合作 id，由迅游给出明确值</param>
    /// <returns></returns>
    [LibraryImport(libraryName, EntryPoint = "xunyou_is_install")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    [return: MarshalAs(UnmanagedType.U1)] // Bool 为 1 字节  true为 1 字节的 01  false 为 1 字节的 00
    public static partial bool IsInstall(int id = appId);
}