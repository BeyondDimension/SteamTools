namespace System.Application.Models
{
    /// <summary>
    /// 退出系统方式
    /// </summary>
    public enum SystemEndMode
    {
        /// <summary>
        /// 睡眠
        /// </summary>
        Sleep,

        /// <summary>
        /// 休眠
        /// </summary>
        Hibernate,

        /// <summary>
        /// 关机
        /// </summary>
        Shutdown,

        /// <summary>
        /// 锁定
        /// </summary>
        Lock,
    }
}