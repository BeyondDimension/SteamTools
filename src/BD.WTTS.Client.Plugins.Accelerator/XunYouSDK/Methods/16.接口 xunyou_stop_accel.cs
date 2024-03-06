namespace Mobius;

partial class XunYouSDK // 16.接口 xunyou_stop_accel
{
    /// <summary>
    /// 同步发送停止加速命令给客户端
    /// </summary>
    /// <returns></returns>
    public static XunYouSendResultCode Stop()
    {
        int result;
        unsafe
        {
            result = xunyou_stop_accel(appId);
        }
        return (XunYouSendResultCode)result;
    }

    /// <summary>
    /// 同步发送停止加速命令给客户端
    /// </summary>
    /// <param name="id">合作 id，由迅游给出明确值</param>
    /// <returns></returns>
    [LibraryImport(libraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    private static unsafe partial int xunyou_stop_accel(int id);
}