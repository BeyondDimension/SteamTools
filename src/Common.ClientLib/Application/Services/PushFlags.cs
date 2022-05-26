namespace System.Application.Services;

/// <summary>
/// 页面跳转的逻辑参数
/// </summary>
[Flags]
public enum PushFlags
{
    Empty = 0,

    /// <summary>
    /// CurrentActivity.Finish()
    /// </summary>
    Finish = 0x00000001,

    /// <summary>
    /// ActivityFlags.ClearTop
    /// </summary>
    ClearTop = 0x00000010,

    /// <summary>
    /// ActivityFlags.SingleTop
    /// </summary>
    SingleTop = 0x00000100,

    /// <summary>
    /// CurrentActivity.OverridePendingTransition(0, 0)
    /// </summary>
    OverridePendingTransitionZeroZero = 0x00001000,
}