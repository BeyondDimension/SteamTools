namespace System.Application.Models
{
    /// <summary>
    /// 获取登录凭证
    /// </summary>
    public interface IReadOnlyAuthToken
    {
        /// <inheritdoc cref="IReadOnlyAuthToken"/>
        string? AuthToken { get; }
    }
}