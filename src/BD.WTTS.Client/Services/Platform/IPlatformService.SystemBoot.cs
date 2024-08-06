// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services;

partial interface IPlatformService
{
    const string SystemBootRunArguments = "-clt c -silence";

    /// <summary>
    /// 锁定
    /// </summary>
    [Mobius(
"""
Mobius.Helpers.SystemBootHelper
""")]
    void SystemLock(int waitSecond = 30) { }

    /// <summary>
    /// 关机
    /// </summary>
    /// <param name="waitSecond">等待秒数</param>
    [Mobius(
"""
Mobius.Helpers.SystemBootHelper
""")]
    void SystemShutdown(int waitSecond = 30) { }

    /// <summary>
    /// 睡眠
    /// </summary>
    [Mobius(
"""
Mobius.Helpers.SystemBootHelper
""")]
    /// <param name="waitSecond">等待秒数</param>
    void SystemSleep(int waitSecond = 30) { }

    /// <summary>
    /// 休眠
    /// </summary>
    /// <param name="waitSecond">等待秒数</param>
    [Mobius(
"""
Mobius.Helpers.SystemBootHelper
""")]
    void SystemHibernate(int waitSecond = 30) { }
}