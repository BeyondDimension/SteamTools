namespace Mobius.Models;

/// <summary>
/// 测速回调信息
/// <para>7.接口 xunyou_testspeed</para>
/// </summary>
[StructLayout(LayoutKind.Sequential)]
[MP2Obj(MP2SerializeLayout.Explicit)]
public readonly partial record struct SpeedCallbackInfo
{
    /// <summary>
    /// 测速失败描述
    /// </summary>
    [MP2Key(0)]
    public readonly nint ErrorDesc { get; init; }

    /// <summary>
    /// 测速失败描述字符串长度
    /// </summary>
    [MP2Key(1)]
    public readonly uint ErrorDescLen { get; init; }

    /// <summary>
    /// 测速失败错误码，0 表示测速成功
    /// </summary>
    [MP2Key(2)]
    public readonly uint ErrorCode { get; init; }

    /// <summary>
    /// 加速成功的延迟，未加速成功时为 0
    /// </summary>
    [MP2Key(3)]
    public readonly uint PingSpeed { get; init; }

    /// <summary>
    /// 未加速的延迟，加速成功时为 0
    /// </summary>
    [MP2Key(4)]
    public readonly uint PingLocal { get; init; }

    /// <summary>
    /// 加速成功的丢包
    /// </summary>
    [MP2Key(5)]
    public readonly float PingSpeedLoss { get; init; }

    /// <summary>
    /// 未加速的丢包
    /// </summary>
    [MP2Key(6)]
    public readonly float PingLocalLoss { get; init; }
}

//struct SpeedCallbackInfo
//{
//    wchar_t* errorDesc;		//测速失败描述
//    uint32_t errorDescLen;		//测速失败描述 长度
//    uint32_t errorCode; 		//测速失败错误码，0表示测速成功
//    uint32_t pingSpeed;	    //加速成功的延迟，未加速成功时为0
//    uint32_t pingLocal;		//未加速的延迟，加速成功时为0
//    float pingSpeedLoss;	//加速成功的丢包
//    float pingLocalLoss;	//未加速的丢包
//};