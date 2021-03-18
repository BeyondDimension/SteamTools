namespace System.Application.Models
{
    /// <summary>
    /// 用户活跃类型，用于 DAU，MAU 等统计指标分析
    /// </summary>
    public enum ActiveUserType : ushort
    {
        /// <summary>
        /// 启动时
        /// </summary>
        OnStartup = 1,
    }
}