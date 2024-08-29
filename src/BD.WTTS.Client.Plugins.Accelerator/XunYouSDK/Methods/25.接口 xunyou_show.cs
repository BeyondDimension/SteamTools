namespace Mobius;

partial class XunYouSDK // 25.接口 xunyou_show
{
    /// <summary>
    /// 设置当前启动客户端的显隐状态
    /// </summary>
    /// <param name="show">客户端显示状态(SW_HIDE)</param>
    [LibraryImport(libraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    public static partial void xunyou_show(int show = 0);
}