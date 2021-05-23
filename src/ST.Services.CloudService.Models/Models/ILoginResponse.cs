using System.Application.Columns;

namespace System.Application.Models
{
    /// <summary>
    /// 登录响应内容
    /// </summary>
    public interface ILoginResponse : IReadOnlyAuthToken, IReadOnlyPhoneNumber
    {
        /// <summary>
        /// 用户Id
        /// </summary>
        Guid UserId => User.Id;

        UserInfoDTO User { get; }
    }
}