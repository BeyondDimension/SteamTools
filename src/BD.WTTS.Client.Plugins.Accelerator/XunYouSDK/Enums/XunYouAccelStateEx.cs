namespace Mobius.Enums;

/// <summary>
/// 迅游加速状态
/// <para>10.接口 xunyou_accel_state_ex</para>
/// </summary>
public enum XunYouAccelStateEx
{
    /// <summary>
    /// 获取失败
    /// </summary>
    获取失败 = -1,

    /// <summary>
    /// 获取成功，未加速
    /// </summary>
    未加速 = 0,

    /// <summary>
    /// 获取成功，已加速
    /// </summary>
    已加速 = 1,

    /// <summary>
    /// 获取成功，正在启动加速中
    /// </summary>
    启动加速中 = 2,

    /// <summary>
    /// 获取成功，停止加速中
    /// </summary>
    停止加速中 = 3,

    /// <summary>
    /// 获取成功，加速已断开
    /// </summary>
    加速已断开 = 4,

    /// <summary>
    /// 获取成功，加速失败
    /// </summary>
    加速失败 = 5,
}