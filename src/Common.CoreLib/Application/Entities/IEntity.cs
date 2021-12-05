namespace System.Application.Entities
{
    /// <summary>
    /// (数据库表)实体模型接口
    /// </summary>
    /// <typeparam name="TPrimaryKey"></typeparam>
    public interface IEntity<TPrimaryKey> where TPrimaryKey : IEquatable<TPrimaryKey>
    {
        /// <summary>
        /// 主键
        /// </summary>
        TPrimaryKey Id { get; set; }
    }
}