using System.Application.Models;
using System.Threading.Tasks;

namespace System.Application.Services
{
    /// <summary>
    /// 用户管理
    /// </summary>
    public interface IUserManager : IAuthHelper
    {
        public static IUserManager Instance => DI.Get<IUserManager>();

        /// <inheritdoc cref="GetCurrentUserAsync"/>
        CurrentUser? GetCurrentUser();

        /// <summary>
        /// 获取当前登录用户
        /// <para>如果[退出登录]则为 <see langword="null"/>，对于接收到的推送消息，要求在服务端时传入接收人用户Id，客户端根据Id读取用户信息，而不使用此值</para>
        /// </summary>
        /// <returns></returns>
        ValueTask<CurrentUser?> GetCurrentUserAsync();

        /// <summary>
        /// 设置当前登录用户，当[退出登录]时可传入<see langword="null"/>
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        Task SetCurrentUserAsync(CurrentUser? value);

        /// <summary>
        /// 获取当前登录用户资料
        /// <para>如果[退出登录]则为 <see langword="null"/>，对于接收到的推送消息，要求在服务端时传入接收人用户Id，客户端根据Id读取用户信息，而不使用此值</para>
        /// </summary>
        /// <returns></returns>
        ValueTask<UserInfoDTO?> GetCurrentUserInfoAsync();

        /// <summary>
        /// 设置当前登录用户资料
        /// </summary>
        /// <param name="value"></param>
        /// <param name="updateToDataBase">更新到数据库中</param>
        /// <returns></returns>
        Task SetCurrentUserInfoAsync(UserInfoDTO value, bool updateToDataBase);

        /// <summary>
        /// 根据Id获取用户数据
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<UserDTO?> GetUserByIdAsync(Guid userId);

        /// <summary>
        /// 根据Id获取用户资料
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<UserInfoDTO?> GetUserInfoByIdAsync(Guid userId);

        /// <summary>
        /// 添加或更新用户数据到数据库中
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        Task InsertOrUpdateAsync(IUserDTO user);

        /// <summary>
        /// 获取当前登录用户的手机号码
        /// </summary>
        /// <param name="notHideMiddleFour"></param>
        /// <returns></returns>
        async Task<string> GetCurrentUserPhoneNumberAsync(bool notHideMiddleFour = false)
        {
            var phone_number = (await GetCurrentUserAsync())?.PhoneNumber;
            if (string.IsNullOrWhiteSpace(phone_number)) return string.Empty;
            return notHideMiddleFour ? phone_number : PhoneNumberHelper.ToStringHideMiddleFour(phone_number);
        }

        /// <summary>
        /// 当登出时
        /// </summary>
        event Action? OnSignOut;
    }
}