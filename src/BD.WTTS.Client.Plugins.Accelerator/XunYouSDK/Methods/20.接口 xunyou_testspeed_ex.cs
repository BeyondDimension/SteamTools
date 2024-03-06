namespace Mobius;

partial class XunYouSDK // 20.接口 xunyou_testspeed_ex
{
    /// <summary>
    /// 获取指定游戏某分区的测速。
    /// </summary>
    /// <param name="gameid">游戏 id，由迅游给出明确值。</param>
    /// <param name="area">游戏分区 id ，由游戏给出区服名和 id 对照表。</param>
    /// <param name="server">游戏服ID</param>
    /// <param name="callback">测速回调函数。</param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static XunYouTestSpeedCode TestSpeed(int gameid,
        int area,
        int server,
        TestSpeedCallback callback)
    {
        int testSpeedCode = int.MinValue;
        unsafe
        {
            [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
            int TestSpeedCallback_Wrapper(XunYouSpeedNotifyState state, SpeedCallbackInfo* info)
            {
                var info_ = Marshal.PtrToStructure<SpeedCallbackInfo>((nint)info);
                var result = callback(new SpeedCallbackWrapper
                {
                    Struct = info_,
                    State = state,
                    Code = (XunYouTestSpeedCode)testSpeedCode,
                    ErrorDesc = info_.GetErrorDescString(),
                });
                return result;
            }
            var xunyou_callback = (delegate* unmanaged[Stdcall]<XunYouSpeedNotifyState, SpeedCallbackInfo*, int>)Marshal.GetFunctionPointerForDelegate(testSpeedCallback = TestSpeedCallback_Wrapper);
            testSpeedCode = xunyou_testspeed_ex(gameid, area, server, xunyou_callback);
        }

        return (XunYouTestSpeedCode)testSpeedCode;
    }

    [LibraryImport(libraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    private static unsafe partial int xunyou_testspeed_ex(int gameid,
        int area,
        int server,
        delegate* unmanaged[Stdcall]<XunYouSpeedNotifyState, SpeedCallbackInfo*, int> callback = null);
}