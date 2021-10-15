namespace System.Application.Columns
{
    /// <summary>
    /// 列 - 昵称
    /// </summary>
    public interface INickName
    {
        /// <summary>
        /// 昵称
        /// </summary>
        string? NickName { get; set; }
    }

    /// <summary>
    /// 列(只读) - 昵称
    /// </summary>
    public interface IReadOnlyNickName
    {
        /// <inheritdoc cref="INickName.NickName"/>
        string? NickName { get; }
    }
}