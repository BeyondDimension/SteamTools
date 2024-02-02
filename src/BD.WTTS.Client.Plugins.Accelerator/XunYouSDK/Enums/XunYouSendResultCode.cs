namespace Mobius.Enums;

/// <summary>
/// 迅游发送命令返回值
/// </summary>
public enum XunYouSendResultCode
{
    /// <summary>
    /// 发送失败
    /// </summary>
    发送失败 = -1,

    /// <summary>
    /// 发送停止命令到客户端成功（并不代表停止加速成功）
    /// </summary>
    发送成功 = 0,
}