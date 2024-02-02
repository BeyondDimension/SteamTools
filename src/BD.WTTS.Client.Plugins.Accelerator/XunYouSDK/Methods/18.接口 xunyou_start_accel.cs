namespace Mobius;

partial class XunYouSDK // 18.接口 xunyou_start_accel
{
    /// <summary>
    /// 同步发送加速命令给客户端
    /// </summary>
    /// <param name="gameid">游戏 Id</param>
    /// <param name="area">区 Id</param>
    /// <param name="areaName">游戏区名</param>
    /// <param name="server">服 Id</param>
    /// <param name="svrName">游戏服名</param>
    /// <returns></returns>
    public static XunYouSendResultCode StartAccel(
        int gameid,
        int area,
        string? areaName,
        int server,
        string? svrName)
    {
        int result;
        unsafe
        {
            int area_name_len = areaName?.Length ?? 0;
            int svr_name_len = svrName?.Length ?? 0;

            result = xunyou_start_accel(appId, gameid,
                area, areaName, area_name_len,
                server, svrName, svr_name_len);
        }
        return (XunYouSendResultCode)result;
    }

    [LibraryImport(libraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    private static unsafe partial int xunyou_start_accel(
        int id,
        int gameid = 0,
        int area = 0,
        [MarshalAs(UnmanagedType.LPStr)] string? area_name = default,
        int area_name_len = 0,
        int server = 0,
        [MarshalAs(UnmanagedType.LPStr)] string? svr_name = default,
        int svr_name_len = 0);
}