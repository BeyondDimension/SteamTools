namespace Mobius;

partial class XunYouSDK // 7.接口 xunyou_testspeed
{
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private unsafe delegate int TestSpeedCallback_(XunYouSpeedNotifyState state, SpeedCallbackInfo* info);

    static TestSpeedCallback_? testSpeedCallback;

    /// <summary>
    /// 测速回调函数
    /// </summary>
    /// <param name="m">测速回调信息</param>
    /// <returns></returns>
    public delegate int TestSpeedCallback(SpeedCallbackWrapper m);

    ///// <summary>
    ///// 获取指定游戏某分区的测速。
    ///// </summary>
    ///// <param name="gameid">游戏 id，由迅游给出明确值。</param>
    ///// <param name="area">游戏分区 id ，由游戏给出区服名和 id 对照表。</param>
    ///// <param name="callback">测速回调函数。</param>
    ///// <returns></returns>
    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    //public static XunYouTestSpeedCode TestSpeed(int gameid,
    //    int area,
    //    TestSpeedCallback callback)
    //{
    //    int testSpeedCode = int.MinValue;
    //    unsafe
    //    {
    //        [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    //        int TestSpeedCallback_Wrapper(XunYouSpeedNotifyState state, SpeedCallbackInfo* info)
    //        {
    //            var info_ = Marshal.PtrToStructure<SpeedCallbackInfo>((nint)info);
    //            var result = callback(new SpeedCallbackWrapper
    //            {
    //                Struct = info_,
    //                State = state,
    //                Code = (XunYouTestSpeedCode)testSpeedCode,
    //                ErrorDesc = info_.GetErrorDescString(),
    //            });
    //            return result;
    //        }
    //        var xunyou_callback = (delegate* unmanaged[Stdcall]<XunYouSpeedNotifyState, SpeedCallbackInfo*, int>)Marshal.GetFunctionPointerForDelegate(testSpeedCallback = TestSpeedCallback_Wrapper);
    //        testSpeedCode = xunyou_testspeed(gameid, area, xunyou_callback);
    //    }

    //    return (XunYouTestSpeedCode)testSpeedCode;
    //}

    ///// <summary>
    ///// 获取指定游戏某分区的测速。
    ///// </summary>
    ///// <param name="gameid">游戏 id，由迅游给出明确值。</param>
    ///// <param name="area">游戏分区 id ，由游戏给出区服名和 id 对照表。</param>
    ///// <returns></returns>
    ///// <exception cref="NotSupportedException"></exception>
    //public static IAsyncEnumerable<SpeedCallbackWrapper> TestSpeed(int gameid, int area)
    //{
    //    // 异步迭代器版本，用于 Ipc 调用，封装回调函数
    //    // 注意配合停止测试接口
    //    throw new NotSupportedException();
    //}

    //[LibraryImport(libraryName)]
    //[UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    //private static unsafe partial int xunyou_testspeed(int gameid,
    //    int area,
    //    delegate* unmanaged[Stdcall]<XunYouSpeedNotifyState, SpeedCallbackInfo*, int> callback = null);

    /// <summary>
    /// 获取测速失败描述字符串
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    static string? GetErrorDescString(this SpeedCallbackInfo value)
    {
        if (value.ErrorDesc == default || value.ErrorDescLen == 0 || value.ErrorDescLen > int.MaxValue)
            return null;
        var result = Marshal.PtrToStringUni(value.ErrorDesc, (int)value.ErrorDescLen);
        return result;
    }
}