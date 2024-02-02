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
    /// 获取成功，正在启动加速中
    /// </summary>
    加速中 = 1,

    /// <summary>
    /// 获取成功，已加速，只有已加速情况才会返回对应信息
    /// </summary>
    加速已完成 = 2,
}