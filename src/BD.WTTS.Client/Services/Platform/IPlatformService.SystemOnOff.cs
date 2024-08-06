// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services;

partial interface IPlatformService
{
    /// <summary>
    /// 设置系统关闭时任务
    /// </summary>
    [Mobius(
"""
Mobius.Helpers.SystemBootHelper
""")]
    void SetSystemSessionEnding(Action action) { }

    /// <summary>
    /// 设置开机自启动
    /// </summary>
    /// <param name="isAutoStart">开启<see langword="true"/>、关闭<see langword="false"/></param>
    /// <param name="name"></param>
    [Mobius(
"""
Mobius.Helpers.SystemBootHelper
""")]
    void SetBootAutoStart(bool isAutoStart, string name) { }
}