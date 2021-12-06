namespace System.Application.Services
{
#if NETFRAMEWORK
    static
#endif
    partial
#if NETFRAMEWORK
    class
#else
    interface
#endif
    IPlatformService
    {
#if NETFRAMEWORK
        public
#endif
        const string SystemBootRunArguments = "-clt c -silence";

        /// <summary>
        /// 锁定
        /// </summary>
        /// <returns></returns>
        void SystemLock(int waitSecond = 30) { }

        /// <summary>
        /// 关闭系统
        /// </summary>
        /// <param name="waitSecond">等待秒数</param>
        void SystemShutdown(int waitSecond = 30) { }

        /// <summary>
        /// 睡眠系统
        /// </summary>
        /// <param name="waitSecond">等待秒数</param>
        void SystemSleep(int waitSecond = 30) { }

        /// <summary>
        /// 休眠系统
        /// </summary>
        /// <param name="waitSecond">等待秒数</param>
        void SystemHibernate(int waitSecond = 30) { }
    }
}