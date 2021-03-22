namespace System.Application.Columns
{
    /// <summary>
    /// 头像
    /// </summary>
    public interface IAvatar
    {
        /// <inheritdoc cref="IAvatar"/>
        Guid? Avatar { get; set; }
    }

    /// <inheritdoc cref="IAvatar"/>
    public interface IReadOnlyAvatar
    {
        /// <inheritdoc cref="IAvatar"/>
        Guid Avatar { get; }
    }
}