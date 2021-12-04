using System.ComponentModel;

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
        [Description("Sleep")]
        Sleep,

        /// <summary>
        /// 休眠
        /// </summary>
        [Description("Hibernate")]
        Hibernate,

        /// <summary>
        /// 关机
        /// </summary>
        [Description("Shutdown")]
        Shutdown,

        ///// <summary>
        ///// 锁屏
        ///// </summary>
        //[Description("Lock")]
        //Lock,
    }
}