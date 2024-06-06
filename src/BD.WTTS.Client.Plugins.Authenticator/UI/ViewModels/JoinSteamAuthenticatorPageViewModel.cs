using BD.SteamClient.Models;
using BD.SteamClient.Services;
using WinAuth;
using AppResources = BD.WTTS.Client.Resources.Strings;

namespace BD.WTTS.UI.ViewModels;

/// <summary>
/// 联合Steam令牌页面
/// </summary>
public sealed partial class JoinSteamAuthenticatorPageViewModel : ViewModelBase
{
    #region 步骤索引常量

    const int STEPINDEX_LOGIN = 0;

    const int STEPINDEX_VERIFY = 1;

    const int STEPINDEX_DONE = 2;

    #endregion 步骤索引常量

    #region 页面属性

    /// <summary>
    /// 用户填入的 Steam 令牌验证码
    /// </summary>
    [Reactive]
    public string? Code { get; set; }

    /// <summary>
    /// 用户名
    /// </summary>
    [Reactive]
    public string? UserNameText { get; set; }

    /// <summary>
    /// 密码
    /// </summary>
    [Reactive]
    public string? PasswordText { get; set; }

    /// <summary>
    /// 验证码
    /// </summary>
    [Reactive]
    public string? VerifyCodeText { get; set; }

    /// <summary>
    /// 恢复代码
    /// </summary>
    [Reactive]
    public string? RevocationCodeText { get; set; }

    /// <summary>
    /// 当前步骤
    /// </summary>
    [Reactive]
    public int SelectIndex { get; set; } = STEPINDEX_LOGIN;

    /// <summary>
    /// 加载
    /// </summary>
    [Reactive]
    public bool Loading { get; set; } = false;

    /// <summary>
    /// 是否需要验证码
    /// </summary>
    [Reactive]
    public bool RequireVerifyCode { get; set; } = false;

    #endregion 页面属性

    #region 私有变量

    /// <summary>
    /// Steam 账号服务
    /// </summary>
    private static readonly ISteamAccountService _steamAccountService = Ioc.Get<ISteamAccountService>();

    /// <summary>
    /// 当前页面待导入的 Steam Authenticator
    /// </summary>
    private SteamAuthenticator? _currentSteamAuthenticator;

    private SteamLoginState? _currentSteamLoginState;

    #endregion 私有变量

    #region 联合 Steam令牌 页面流程

    /// <summary>
    /// 登录
    /// </summary>
    /// <returns></returns>
    public async Task LoginAsync()
    {
        if (Loading)
            return;

        Loading = true;

        try
        {
            if (string.IsNullOrEmpty(UserNameText) || string.IsNullOrEmpty(PasswordText))
            {
                Toast.Show(ToastIcon.Error, AppResources.Error_PleaseEnterUsernamePassword);
                return;
            }

            // 初始化登录状态
            var loginState = _currentSteamLoginState ??= new SteamLoginState();

            loginState.Username = UserNameText;
            loginState.Password = PasswordText;
            loginState.Language = ResourceService.GetCurrentCultureSteamLanguageName();

            if (RequireVerifyCode && loginState.RequiresEmailAuth)
            {
                loginState.EmailCode = VerifyCodeText;
            }

            try
            {
                // 开始登录
                await _steamAccountService.DoLoginV2Async(loginState);
            }
            catch (HttpRequestException)
            {
                Toast.Show(ToastIcon.Error, AppResources.LocalAuth_JoinSteamAuthenticator_LoginRequestError);
                return;
            }

            // 登陆结果
            if (!loginState.Success)
            {
                if (loginState.RequiresEmailAuth)
                {
                    RequireVerifyCode = true;
                    Toast.Show(ToastIcon.Info, Strings.LocalAuth_EmailCodeTip);
                    return;
                }
                else if (loginState.Requires2FA)
                {
                    Toast.Show(ToastIcon.Error, AppResources.LocalAuth_SteamUser_Requires2FA);
                    return;
                }
                else
                {
                    Toast.Show(ToastIcon.Error, loginState.Message ?? string.Empty);
                }

                return;
            }

            // 根据登录信息 获取/初始化 TwoFA Steam Authenticator 信息
            var (initialized, errMsg) = await InitTwoFASteamAuthenticatorAsync(loginState);

            if (!initialized)
            {
                Toast.Show(ToastIcon.Error, errMsg);
                return;
            }

            RevocationCodeText = _currentSteamAuthenticator!.RecoveryCode;
            // 跳转到验证页面
            SelectIndex = STEPINDEX_VERIFY;
        }
        finally
        {
            Loading = false;
        }
    }

    /// <summary>
    /// 校验验证码
    /// </summary>
    /// <returns></returns>
    public async Task VerifyCodeAsync()
    {
        if (Loading)
            return;

        Loading = true;
        try
        {
            if (string.IsNullOrWhiteSpace(Code))
            {
                Toast.Show(ToastIcon.Error, AppResources.LocalAuth_JoinSteamAuthenticator_SteamGuardCodeEmptyError);
                return;
            }

            var equals = await CompareCodeAsync();

            if (!equals)
            {
                Toast.Show(ToastIcon.Warning,
                    $@"{AppResources.LocalAuth_JoinSteamAuthenticator_VerifySteamGuardCodeError}: {Code} != {_currentSteamAuthenticator?.CurrentCode}");
                return;
            }

            RevocationCodeText = _currentSteamAuthenticator!.RecoveryCode;
            SelectIndex = STEPINDEX_DONE;
        }
        finally
        {
            Loading = false;
        }
    }

    /// <summary>
    /// 保存令牌
    /// </summary>
    /// <returns></returns>
    public async Task SaveAsync()
    {
        if (Loading)
            return;

        Loading = true;

        try
        {
            if (_currentSteamAuthenticator == null)
            {
                Toast.Show(ToastIcon.Error, AppResources.LocalAuth_JoinSteamAuthenticator_InitLocalAuthenticatorError);
                SelectIndex = STEPINDEX_LOGIN;
                return;
            }

            if (string.IsNullOrEmpty(Code))
            {
                Toast.Show(ToastIcon.Error, AppResources.LocalAuth_JoinSteamAuthenticator_SteamGuardCodeEmptyError);
                SelectIndex = STEPINDEX_VERIFY;
                return;
            }

            if (!await SaveAuthenticatorAsync(UserNameText!, _currentSteamAuthenticator))
            {
                Toast.Show(ToastIcon.Error, AppResources.LocalAuth_JoinSteamAuthenticator_SaveLocalAuthenticatorError);
                return;
            }

            // 保存完成重置
            Reset();
        }
        finally
        {
            Loading = false;
        }
    }

    /// <summary>
    /// 重置恢复代码
    /// </summary>
    /// <returns></returns>
    public async Task ResetRevocationCodeAsync()
    {
        if (_currentSteamLoginState == null)
        {
            return;
        }

        // 根据登录信息 获取/初始化 TwoFA Steam Authenticator 信息
        var (initialized, errMsg) = await InitTwoFASteamAuthenticatorAsync(_currentSteamLoginState);

        if (!initialized)
        {
            Toast.Show(ToastIcon.Error, errMsg);
            return;
        }

        RevocationCodeText = _currentSteamAuthenticator!.RecoveryCode;
    }

    #endregion 联合 Steam令牌 页面流程

    #region 辅助方法

    /// <summary>
    /// 初始化 TwoFA Steam Authenticator 信息
    /// </summary>
    /// <param name="loginState"></param>
    /// <returns></returns>
    private async Task<(bool initialized, string errorMsg)> InitTwoFASteamAuthenticatorAsync(SteamLoginState loginState)
    {
        _currentSteamAuthenticator = new SteamAuthenticator();

        string? steamId = loginState.SteamId != default
            ? loginState.SteamId.ToString()
            : null;

        if (string.IsNullOrEmpty(steamId))
            return (false, $"{AppResources.LocalAuth_JoinSteamAuthenticator_InitLocalAuthenticatorError}:{nameof(steamId)}");

        string deviceId = CreateNewDeviceId();

        var client = _currentSteamAuthenticator.GetClient();

        string authenticatorJson;

        try
        {
            authenticatorJson = await client
                .AddAuthenticatorAsync(
                        steamId,
                        DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                        deviceId,
                        loginState.AccessToken ?? string.Empty
                    );
        }
        catch (Exception ex)
        {
            return (false, $"{AppResources.LocalAuth_JoinSteamAuthenticator_RequestUser2FAInfoError}:{ex.Message}");
        }

        if (string.IsNullOrEmpty(authenticatorJson))
            return (false, AppResources.LocalAuth_JoinSteamAuthenticator_RequestUser2FAInfoError);

        var (initialized, msg) = TryParseSteamAuthenticator(authenticatorJson, out var parsedSteamData)
            ? (true, string.Empty)
            : (false, AppResources.LocalAuth_JoinSteamAuthenticator_ParseUser2FAInfoError);

        // 解析响应之后赋值给当前本地令牌
        if (initialized)
        {
            _currentSteamAuthenticator.DeviceId = deviceId;
            _currentSteamAuthenticator.SteamData = SystemTextJsonSerializer.Serialize(parsedSteamData!, SteamJsonContext.Default.SteamConvertSteamDataJsonStruct);
            _currentSteamAuthenticator.SecretKey = Base64Extensions.Base64DecodeToByteArray_Nullable(parsedSteamData!.SharedSecret);
            _currentSteamAuthenticator.Serial = parsedSteamData.SerialNumber;
        }

        return (initialized, msg);
    }

    /// <summary>
    /// 验证令牌验证码
    /// </summary>
    /// <returns></returns>
    private async Task<bool> CompareCodeAsync()
    {
        if (_currentSteamAuthenticator == null)
        {
            return false;
        }

        try
        {
            await Task.Yield();

            _currentSteamAuthenticator.Sync();
        }
        catch (Exception ex)
        {
            ex.LogAndShowT();
            return false;
        }

        bool compareResult = string.Equals(Code, _currentSteamAuthenticator?.CurrentCode, StringComparison.OrdinalIgnoreCase);

        return compareResult;
    }

    /// <summary>
    /// 解析 Steam Authenticator json响应
    /// </summary>
    /// <param name="authenticatorJson"></param>
    /// <param name="parsedSteamAuthenticator">已解析的响应信息</param>
    /// <returns></returns>
    private static bool TryParseSteamAuthenticator(string authenticatorJson, out SteamConvertSteamDataJsonStruct? parsedSteamAuthenticator)
    {
        parsedSteamAuthenticator = default;

        SteamDoLoginTfaJsonStruct? authenticatorDataResp;

        try
        {
            authenticatorDataResp = SystemTextJsonSerializer.Deserialize(
                   authenticatorJson,
                   SteamJsonContext.Default.SteamDoLoginTfaJsonStruct
               );
        }
        catch (Exception)
        {
            return false;
        }

        if (authenticatorDataResp == null || authenticatorDataResp.Response == null)
            return false;

        parsedSteamAuthenticator = authenticatorDataResp.Response;

        // 目前只需要知道是否含有令牌数据信息
        // 不需要知道当前状态可不可以绑定,绑定操作在 Steam App 中完成
        // 这里只是将令牌数据保存到本地 以便后续计算
        return parsedSteamAuthenticator != null;
    }

    /// <summary>
    /// 保存令牌
    /// </summary>
    /// <param name="authenticatorName"></param>
    /// <param name="authenticator"></param>
    /// <returns></returns>
    private static Task<bool> SaveAuthenticatorAsync(string authenticatorName, SteamAuthenticator authenticator)
    {
        var iADTO = new AuthenticatorDTO
        {
            Name = $"Steam({authenticatorName})",
            Value = authenticator,
            Created = DateTimeOffset.Now,
        };

        return AuthenticatorHelper.SaveAuthenticator(iADTO);
    }

    /// <summary>
    /// 创建新的设备ID
    /// </summary>
    /// <returns></returns>
    private static string CreateNewDeviceId() => $"android:{Guid.NewGuid()}";

    /// <summary>
    /// 重置页面绑定信息
    /// </summary>
    public void Reset()
    {
        UserNameText = default;
        PasswordText = default;
        Code = default;
        VerifyCodeText = default;
        RequireVerifyCode = default;
        RevocationCodeText = default;
        SelectIndex = STEPINDEX_LOGIN;

        _currentSteamAuthenticator = default;
        _currentSteamLoginState = default;
    }

    #endregion 辅助方法
}