using static BD.WTTS.Services.AuthenticatorHelper;

namespace BD.WTTS.UI.ViewModels;

public sealed partial class AuthenticatorHomePageViewModel
{
    bool sessionVerifiedWithHello;
    VaultUnlockMethod unlockMethod;

    [Reactive]
    public partial bool HasVaultData { get; private set; }

    [Reactive]
    public partial bool ProtectionUpgradeAvailable { get; private set; }

    [Reactive]
    public partial bool WindowsHelloEnabled { get; private set; }

    [Reactive]
    public partial string? VaultReadError { get; private set; }

    public bool ShowWindowsHello => OperatingSystem.IsWindowsVersionAtLeast(10, 0, 22000) ||
        (OperatingSystem.IsWindows() && WindowsHelloEnabled);

    public bool CanUnlockVault => OperatingSystem.IsWindows() || !WindowsHelloEnabled;

    public bool CanLockVault => WindowsHelloEnabled || HasPasswordEncrypt;

    public string UnlockVaultText => unlockMethod switch
    {
        VaultUnlockMethod.WindowsHello => Strings.Auth_VaultHelloUnlock,
        VaultUnlockMethod.LegacyPassword => Strings.Auth_VaultHelloRecovery,
        _ => Strings.Verify,
    };

    public string VaultLockedMessage => unlockMethod switch
    {
        VaultUnlockMethod.WindowsHello when !CanUnlockVault => Strings.Auth_VaultLocalReadFailed,
        VaultUnlockMethod.WindowsHello => Strings.Auth_VaultHelloLocked,
        VaultUnlockMethod.LegacyPassword => Strings.Auth_VaultLegacyHelloLocked,
        _ => Strings.Auth_PasswordProtectedVerifyFirst,
    };

    public string WindowsHelloActionText => WindowsHelloEnabled ? Strings.Auth_VaultHelloDisable : Strings.Auth_VaultHelloEnable;

    public string PasswordActionText => HasPasswordEncrypt ? Strings.Auth_VaultChangePassword : Strings.Auth_AddPasswordProtect;

    public string LocalProtectionActionText => HasLocalPcEncrypt ? Strings.Auth_RemoveMachineValidation : Strings.Auth_AddMachineValidation;

    public string VaultProtectionSummary => WindowsHelloEnabled ? Strings.Auth_VaultHelloSummary
        : HasPasswordEncrypt ? Strings.Auth_VaultPasswordSummary : Strings.Auth_VaultNoLockSummary;

    public void Initialize() => _ = LoadVaultAsync();

    public Task UnlockVaultAsync() => LoadVaultAsync(unlock: true);

    Task LoadVaultAsync(bool checkCache = false, bool unlock = false) => RunVaultOperationAsync(async () =>
    {
        var sources = await AuthenticatorHelper.GetAllSourceAuthenticatorAsync();
        if (Disposed) return;
        UpdateProtectionState(sources);

        if (IsVerificationPass && (WindowsHelloEnabled && !sessionVerifiedWithHello || HasPasswordEncrypt && _currentPassword == null))
            ClearVaultSession();
        if (checkCache && IsVerificationPass && loadedSources != null && SourcesEqual(loadedSources, sources))
            return;

        var password = HasPasswordEncrypt ? _currentPassword : null;
        var verifiedWithHello = WindowsHelloEnabled && sessionVerifiedWithHello;
        if (!IsVerificationPass && unlockMethod != VaultUnlockMethod.None)
        {
            if (!unlock || !CanUnlockVault) return;
            var access = await RequestVaultUnlockAsync(unlockMethod);
            if (Disposed || !access.authorized) return;
            (password, verifiedWithHello) = (access.password, access.verifiedWithHello);
        }

        var items = await GetAllAuthenticatorsAsync(sources, password, verifiedWithHello);
        if (Disposed) return;
        // An outside import or protection change while the prompt was open invalidates this snapshot.
        if (unlock && !IsVerificationPass && !SourcesEqual(sources, await AuthenticatorHelper.GetAllSourceAuthenticatorAsync()))
            throw new InvalidOperationException("Authenticator vault changed during verification.");
        if (Disposed) return;

        if (unlockMethod == VaultUnlockMethod.LegacyPassword)
        {
            ClearLegacyHelloPreference();
            unlockMethod = VaultUnlockMethod.Password;
            RaiseProtectionPresentation();
        }
        sessionVerifiedWithHello = verifiedWithHello;
        _currentPassword = password;
        loadedSources = sources;
        AuthSource.Edit(updater =>
        {
            updater.Clear();
            updater.AddOrUpdate(items.Select(static item => new AuthenticatorItemModel(item)));
        });
        IsVerificationPass = true;
        UpdateTrayMenu();
    }, lockOnReadFailure: true);

    void UpdateProtectionState(AccountPlatformAuthenticator[] sources)
    {
        HasVaultData = sources.Length != 0;
        (HasLocalPcEncrypt, HasPasswordEncrypt) = AuthenticatorHelper.HasEncrypt(sources);
        ProtectionUpgradeAvailable = AuthenticatorHelper.NeedsProtectionUpgrade(sources);
        unlockMethod = GetUnlockMethod(sources, HasPasswordEncrypt);
        WindowsHelloEnabled = unlockMethod == VaultUnlockMethod.WindowsHello;
        RaiseProtectionPresentation();
    }

    void RaiseProtectionPresentation()
    {
        this.RaisePropertyChanged(nameof(ShowWindowsHello));
        this.RaisePropertyChanged(nameof(CanUnlockVault));
        this.RaisePropertyChanged(nameof(CanLockVault));
        this.RaisePropertyChanged(nameof(UnlockVaultText));
        this.RaisePropertyChanged(nameof(VaultLockedMessage));
        this.RaisePropertyChanged(nameof(WindowsHelloActionText));
        this.RaisePropertyChanged(nameof(PasswordActionText));
        this.RaisePropertyChanged(nameof(LocalProtectionActionText));
        this.RaisePropertyChanged(nameof(VaultProtectionSummary));
    }

    void ClearVaultSession()
    {
        IsVerificationPass = false;
        _currentPassword = null;
        _currentAnswer = null;
        loadedSources = null;
        sessionVerifiedWithHello = false;
        if (AuthSource.Count == 0) return;
        AuthSource.Clear();
        UpdateTrayMenu();
    }

    async Task RunVaultOperationAsync(Func<Task> operation, bool lockOnReadFailure = false)
    {
        if (Disposed || IsLoading) return;
        IsLoading = true;
        await reorderLock.WaitAsync();
        try
        {
            if (Disposed) return;
            VaultReadError = null;
            await operation();
        }
        catch (AuthenticatorReadException ex)
        {
            if (Disposed) return;
            if (lockOnReadFailure) ClearVaultSession();
            VaultReadError = AuthenticatorHelper.ReadErrorText(ex.ResultCode);
            if (IsVerificationPass) Toast.Show(ToastIcon.Warning, VaultReadError);
        }
        catch (Exception ex)
        {
            if (Disposed) return;
            if (lockOnReadFailure) ClearVaultSession();
            Log.Error(nameof(AuthenticatorHomePageViewModel), ex, "Authenticator vault operation failed.");
            VaultReadError = lockOnReadFailure ? Strings.Auth_VaultReadFailed : Strings.Auth_VaultChangeFailed;
            if (IsVerificationPass) Toast.Show(ToastIcon.Error, VaultReadError);
        }
        finally
        {
            reorderLock.Release();
            IsLoading = false;
        }
    }

    public Task SetPasswordProtection() => RunVaultOperationAsync(async () =>
    {
        if (!CanManageVault()) return;
        if (WindowsHelloEnabled && !await ConfirmAsync(Strings.Auth_AddPasswordProtect, Strings.Auth_VaultSwitchToPasswordTip)) return;
        var current = await RequestCurrentPasswordAsync();
        if (!current.confirmed) return;

        var password = await AuthenticatorHelper.RequestPasswordAsync(Strings.Auth_VaultNewPassword, Strings.Auth_VaultPasswordGuidance, isNew: true);
        if (password == null) return;
        if (!AuthenticatorHelper.IsValidNewPassword(password))
        {
            Toast.Show(ToastIcon.Warning, Strings.Auth_VaultPasswordGuidance);
            return;
        }
        var confirmation = await AuthenticatorHelper.RequestPasswordAsync(Strings.Title_PasswordConfirm, isNew: true);
        if (confirmation == null) return;
        if (!string.Equals(password, confirmation, StringComparison.Ordinal))
        {
            Toast.Show(ToastIcon.Warning, Strings.LocalAuth_ProtectionAuth_PasswordErrorTip);
            return;
        }

        await ChangeProtectionAsync(HasLocalPcEncrypt, current.password, password);
        Toast.Show(ToastIcon.Success, Strings.Success_AuthPasswordSetSuccessfully);
    });

    public Task RemovePasswordProtection() => RunVaultOperationAsync(async () =>
    {
        if (!CanManageVault() || !HasPasswordEncrypt) return;
        var current = await RequestCurrentPasswordAsync();
        if (!current.confirmed) return;
        if (!await ConfirmAsync(Strings.Auth_RemovePasswordProtect, Strings.Auth_VaultRemovePasswordTip)) return;

        await ChangeProtectionAsync(HasLocalPcEncrypt, current.password, null);
        Toast.Show(ToastIcon.Success, Strings.Success_AuthPasswordRemovedSuccessfully);
    });

    public Task ToggleLocalProtection() => RunVaultOperationAsync(async () =>
    {
        if (!CanManageVault() || WindowsHelloEnabled) return;
        var enable = !HasLocalPcEncrypt;
        if (!await ConfirmAsync(enable ? Strings.Auth_AddMachineValidation : Strings.Auth_RemoveMachineValidation,
                enable ? Strings.Auth_VaultLocalRecoveryTip : Strings.Auth_VaultRemoveLocalTip)) return;
        var current = await RequestCurrentPasswordAsync();
        if (!current.confirmed) return;

        await ChangeProtectionAsync(enable, current.password, current.password);
        Toast.Show(ToastIcon.Success, Strings.Success_AuthProtectSuccessfully_.Format(enable ? Strings.Enable : Strings.Disable));
    });

    public Task UpgradeVaultProtectionAsync() => RunVaultOperationAsync(async () =>
    {
        if (!CanManageVault() || !ProtectionUpgradeAvailable) return;
        var current = await RequestCurrentPasswordAsync();
        if (!current.confirmed) return;
        if (!await ConfirmAsync(Strings.Auth_VaultUpgrade, Strings.Auth_VaultUpgradeTip)) return;

        await ChangeProtectionAsync(HasLocalPcEncrypt, current.password, current.password, WindowsHelloEnabled);
        Toast.Show(ToastIcon.Success, Strings.Auth_VaultUpgrade);
    });

    public Task ToggleWindowsHelloAsync() => RunVaultOperationAsync(async () =>
    {
        if (!CanManageVault()) return;
        if (WindowsHelloEnabled)
        {
            if (!await ConfirmAsync(Strings.Auth_VaultHelloDisable, Strings.Auth_VaultHelloDisableTip)) return;
            await ChangeProtectionAsync(isLocal: true, currentPassword: null, newPassword: null);
            Toast.Show(ToastIcon.Success, Strings.Auth_VaultHelloDisabled);
            return;
        }

        if (!await IPlatformService.Instance.IsSupportedBiometricAsync())
        {
            Toast.Show(ToastIcon.Warning, Strings.Auth_VaultHelloUnavailable);
            return;
        }
        if (!await ConfirmAsync(Strings.Auth_VaultHelloEnable, Strings.Auth_VaultHelloEnableTip) ||
            !await VerifyWindowsHelloAsync() || Disposed) return;

        await ChangeProtectionAsync(isLocal: true, _currentPassword, newPassword: null, windowsHello: true, verifiedWithHello: true);
        Toast.Show(ToastIcon.Success, Strings.Auth_VaultHelloEnabled);
    });

    bool CanManageVault() => !Disposed && HasVaultData && IsVerificationPass;

    async Task<(bool confirmed, string? password)> RequestCurrentPasswordAsync()
    {
        if (!HasPasswordEncrypt) return (true, null);
        var password = await AuthenticatorHelper.RequestPasswordAsync(Strings.Title_InputAuthPassword);
        return (!string.IsNullOrWhiteSpace(password), password);
    }

    async Task ChangeProtectionAsync(bool isLocal, string? currentPassword, string? newPassword,
        bool windowsHello = false, bool verifiedWithHello = false)
    {
        if (!CanManageVault()) return;
        await SwitchEncryptionAuthenticators(isLocal, currentPassword, newPassword, windowsHello,
            verifiedWithHello: verifiedWithHello || sessionVerifiedWithHello);
        if (Disposed) return;
        if (unlockMethod == VaultUnlockMethod.LegacyPassword)
            ClearLegacyHelloPreference();
        (HasLocalPcEncrypt, HasPasswordEncrypt, WindowsHelloEnabled) = (isLocal, !string.IsNullOrEmpty(newPassword), windowsHello);
        unlockMethod = windowsHello ? VaultUnlockMethod.WindowsHello
            : HasPasswordEncrypt ? VaultUnlockMethod.Password : VaultUnlockMethod.None;
        sessionVerifiedWithHello = windowsHello;
        ProtectionUpgradeAvailable = false;
        _currentPassword = newPassword;
        loadedSources = null;
        RaiseProtectionPresentation();
    }

    static Task<bool> ConfirmAsync(string title, string text) => IWindowManager.Instance.ShowTaskDialogAsync(
        new MessageBoxWindowViewModel { Content = text }, title, isCancelButton: true);
}
