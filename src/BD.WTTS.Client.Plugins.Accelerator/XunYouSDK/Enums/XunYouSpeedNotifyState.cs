namespace Mobius.Enums;

/// <summary>
/// 迅游测速通知状态
/// <para>7.接口 xunyou_testspeed</para>
/// </summary>
public enum XunYouSpeedNotifyState
{
    /// <summary>
    /// 测速初始化
    /// </summary>
    TEST_INIT = 1,

    /// <summary>
    /// 测速
    /// </summary>
    TEST = 2,

    /// <summary>
    /// 测速异常
    /// </summary>
    Test_FAILED = 3,
}