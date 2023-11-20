using static BD.WTTS.Services.IUserManager;

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

/// <inheritdoc cref="IUserManager"/>
partial class UserManager : IUserManager
{
    protected const string TAG = "UserManager";

    protected readonly ISecureStorage storage;
    protected readonly ILogger logger;
    protected readonly IUserRepository userRepository;
    protected readonly ISecurityService security;

    public UserManager(
        ILoggerFactory loggerFactory,
        ISecureStorage storage,
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
    protected IdentityUserInfoDTO? currentUserInfo;

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
        logger.LogDebug("name: {name}, PhoneNumber: {phoneNumber}",
            name, currentUser?.ToStringHideMiddleFour());
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
        logger.LogDebug("read_cache: {read_cache}, PhoneNumber: {phoneNumber}", read_cache, result?.ToStringHideMiddleFour());
#endif
        return result;
    }

    public ValueTask<CurrentUser?> GetCurrentUserAsync()
    {
        return GetCurrentUserAsync(true);
    }

    public async Task SetCurrentUserAsync(CurrentUser? value)
    {
        await storage.SetAsync(KEY_CURRENT_LOGIN_USER, value);
        CurrentUser = value;
        PrintCurrentUser("SetCurrentUser");
    }

    public async ValueTask<IdentityUserInfoDTO?> GetCurrentUserInfoAsync()
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

    public async Task SetCurrentUserInfoAsync(IdentityUserInfoDTO value, bool updateToDataBase)
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

    public async ValueTask<JWTEntity?> GetShopAuthTokenAsync()
    {
        var value = await GetCurrentUserAsync(false);
        return value?.ShopAuthToken;
    }

    public event Action? OnSignOut;

    public async Task SignOutAsync()
    {
        PrintCurrentUser("SignOut");
        currentUserInfo = default;
        await SetCurrentUserAsync(null);
        OnSignOut?.Invoke();
    }

    ValueTask<User?> GetUserTableByIdAsync(Guid userId)
    {
        return userRepository.FindAsync(userId);
    }

    static async Task<TUserDTO?> GetUserByTableAsync<TUserDTO>(User? user, Func<User, Task<TUserDTO?>> binding) where TUserDTO : IUserDTO
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
            NickName = nickName ?? string.Empty,
            Avatar = user.Avatar ?? default,
        };
        return value;
    }

    //public async Task<UserDTO?> GetUserByIdAsync(Guid userId)
    //{
    //    var user = await GetUserTableByIdAsync(userId);
    //    var value = await GetUserByTableAsync(user, BindingUserAsync<UserDTO>);
    //    return value;
    //}

    async Task<bool> VerifyUserInfoAsync(User user, IdentityUserInfoDTO userInfo)
    {
        if (user.Id != userInfo.Id)
        {
            logger.LogError("VerifyUserInfo Fail(Id).");
            return false;
        }
        var nickName = await security.D(user.NickName) ?? string.Empty;
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

    async Task<IdentityUserInfoDTO?> BindingUserInfoAsync(User user)
    {
        if (user.UserInfo != null)
        {
            try
            {
                var userInfoBytes = await security.DB(user.UserInfo);
                if (userInfoBytes != null)
                {
                    var value = Serializable.DMP<IdentityUserInfoDTO>(userInfoBytes);
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
        var r = await BindingUserAsync<IdentityUserInfoDTO>(user);
        return r;
    }

    public async Task<IdentityUserInfoDTO?> GetUserInfoByIdAsync(Guid userId)
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
        if (user is IdentityUserInfoDTO userInfo)
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
        (var rowCount, var result) = await userRepository.InsertOrUpdateAsync(userTable, cancellationToken: default);
        PrintInsertOrUpdateResult(user, rowCount, result);
    }

    [Conditional("DEBUG")]
    void PrintInsertOrUpdateResult(IUserDTO user, int rowCount, DbRowExecResult result)
    {
        logger.LogInformation(
            "User.InsertOrUpdate rowCount: {rowCount}, result: {result}, user: [{nickName}-{uid}]",
            rowCount, result, user.NickName, user.Id);
    }
}