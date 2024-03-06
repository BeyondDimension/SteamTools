namespace Mobius.Models;

/// <summary>
/// <see cref="SpeedCallbackInfo"/> 的包装类，可用于传输数据
/// <para>7.接口 xunyou_testspeed</para>
/// </summary>
[MP2Obj(MP2SerializeLayout.Explicit)]
public partial record class SpeedCallbackWrapper
{
    /// <inheritdoc cref="SpeedCallbackInfo"/>
    [MP2Key(0)]
    public SpeedCallbackInfo Struct { get; set; }

    /// <summary>
    /// 测速失败描述
    /// </summary>
    [MP2Key(1)]
    public string? ErrorDesc { get; set; }

    /// <inheritdoc cref="XunYouSpeedNotifyState"/>
    [MP2Key(2)]
    public XunYouSpeedNotifyState State { get; set; }

    /// <inheritdoc cref="XunYouTestSpeedCode"/>
    [MP2Key(3)]
    public XunYouTestSpeedCode Code { get; set; }
}