using BD.SteamClient.Models;
using BD.SteamClient.Services;
using WinAuth;

namespace BD.WTTS.UI.ViewModels;

/// <summary>
/// 联合Steam令牌页面
/// </summary>
public sealed partial class JoinSteamAuthenticatorPageViewModel : ViewModelBase
{
    #region 常量

    const int STEPINDEX_LOGIN = 0;

    const int STEPINDEX_VERIFY = 1;

    const int STEPINDEX_DONE = 2;

    #endregion 常量

    #region 页面属性

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
    /// 恢复代码
    /// </summary>
    [Reactive]
    public string? RevocationCodeText { get; set; }

    [Reactive]
    public int SelectIndex { get; set; } = STEPINDEX_LOGIN;

    [Reactive]
    public bool Loading { get; set; } = false;

    #endregion 页面属性

    #region 私有变量

    private static readonly ISteamAccountService _steamAccountService = Ioc.Get<ISteamAccountService>();

    private SteamAuthenticator? _currentSteamAuthenticator;

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
                Toast.Show(ToastIcon.Error, "用户名或密码不能为空!");
                return;
            }

            // 初始化登录状态
            var loginState = new SteamLoginState()
            {
                Username = UserNameText,
                Password = PasswordText,
            };
            try
            {
                // 开始登录
                await _steamAccountService.DoLoginV2Async(loginState);
            }
            catch (HttpRequestException)
            {
                Toast.Show(ToastIcon.Error, "登录请求错误!");
                return;
            }
            catch (Exception ex)
            {
                Toast.Show(ToastIcon.Error, $"登录异常!");
                return;
            }

            // 登陆结果
            if (!loginState.Success)
            {
                if (loginState.Requires2FA || loginState.RequiresEmailAuth)
                {
                    Toast.Show(ToastIcon.Error, "当前账号已有令牌!");
                    return;
                }
                else
                {
                    Toast.Show(ToastIcon.Error, loginState.Message ?? "登陆失败!");
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
                Toast.Show(ToastIcon.Error, "Steam 令牌验证码不能为空!");
                return;
            }

            var equals = await CompareCodeAsync();

            if (!equals)
            {
                Toast.Show(ToastIcon.Warning, "令牌验证失败!");
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
                Toast.Show(ToastIcon.Error, "当前令牌信息为空!");
                SelectIndex = STEPINDEX_LOGIN;
                return;
            }

            if (string.IsNullOrEmpty(Code))
            {
                Toast.Show(ToastIcon.Error, "令牌不能为空!");
                SelectIndex = STEPINDEX_VERIFY;
                return;
            }

            if (!await SaveAuthenticatorAsync(UserNameText!, _currentSteamAuthenticator))
            {
                Toast.Show(ToastIcon.Error, "保存令牌失败!");
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

    #endregion 联合 Steam令牌 页面流程

    #region 辅助方法

    public void Reset()
    {
        UserNameText = default;
        PasswordText = default;
        Code = default;
        RevocationCodeText = default;
        SelectIndex = STEPINDEX_LOGIN;
    }

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
            return (false, "登录信息失败!");

        var client = _currentSteamAuthenticator.GetClient();

        string deviceId = $"android:{Guid.NewGuid()}";

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
            return (false, $"请求令牌信息失败:{ex.Message}");
        }

        if (string.IsNullOrEmpty(authenticatorJson))
            return (false, "获取令牌信息失败!");

        var result = SetSteamAuthenticator(_currentSteamAuthenticator, authenticatorJson)
            ? (true, string.Empty)
            : (false, "解析令牌信息失败!");

        return result;
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
            Toast.Show(ToastIcon.Error, "同步令牌失败!");
            ex.LogAndShowT();
            return false;
        }

        bool compareResult = string.Equals(Code, _currentSteamAuthenticator?.CurrentCode, StringComparison.OrdinalIgnoreCase);

        return compareResult;
    }

    /// <summary>
    /// 设置令牌信息
    /// </summary>
    /// <param name="authenticator"></param>
    /// <param name="authenticatorJson"></param>
    /// <returns></returns>
    private static bool SetSteamAuthenticator(SteamAuthenticator authenticator, string authenticatorJson)
    {
        var authenticatorDataResp = SystemTextJsonSerializer.Deserialize(
                    authenticatorJson,
                    SteamJsonContext.Default.SteamDoLoginTfaJsonStruct
                );

        if (authenticatorDataResp == null || authenticatorDataResp.Response == null)
            return false;

        var data = authenticatorDataResp.Response;

        authenticator.SteamData = SystemTextJsonSerializer.Serialize(data, SteamJsonContext.Default.SteamConvertSteamDataJsonStruct);
        authenticator.SecretKey = Base64Extensions.Base64DecodeToByteArray_Nullable(data.SharedSecret);
        authenticator.Serial = data.SerialNumber;

        return true;
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

    #endregion 辅助方法
}