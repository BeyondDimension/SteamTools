namespace System.Application.Models
{
    /// <summary>
    /// 登录响应内容
    /// </summary>
    public interface ILoginResponse : IReadOnlyAuthToken
    {
        /// <summary>
        /// 用户Id
        /// </summary>
        Guid UserId { get; }
    }
}