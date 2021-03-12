using Microsoft.Extensions.Logging;
using System.Application.Entities;
using System.Application.Models;
using System.Application.Repositories;
using System.Diagnostics;
using System.Security;
using System.Threading.Tasks;
using static System.Application.KeyConstants;

namespace System.Application.Services.Implementation
{
    /// <inheritdoc cref="IUserManager"/>
    public class UserManager : IUserManager
    {
        protected const string TAG = "UserManager";

        protected readonly IStorage storage;
        protected readonly ILogger logger;
        protected readonly IUserRepository userRepository;
        protected readonly ISecurityService security;

        public UserManager(
            ILoggerFactory loggerFactory,
            IStorage storage,
            IUserRepository userRepository,
            ISecurityService security)
        {
            logger = loggerFactory.CreateLogger(TAG);
            this.storage = storage;
            this.userRepository = userRepository;
            this.security = security;
        }

        protected bool isAnonymous;
        protected CurrentUser? currentUser;
        protected UserInfoDTO? currentUserInfo;

        protected CurrentUser? CurrentUser
        {
            set
            {
                currentUser = value;
                isAnonymous = value == null;
            }
        }

        [Conditional("DEBUG")]
        void PrintCurrentUser(string name)
        {
            logger.LogInformation(
                $"{name}: {currentUser?.ToStringHideMiddleFour()}");
        }

        protected async ValueTask<CurrentUser?> GetCurrentUserAsync(bool clone)
        {
            if (currentUser == null && !isAnonymous)
            {
                try
                {
                    CurrentUser = await storage.GetAsync<CurrentUser>(KEY_CURRENT_LOGIN_USER);
                }
                catch (Exception e)
                {
                    logger.LogError(e, nameof(GetCurrentUserAsync));
                }
                PrintCurrentUser(nameof(GetCurrentUserAsync));
            }
            return clone ? currentUser?.Clone() : currentUser;
        }

        public CurrentUser? GetCurrentUser()
        {
            var hasCurrentUser = currentUser != null;
#if DEBUG
            var read_cache = Random2.Next(100) % 2 == 0;
            hasCurrentUser = read_cache && hasCurrentUser;
#endif
            CurrentUser? result;
            if (hasCurrentUser)
            {
                result = currentUser?.Clone();
            }
            else
            {
                Func<ValueTask<CurrentUser?>> func = GetCurrentUserAsync;
                result = func.RunSync();
            }
#if DEBUG
            logger.LogDebug("read_cache: {0}, PhoneNumber: {1}", read_cache, result?.ToStringHideMiddleFour());
#endif
            return result;
        }

        public ValueTask<CurrentUser?> GetCurrentUserAsync()
        {
            return GetCurrentUserAsync(true);
        }

        public async Task SetCurrentUserAsync(CurrentUser? value)
        {
            await storage.SetAsync(KEY_CURRENT_LOGIN_USER, currentUser);
            CurrentUser = value;
            PrintCurrentUser("SetCurrentUser");
        }

        public async ValueTask<UserInfoDTO?> GetCurrentUserInfoAsync()
        {
            if (currentUserInfo == null && !isAnonymous)
            {
                var cUser = await GetCurrentUserAsync();
                if (cUser != null)
                {
                    currentUserInfo = await GetUserInfoByIdAsync(cUser.UserId);
                }
            }
            return currentUserInfo;
        }

        public async Task SetCurrentUserInfoAsync(UserInfoDTO value, bool updateToDataBase)
        {
            currentUserInfo = value;
            if (updateToDataBase)
            {
                await InsertOrUpdateAsync(value);
            }
        }

        public async ValueTask<JWTEntity?> GetAuthTokenAsync()
        {
            var value = await GetCurrentUserAsync(false);
            return value?.AuthToken;
        }

        public async Task SignOutAsync()
        {
            PrintCurrentUser("SignOut");
            currentUserInfo = default;
            await SetCurrentUserAsync(null);
        }

        ValueTask<User?> GetUserTableByIdAsync(Guid userId)
        {
            return userRepository.FindAsync(userId);
        }

        async Task<TUserDTO?> GetUserByTableAsync<TUserDTO>(User? user, Func<User, Task<TUserDTO?>> binding) where TUserDTO : IUserDTO
        {
            if (user == null) return default;
            var value = await binding(user);
            return value;
        }

        async Task<TUserDTO?> BindingUserAsync<TUserDTO>(User user) where TUserDTO : IUserDTO, new()
        {
            var nickName = await security.D(user.NickName);

            var value = new TUserDTO
            {
                Id = user.Id,
                NickName = nickName,
                Avatar = user.Avatar,
            };
            return value;
        }

        public async Task<UserDTO?> GetUserByIdAsync(Guid userId)
        {
            var user = await GetUserTableByIdAsync(userId);
            var value = await GetUserByTableAsync(user, BindingUserAsync<UserDTO>);
            return value;
        }

        async Task<bool> VerifyUserInfoAsync(User user, UserInfoDTO userInfo)
        {
            if (user.Id != userInfo.Id)
            {
                logger.LogError("VerifyUserInfo Fail(Id).");
                return false;
            }
            var nickName = await security.D(user.NickName);
            if (nickName != userInfo.NickName)
            {
                logger.LogError("VerifyUserInfo Fail(NickName).");
                return false;
            }
            if (user.Avatar != userInfo.Avatar)
            {
                logger.LogError("VerifyUserInfo Fail(Avatar).");
                return false;
            }
            return true;
        }

        async Task<UserInfoDTO?> BindingUserInfoAsync(User user)
        {
            if (user.UserInfo != null)
            {
                try
                {
                    var userInfoBytes = await security.DB(user.UserInfo);
                    if (userInfoBytes != null)
                    {
                        var value = Serializable.DMP<UserInfoDTO>(userInfoBytes);
                        if (value != null)
                        {
                            var v = await VerifyUserInfoAsync(user, value);
                            if (v)
                            {
                                return value;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.LogError(e, "BindingUserInfo Serializable & Verify Fail.");
                }
            }
            var r = await BindingUserAsync<UserInfoDTO>(user);
            return r;
        }

        public async Task<UserInfoDTO?> GetUserInfoByIdAsync(Guid userId)
        {
            var user = await GetUserTableByIdAsync(userId);
            return await GetUserByTableAsync(user, BindingUserInfoAsync);
        }

        public async Task InsertOrUpdateAsync(IUserDTO user)
        {
            var nickName_ = await security.E(user.NickName);

            var userTable = new User
            {
                Id = user.Id,
                NickName = nickName_,
                Avatar = user.Avatar,
            };
            if (user is UserInfoDTO userInfo)
            {
                userTable.UserInfo = Serializable.SMP(userInfo);
                userTable.UserInfo = await security.EB(userTable.UserInfo);
            }
            else
            {
                var userTable2 = await GetUserTableByIdAsync(user.Id);
                if (userTable2?.UserInfo != null)
                {
                    userTable.UserInfo = userTable2.UserInfo;
                }
            }
            (var rowCount, var result) = await userRepository.InsertOrUpdateAsync(userTable);
            PrintInsertOrUpdateResult(user, rowCount, result);
        }

        [Conditional("DEBUG")]
        void PrintInsertOrUpdateResult(IUserDTO user, int rowCount, DbRowExecResult result)
        {
            logger.LogInformation(
                $"User.InsertOrUpdate " +
                $"rowCount: {rowCount}, " +
                $"result: {result}, " +
                $"user: [{user.NickName}-{user.Id}]");
        }
    }
}