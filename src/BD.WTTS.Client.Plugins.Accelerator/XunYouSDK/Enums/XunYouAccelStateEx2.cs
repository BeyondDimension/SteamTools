#pragma warning disable IDE0079 // 请删除不必要的忽略
#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
#pragma warning restore IDE0079 // 请删除不必要的忽略
namespace Mobius.Enums;

/// <summary>
/// 迅游加速状态
/// <para>17.接口 xunyou_accel_state_ex2</para>
/// </summary>
public enum XunYouState
{
    更新中 = 100,
    更新失败 = 101,
    更新成功 = 102,
    启动中 = 200,
    启动失败 = 201,
    启动成功 = 202,
    退出 = 203,
    登录中 = 300,
    登录成功 = 301,
    登录失败 = 302,
    退出登录 = 303,
    未加速 = 400,
    加速中 = 401,
    加速已完成 = 402,
    停止加速中 = 403,
}