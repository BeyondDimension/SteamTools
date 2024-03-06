namespace Mobius;

partial class XunYouSDK // 8.接口 xunyou_stop_testspeded
{
    /// <summary>
    /// 停止迅游测速。
    /// </summary>
    public static void StopTestSpeded()
    {
        xunyou_stop_testspeed();
    }

    /// <summary>
    /// 停止迅游测速。
    /// </summary>
    [LibraryImport(libraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    private static partial void xunyou_stop_testspeed();
}