// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services;

partial interface IPlatformService
{
    const string SystemBootRunArguments = "-clt c -silence";

    /// <summary>
    /// 锁定
    /// </summary>
    void SystemLock(int waitSecond = 30) { }

    /// <summary>
    /// 关机
    /// </summary>
    /// <param name="waitSecond">等待秒数</param>
    void SystemShutdown(int waitSecond = 30) { }

    /// <summary>
    /// 睡眠
    /// </summary>
    /// <param name="waitSecond">等待秒数</param>
    void SystemSleep(int waitSecond = 30) { }

    /// <summary>
    /// 休眠
    /// </summary>
    /// <param name="waitSecond">等待秒数</param>
    void SystemHibernate(int waitSecond = 30) { }
}