namespace Mobius;

partial class XunYouSDK // 10.接口 xunyou_accel_state_ex
{
    /// <summary>
    /// 获取当前加速器的加速状态。
    /// </summary>
    /// <param name="gameid">当前加速的游戏 id。</param>
    /// <param name="areaid">当前加速的游戏区 id。</param>
    /// <param name="serverid">当前加速游戏的服 id，没有则为 0。</param>
    /// <returns></returns>
    public static XunYouAccelStateEx GetAccelStateEx(out int gameid,
        out int areaid,
        out int serverid)
    {
        var result = xunyou_accel_state_ex(appId, out gameid, out areaid, out serverid);
        return (XunYouAccelStateEx)result;
    }

    /// <summary>
    /// 获取当前加速器的加速状态。
    /// </summary>
    /// <param name="id">合作 id，由迅游给出明确值</param>
    /// <param name="gameid">当前加速的游戏 id。</param>
    /// <param name="areaid">当前加速的游戏区 id。</param>
    /// <param name="serverid">当前加速游戏的服 id，没有则为 0。</param>
    /// <returns></returns>
    [LibraryImport(libraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    private static partial int xunyou_accel_state_ex(
        int id,
        out int gameid,
        out int areaid,
        out int serverid);
}