namespace System.Application.Models
{
    /// <summary>
    /// 数据库设置
    /// </summary>
    public interface IDatabaseSettings
    {
        /// <summary>
        /// 数据库表名称前缀
        /// </summary>
        string? TablePrefix { get; }
    }
}