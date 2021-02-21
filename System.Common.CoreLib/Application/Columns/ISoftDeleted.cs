namespace System.Application.Columns
{
    /// <summary>
    /// 软删除
    /// </summary>
    public interface ISoftDeleted
    {
        /// <inheritdoc cref="ISoftDeleted"/>
        bool SoftDeleted { get; set; }
    }
}