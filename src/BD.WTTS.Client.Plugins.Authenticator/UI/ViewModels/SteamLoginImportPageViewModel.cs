using BD.SteamClient.Models;
using BD.SteamClient.Services;
using BD.WTTS.UI.Views.Controls;
using System.Net.Http;
using WinAuth;
using AppResources = BD.WTTS.Client.Resources.Strings;

namespace BD.WTTS.UI.ViewModels;

public sealed partial class SteamLoginImportPageViewModel : ViewModelBase
{
    const int TAB_INDEX_LOGIN = 0;
    const int TAB_INDEX_LOGINCONFIRM = 1;
    const int TAB_INDEX_VERIFYCODE = 2;
    const int TAB_INDEX_DONE = 3;

    public static string Name => AppResources.Auth_SteamLoginImport;

    readonly ISteamAccountService _steamAccountService = Ioc.Get<ISteamAccountService>();

    SteamAuthenticator steamAuthenticator = new();

    readonly CancellationTokenSource lifetime = new();

    SteamAuthenticator.EnrollState _enrollState = new();

    bool hasAuthenticatedSession;
    bool requiresAuthenticatorStatus;
    string? sessionId;

    public SteamLoginImportPageViewModel()
    {
        Login = CreateLogin();
    }

    SteamLoginViewModel CreateLogin()
    {
        var login = new SteamLoginViewModel(accountService: _steamAccountService, requireMobileSession: true);
        login.LoginSucceededAsync = OnLoginSucceededAsync;
        return login;
    }

    /// <summary>
    /// 重置状态
    /// </summary>
    public void Reset()
    {
        Login.Dispose();
        Login = CreateLogin();
        steamAuthenticator = new();
        _enrollState = new();
        hasAuthenticatedSession = false;
        requiresAuthenticatorStatus = false;
        sessionId = null;
        PhoneCodeText = null;
        PhoneNumberText = null;
        RevocationCodeText = null;
        IsVerifyAccountPhone = false;
        SelectIndex = TAB_INDEX_LOGIN;
    }

    async Task OnLoginSucceededAsync(SteamLoginState state)
    {
        if (Disposed || !state.Success || string.IsNullOrEmpty(state.AccessToken) || state.SteamId == 0)
            return;

        _enrollState = new()
        {
            Username = state.Username,
            Language = state.Language,
            AccessToken = state.AccessToken,
            RefreshToken = state.RefreshToken,
            SteamId = (long)state.SteamId,
            ReplaceAuth = Login.HadTwoFactorChallenge,
        };
        sessionId = state.SeesionId;
        hasAuthenticatedSession = true;
        // QR approval bypasses the credentials flow's existing-authenticator challenge.
        requiresAuthenticatorStatus = Login.UsedQrCode;
        SelectIndex = TAB_INDEX_LOGINCONFIRM;
        await LoginSteamImport();
    }

    async Task<bool?> HasAuthenticatorAsync()
    {
        using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
        using var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["steamid"] = _enrollState.SteamId.ToString(CultureInfo.InvariantCulture),
            ["access_token"] = _enrollState.AccessToken!,
        });
        using var response = await client.PostAsync("https://api.steampowered.com/ITwoFactorService/QueryStatus/v1/", content, lifetime.Token);
        response.EnsureSuccessStatusCode();
        if (response.Headers.TryGetValues("x-eresult", out var results) && results.Any(x => x != "1"))
            return null;
        return ParseAuthenticatorStatus(await response.Content.ReadAsStringAsync(lifetime.Token));
    }

    internal static bool? ParseAuthenticatorStatus(string json)
    {
        try
        {
            using var document = JsonDocument.Parse(json);
            if (document.RootElement.ValueKind != JsonValueKind.Object ||
                !document.RootElement.TryGetProperty("response", out var response) ||
                response.ValueKind != JsonValueKind.Object ||
                !response.TryGetProperty("state", out var state) ||
                state.ValueKind != JsonValueKind.Number || !state.TryGetUInt32(out var stateValue))
                return null;

            uint? authenticatorType = null;
            if (response.TryGetProperty("authenticator_type", out var type))
            {
                if (type.ValueKind != JsonValueKind.Number || !type.TryGetUInt32(out var value))
                    return null;
                authenticatorType = value;
            }

            uint? scheme = null;
            if (response.TryGetProperty("steamguard_scheme", out var guardScheme))
            {
                if (guardScheme.ValueKind != JsonValueKind.Number || !guardScheme.TryGetUInt32(out var value))
                    return null;
                scheme = value;
            }

            return stateValue switch
            {
                1 when (authenticatorType is null or 1) && (scheme is null or 2) => true,
                0 when (authenticatorType is null or 0) && (scheme is null or 0 or 1) => false,
                _ => null,
            };
        }
        catch (System.Text.Json.JsonException)
        {
            return null;
        }
    }

    public async Task<bool> FinalizeAddAuthenticatorAsync()
    {
        if (string.IsNullOrEmpty(PhoneCodeText))
        {
            Toast.Show(ToastIcon.Error, AppResources.Error_PleaseEnterCode);
            return false;
        }
        else
            _enrollState.ActivationCode = PhoneCodeText;

        await steamAuthenticator.FinalizeAddAuthenticatorAsync(_enrollState);

        PhoneCodeText = null;
        return _enrollState.Success;
    }

    public async Task SaveAuthenticatorAsync()
    {
        steamAuthenticator.SessionData = new WinAuth.SteamClient.SteamSession
        {
            SteamID = (ulong)_enrollState.SteamId,
            AccessToken = _enrollState.AccessToken,
            RefreshToken = _enrollState.RefreshToken,
            SessionID = sessionId,
        }.ToString();
        steamAuthenticator.GetClient(_enrollState.Language);
        var accountName = steamAuthenticator.AccountName;
        if (string.IsNullOrWhiteSpace(accountName))
            accountName = _enrollState.Username;
        if (string.IsNullOrWhiteSpace(accountName))
            accountName = _enrollState.SteamId.ToString(CultureInfo.InvariantCulture);
        var iADTO = new AuthenticatorDTO()
        {
            Name = $"Steam({accountName})",
            Value = steamAuthenticator,
            Created = DateTimeOffset.Now,
        };
        await AuthenticatorHelper.SaveAuthenticator(iADTO);
        SelectIndex = TAB_INDEX_DONE;
    }

    /// <summary>
    /// 检查添加令牌是否成功
    /// </summary>
    /// <returns></returns>
    async Task CheckAddAuthenticatorResult()
    {
        if (string.IsNullOrEmpty(_enrollState.Error) == false)
        {
            if (_enrollState.Error.Length > 50)
                await IWindowManager.Instance.ShowTaskDialogAsync(
                    new MessageBoxWindowViewModel { Content = _enrollState.Error });
            else
                Toast.Show(ToastIcon.Info, _enrollState.Error);
        }

        if (_enrollState.NoPhoneNumber == true)
        {
            _enrollState.Error = null;
            //SelectIndex = 2;
            // 正常情况下不会走到这里，因为已经不需要绑定手机的步骤了
            return;
        }

        // 导入最后一步，需要账号绑定的手机验证码确认
        // 2024-04-23 Steam绑定令牌不强制手机绑定 可以输入 邮箱者手机验证码
        if (_enrollState.RequiresActivation == true)
        {
            // 查询账号手机号绑定状态
            // IsVerifyAccountPhone = await _steamAccountService.CheckAccountPhoneStatus(_enrollState.AccessToken!) ?? false;

            await CheckAccountPhoneStatus();

            _enrollState.Error = null;
            RevocationCodeText = _enrollState.RevocationCode;
            SelectIndex = TAB_INDEX_VERIFYCODE;
        }
    }

    async ValueTask CheckAccountPhoneStatus()
    {
        // 查询账号手机号绑定状态
        IsVerifyAccountPhone = await _steamAccountService.CheckAccountPhoneStatus(_enrollState.AccessToken!) ?? false;
    }

    public async Task LoginSteamImport()
    {
        if (Disposed || !hasAuthenticatedSession || string.IsNullOrEmpty(_enrollState.AccessToken) ||
            SelectIndex is not (TAB_INDEX_LOGINCONFIRM or TAB_INDEX_VERIFYCODE))
            return;
        if (IsLoading)
        {
            Toast.Show(ToastIcon.Warning, AppResources.Warning_DoNotOperateFrequently);
            return;
        }
        IsLoading = true;

        try
        {
            if (requiresAuthenticatorStatus)
            {
                var hasAuthenticator = await HasAuthenticatorAsync();
                if (!hasAuthenticator.HasValue)
                {
                    Toast.Show(ToastIcon.Error, Strings.LocalAuth_SteamUser_Error);
                    return;
                }
                _enrollState.ReplaceAuth = hasAuthenticator.Value;
                requiresAuthenticatorStatus = false;
            }

            //已有令牌情况下执行替换令牌逻辑
            if (_enrollState.ReplaceAuth)
            {
                if (SelectIndex == TAB_INDEX_LOGINCONFIRM && await steamAuthenticator.RemoveAuthenticatorViaChallengeStartSync(_enrollState.AccessToken!))
                {
                    await CheckAccountPhoneStatus();

                    SelectIndex = TAB_INDEX_VERIFYCODE;

                    if (!IsVerifyAccountPhone)
                    {
                        Toast.Show(ToastIcon.Warning, "只绑定邮箱的情况下无法直接替换令牌");

                        // 弹出绑定手机号流程
                        var added = await IWindowManager.Instance.ShowTaskDialogAsync(
                                viewModel: new AddSteamAccountPhoneNumberPageViewModel(_enrollState, steamAuthenticator),
                                pageContent: new AddSteamAccountPhoneNumberPage(),
                                isCancelButton: false,
                                isOkButton: false,
                                isRetryButton: false
                                );

                        // 查询账号手机号绑定状态
                        await CheckAccountPhoneStatus();

                        // 如果通过弹出的绑定手机号绑定成功 开始执行替换令牌
                        if (added && IsVerifyAccountPhone)
                        {
                            /*
                             * 绑定手机号会发送验证码
                             * 执行开始替换令牌操作也会发送验证码
                             * 如果两个操作间隔太短会导致 Steam 不发送验证码
                             */

                            await Task.Delay(TimeSpan.FromSeconds(30), lifetime.Token);

                            await steamAuthenticator.RemoveAuthenticatorViaChallengeStartSync(_enrollState.AccessToken!);
                        }
                        // 取消了绑定手机号流程 无法执行替换令牌操作
                        else
                        {
                            Reset();
                        }

                        return;
                    }
                    return;
                }
                else if (SelectIndex == TAB_INDEX_VERIFYCODE)
                {
                    if (string.IsNullOrEmpty(PhoneCodeText))
                    {
                        Toast.Show(ToastIcon.Error, Strings.Error_PleaseEnterCode);
                        return;
                    }

                    if (await steamAuthenticator.RemoveAuthenticatorViaChallengeContinueSync(PhoneCodeText, _enrollState.AccessToken!))
                    {
                        await SaveAuthenticatorAsync();
                        RevocationCodeText = steamAuthenticator.RecoveryCode;
                        return;
                    }
                }

                var error = string.IsNullOrEmpty(_enrollState.Error) ? Strings.LocalAuth_SteamUser_Error : _enrollState.Error;
                Toast.Show(ToastIcon.Error, error);
            }
            else if (_enrollState.RequiresActivation) //是否需激活令牌
            {
                if (await FinalizeAddAuthenticatorAsync())
                {
                    await SaveAuthenticatorAsync();
                    return;
                }
                else
                {
                    var error = string.IsNullOrEmpty(_enrollState.Error)
                        ? Strings.LocalAuth_SteamUser_Error
                        : _enrollState.Error;
                    Toast.Show(ToastIcon.Error, error);
                }
            }
            else
            {
                if (_enrollState.NoPhoneNumber)
                {
                    if (string.IsNullOrEmpty(PhoneNumberText))
                    {
                        Toast.Show(ToastIcon.Warning, AppResources.Warning_PleaseEnterTel);
                        return;
                    }

                    var reslut = await steamAuthenticator.AddPhoneNumberAsync(_enrollState, PhoneNumberText);

                    if (_enrollState.RequiresEmailConfirmPhone)
                    {
                        while (await MessageBox.ShowAsync(Strings.Steam_EmailConfirmation.Format(_enrollState.PhoneNumber, _enrollState.EmailDomain), button: MessageBox.Button.OKCancel) == MessageBox.Result.OK)
                        {
                            reslut = await steamAuthenticator.AddPhoneNumberAsync(_enrollState, PhoneNumberText);
                            if (_enrollState.RequiresEmailConfirmPhone == false && reslut == null)
                                break;
                        }
                    }

                    if (reslut != null)
                    {
                        Toast.Show(ToastIcon.Info, reslut);
                        return;
                    }
                }

                await steamAuthenticator.AddAuthenticatorAsync(_enrollState);

                await CheckAddAuthenticatorResult();
            }
        }
        catch (OperationCanceledException) when (lifetime.IsCancellationRequested)
        {
        }
        catch (WinAuthSteamToManyRequestException ex)
        {
            Toast.Show(ToastIcon.Error, ex.Message);
        }
        catch (WinAuthInvalidEnrollResponseException ex)
        {
            Toast.Show(ToastIcon.Error, ex.Message);
        }
        catch (Exception ex)
        {
            ex.LogAndShowT();
            //Log.Error(nameof(AuthenticatorHomePageViewModel), ex, nameof(LoginSteamImport));
            //await IWindowManager.Instance.ShowTaskDialogAsync(new MessageBoxWindowViewModel
            //{
            //    Content = ex.ToString()
            //});
        }
        finally
        {
            IsLoading = false;
        }
    }

    public override void Deactivation()
    {
        Login.Cancel();
        base.Deactivation();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && !Disposed)
        {
            Login.Dispose();
            lifetime.Cancel();
            lifetime.Dispose();
        }
        base.Dispose(disposing);
    }
}
