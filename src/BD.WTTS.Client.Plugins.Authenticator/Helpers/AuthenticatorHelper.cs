using WinAuth;
using Avalonia.Controls;
using BD.WTTS.Serialization;
using static BD.WTTS.Repositories.Abstractions.IAccountPlatformAuthenticatorRepository;
using AppResources = BD.WTTS.Client.Resources.Strings;

namespace BD.WTTS.Services;

public static class AuthenticatorHelper
{
    static IAccountPlatformAuthenticatorRepository repository = Ioc.Get<IAccountPlatformAuthenticatorRepository>();

    public static async Task<(bool isSuccess, bool isUpdate)> AddOrUpdateSaveAuthenticatorsAsync(IAuthenticatorDTO authenticatorDto,
        string? password, bool isLocal, bool verifiedWithHello = false)
    {
        //var isLocal = await repository.HasLocalAsync();
        // var sourceList = await GetAllSourceAuthenticatorAsync();
        // if (sourceList.Length >= IAccountPlatformAuthenticatorRepository.MaxValue)
        // {
        //     if (sourceList.FirstOrDefault(i => i.Id == authenticatorDto.Id) == null)
        //     {
        //         Toast.Show(ToastIcon.Error, "操作失败：超出令牌最大数量限制");
        //         return false;
        //     }
        // }s
        // if (await repository.Exists(sourceList, authenticatorDto, isLocal, password))
        // {
        //     Toast.Show(ToastIcon.Error, "操作失败：令牌已存在重复数据");
        //     return false;
        // }
        return await repository.InsertOrUpdateAsync(authenticatorDto, isLocal, password, verifiedWithHello);
    }

    // public static async Task<bool> AddOrUpdateSaveAndSyncAuthenticatorAsync(string? password,
    //     params IAuthenticatorDTO[] authenticatorDto)
    // {
    //     foreach (var auth in authenticatorDto)
    //     {
    //         await AddOrUpdateSaveAuthenticatorsAsync(auth, password);
    //     }
    //     
    //     var pushItems = authenticatorDto.Select(item => new UserAuthenticatorPushItem()
    //     {
    //         Id = item.ServerId,
    //         TokenType = item.Platform == AuthenticatorPlatform.HOTP
    //             ? UserAuthenticatorTokenType.HOTP
    //             : UserAuthenticatorTokenType.TOTP,
    //         Name = item.Name,
    //         Order = item.Index,
    //         Token = MemoryPackSerializer.Serialize(item.ToExport()),
    //     }).ToArray();
    //     
    //     return true;
    // }

    public static async Task<AccountPlatformAuthenticator[]> GetAllSourceAuthenticatorAsync()
    {
        return await repository.GetAllSourceAsync();
    }

    public static async Task<List<IAuthenticatorDTO>> GetAllAuthenticatorsAsync(AccountPlatformAuthenticator[] source,
        string? password = null, bool verifiedWithHello = false)
    {
        return await repository.ConvertToListAsync(source, password, verifiedWithHello);
    }

    public static async Task DeleteAllAuthenticatorsAsync()
    {
        var list = await repository.GetAllSourceAsync();
        foreach (var item in list)
            await repository.DeleteAsync(item.Id);
    }

    public static async Task DeleteAuth(IAuthenticatorDTO authenticatorDto)
    {
        if (authenticatorDto.ServerId.HasValue)
            await repository.DeleteAsync(authenticatorDto.ServerId.Value);
        await repository.DeleteAsync(authenticatorDto.Id);
    }

    public static async Task<bool> SaveEditAuthNameAsync(IAuthenticatorDTO authenticatorDto, string newName)
    {
        var access = await GetCurrentPassword();
        if (!access.authorized) return false;
        await repository.RenameAsync(authenticatorDto.Id, newName, access.hasLocalPcEncrypt, access.password, access.verifiedWithHello);
        return true;
    }

    public static async Task<(IAccountPlatformAuthenticatorRepository.ImportResultCode resultCode, IReadOnlyList<IAuthenticatorDTO> result, int sourcesCount)>
        ImportAsync(string? exportPassword, byte[] data)
    {
        return await repository.ImportAsync(exportPassword, data);
    }

    public static (bool haslocal, bool haspassword) HasEncrypt(AccountPlatformAuthenticator[] sourceData)
    {
        var hasLocal = repository.HasLocal(sourceData);

        var hasPassword = repository.HasSecondaryPassword(sourceData);

        return (hasLocal, hasPassword);
    }

    internal static bool NeedsProtectionUpgrade(IEnumerable<AccountPlatformAuthenticator> sources) =>
        sources.Any(AccountPlatformAuthenticatorRepository.NeedsProtectionUpgrade);

    internal static Task SwitchEncryptionAuthenticators(bool isLocal, string? currentPassword, string? newPassword,
        bool windowsHello = false, bool verifiedWithHello = false) =>
        repository.SwitchEncryptionModeAsync(isLocal, currentPassword, newPassword, windowsHello, verifiedWithHello);

    internal static Task SaveImportedAuthenticatorsAsync(IReadOnlyList<IAuthenticatorDTO> items, bool isLocal,
        string? password, bool verifiedWithHello) =>
        repository.InsertOrUpdateAsync(items, isLocal, password, verifiedWithHello);

    internal const int MinNewPasswordLength = 12;
    internal const int MaxNewPasswordLength = 256;

    internal static bool IsValidNewPassword(string? password) =>
        !string.IsNullOrWhiteSpace(password) && password.Length is >= MinNewPasswordLength and <= MaxNewPasswordLength;

    internal static async Task<string?> RequestPasswordAsync(string title, string? description = null, bool isNew = false)
    {
        var prompt = new TextBoxWindowViewModel
        {
            InputType = TextBoxWindowViewModel.TextBoxInputType.Password,
            Description = description,
            MaxLength = isNew ? MaxNewPasswordLength : 0,
            ValueIsNullOrWhiteSpaceTip = Strings.LocalAuth_PasswordRequired,
        };
        try
        {
            return await IWindowManager.Instance.ShowTaskDialogAsync(prompt, title, isDialog: false, isCancelButton: true)
                ? prompt.Value : null;
        }
        finally
        {
            prompt.Value = null;
        }
    }

    internal enum VaultUnlockMethod { None, Password, WindowsHello, LegacyPassword }

    internal static VaultUnlockMethod GetUnlockMethod(AccountPlatformAuthenticator[] sources, bool hasPassword)
    {
        if (sources.Any(AccountPlatformAuthenticatorRepository.RequiresWindowsHello))
        {
            if (hasPassword)
                throw new AuthenticatorReadException(ImportResultCode.IncorrectFormat);
            return VaultUnlockMethod.WindowsHello;
        }
        if (!hasPassword) return VaultUnlockMethod.None;
        return OperatingSystem.IsWindows() && AuthenticatorSettings.RequireWindowsHello.Value
            ? VaultUnlockMethod.LegacyPassword : VaultUnlockMethod.Password;
    }

    internal static async Task<(bool authorized, string? password, bool verifiedWithHello)> RequestVaultUnlockAsync(
        VaultUnlockMethod method, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        switch (method)
        {
            case VaultUnlockMethod.WindowsHello:
                var verified = await VerifyWindowsHelloAsync(cancellationToken);
                return (verified, null, verified);
            case VaultUnlockMethod.LegacyPassword:
                if (!await IWindowManager.Instance.ShowTaskDialogAsync(
                    new MessageBoxWindowViewModel { Content = Strings.Auth_VaultHelloRecoveryTip },
                    Strings.Auth_VaultHelloRecovery, isCancelButton: true)) return default;
                cancellationToken.ThrowIfCancellationRequested();
                goto case VaultUnlockMethod.Password;
            case VaultUnlockMethod.Password:
                var password = await RequestPasswordAsync(Strings.Title_InputAuthPassword);
                cancellationToken.ThrowIfCancellationRequested();
                return string.IsNullOrWhiteSpace(password) ? default : (true, password, false);
            default:
                return (true, null, false);
        }
    }

    internal static void ClearLegacyHelloPreference() => AuthenticatorSettings.RequireWindowsHello.Value = false;

    internal static Task<bool> VerifyWindowsHelloAsync(CancellationToken cancellationToken = default) => MainThread2.InvokeOnMainThreadAsync<bool>(async () =>
    {
        cancellationToken.ThrowIfCancellationRequested();
        var owner = App.Instance.GetActiveWindow();
        if (owner is not { IsVisible: true } || owner.TryGetPlatformHandle() is not { Handle: not 0 } handle)
        {
            Toast.Show(ToastIcon.Warning, Strings.Auth_VaultHelloUnavailable);
            return false;
        }

        var result = await IPlatformService.Instance.VerifyLocalUserAsync(handle.Handle, Strings.Auth_VaultHelloPrompt, cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        if (result is LocalUserVerificationResult.Verified)
            return owner.IsVisible && owner.TryGetPlatformHandle()?.Handle == handle.Handle;

        if (result is not LocalUserVerificationResult.Canceled)
            Toast.Show(ToastIcon.Warning, result == LocalUserVerificationResult.Unavailable
                ? Strings.Auth_VaultHelloUnavailable : Strings.Auth_VaultHelloFailed);
        return false;
    });

    internal static string ReadErrorText(ImportResultCode code) => code switch
    {
        ImportResultCode.SecondaryPasswordFail => Strings.Auth_VaultPasswordFailed,
        ImportResultCode.LocalFail => Strings.Auth_VaultLocalReadFailed,
        ImportResultCode.WindowsHelloRequired => Strings.Auth_VaultHelloLocked,
        _ => Strings.Auth_VaultReadFailed,
    };

    internal static async Task<(List<IAuthenticatorDTO> items, string? password, bool verifiedWithHello)?> ReadAuthorizedVaultAsync(
        AccountPlatformAuthenticator[] sources, CancellationToken cancellationToken = default)
    {
        try
        {
            var method = GetUnlockMethod(sources, HasEncrypt(sources).haspassword);
            var access = await RequestVaultUnlockAsync(method, cancellationToken);
            if (!access.authorized) return null;

            var items = await GetAllAuthenticatorsAsync(sources, access.password, access.verifiedWithHello);
            cancellationToken.ThrowIfCancellationRequested();
            if (method == VaultUnlockMethod.LegacyPassword)
                ClearLegacyHelloPreference();
            return (items, access.password, access.verifiedWithHello);
        }
        catch (AuthenticatorReadException ex)
        {
            Toast.Show(ToastIcon.Warning, ReadErrorText(ex.ResultCode));
            return null;
        }
    }

    /// <summary>
    /// 导出全部令牌
    /// </summary>
    /// <param name="fileName">保存文件名</param>
    /// <param name="isLocal">导出是否携带本机加密</param>
    /// <param name="items">需要导出的令牌集合</param>
    /// <param name="password">导出是否携带二级密码</param>
    /// <returns>返回文件保存结果,为Null则失败</returns>
    public static async Task<SaveFileResult?> ExportAsync(string fileName, bool isLocal,
        IEnumerable<IAuthenticatorDTO> items, string? password = null)
    {
        SaveFileResult? exportFile = null;
        if (CommonEssentials.IsSupportedSaveFileDialog)
        {
            AvaloniaFilePickerFileTypeFilter fileTypes = new AvaloniaFilePickerFileTypeFilter.Item[] {
                new("MsgPack Files") {
                    Patterns = new[] { $"*{FileEx.MPO}", },
                    //MimeTypes
                    //AppleUniformTypeIdentifiers = 
                },
                new("Data Files") {
                    Patterns = new[] { $"*{FileEx.DAT}", },
                    //MimeTypes
                    //AppleUniformTypeIdentifiers = 
                },
            };
            exportFile = await FilePicker2.SaveAsync(new PickOptions
            {
                FileTypes = fileTypes,
                InitialFileName = fileName,
                PickerTitle = "Watt Toolkit",
            });
            if (exportFile == null) return exportFile;

            await using var filestream = exportFile.OpenWrite();
            if (filestream.CanSeek) filestream.SetLength(0);
            await repository.ExportAsync(filestream, isLocal, password, items);
            await filestream.FlushAsync();
        }

        return exportFile;
    }

    public static async Task SaveOrderAsync(IReadOnlyList<IAuthenticatorDTO> items, string? answer)
    {
        var newOrder = items.Select((item, index) => (item.Id, Index: index + 1)).ToArray();
        var cloud = items.Select((item, index) => (item, index))
            .Where(x => x.item.ServerId != null).ToArray();
        if (cloud.Length != 0 && string.IsNullOrEmpty(answer))
            throw new InvalidOperationException("Cloud authenticator order requires a security answer.");

        async Task SyncCloud(bool restore)
        {
            if (cloud.Length == 0) return;
            var response = await IMicroServiceClient.Instance.AuthenticatorClient.SyncAuthenticatorsToCloud(new()
            {
                Difference = cloud.Select(x => new UserAuthenticatorPushItem
                {
                    Id = x.item.ServerId,
                    Order = restore ? x.item.Index : x.index + 1,
                    Name = x.item.Name,
                }).ToArray(),
                Answer = answer,
            });
            if (!response.IsSuccess || response.Content == null || !response.Content.Result)
                throw new InvalidOperationException("Cloud authenticator order could not be synchronized.");
        }

        try { await SyncCloud(false); }
        catch (Exception cloudError)
        {
            throw new InvalidOperationException("Cloud order synchronization failed or its outcome is unknown. Synchronize before retrying.", cloudError);
        }
        try
        {
            await repository.UpdateOrderAsync(newOrder);
        }
        catch (Exception saveError)
        {
            try { await SyncCloud(true); }
            catch (Exception rollbackError)
            {
                throw new AggregateException("Local order was not saved and cloud order could not be restored. Synchronize before retrying.", saveError, rollbackError);
            }
            throw;
        }
    }

    /// <summary>
    /// 验证安全问题
    /// </summary>
    /// <returns>成功返回正确答案，失败返回null</returns>
    /// <exception cref="Exception">后端异常信息</exception>
    public static async Task<string?> VerifyIndependentPassword()
    {
        string? question = null;
        string? answer = null;
        var passwordQuestionResponse =
            await IMicroServiceClient.Instance.AuthenticatorClient.GetIndependentPasswordQuestion();
        if (passwordQuestionResponse.Content == null && passwordQuestionResponse.Code != ApiRspCode.Unauthorized)
        {
            var textViewModel = new TextBoxWindowViewModel();
            if (!await IWindowManager.Instance.ShowTaskDialogAsync(textViewModel, AppResources.Title_SetSecurityIssues,
                    subHeader: AppResources.SubHeader_FirstSyncSetAuth, isCancelButton: true)) return null;
            question = textViewModel.Value;
            textViewModel = new TextBoxWindowViewModel();
            if (!await IWindowManager.Instance.ShowTaskDialogAsync(textViewModel, AppResources.Title_SetSecurityIssues, subHeader: AppResources.SubHeader_PleaseEnterTheAnswerAgain,
                    isCancelButton: true)) return null;
            answer = textViewModel.Value;
            if (string.IsNullOrEmpty(question) || string.IsNullOrEmpty(answer)) return null;
            var setPassword =
                await IMicroServiceClient.Instance.AuthenticatorClient.SetIndependentPassword(new()
                {
                    PwdQuestion = question,
                    Answer = answer,
                });
            if (!setPassword.IsSuccess)
            {
                Toast.Show(ToastIcon.Error, AppResources.Error_SetSecurityIssuesFailed);
                return null;
            }
        }

        question ??= passwordQuestionResponse.Content;
        var answerTextViewModel = new TextBoxWindowViewModel();
        if (string.IsNullOrEmpty(answer) && await IWindowManager.Instance.ShowTaskDialogAsync(answerTextViewModel,
             AppResources.Title_PleaseEnterTheAnswer, subHeader: AppResources.SubHeader_SecurityIssues_.Format(question), isCancelButton: true))
        {
            answer = answerTextViewModel.Value;

            if (string.IsNullOrEmpty(answer))
            {
                Toast.Show(ToastIcon.Error, AppResources.Error_PleaseEnterAnswer);
                return await VerifyIndependentPassword();
            }

            var verifyResponse =
                await IMicroServiceClient.Instance.AuthenticatorClient
                    .VerifyIndependentPassword(new() { Answer = answer, });
            if (!verifyResponse.Content)
            {
                Toast.Show(ToastIcon.Error, AppResources.Error_AnswerIncorrect);
                return await VerifyIndependentPassword();
            }
        }

        return answer;
    }

    public static IAuthenticatorDTO ConvertToAuthenticatorDto(
        UserAuthenticatorResponse authenticatorResponse)
    {
        var exportDto = AuthenticatorPayloadSerializer.ReadCloudToken(authenticatorResponse.Token);
        exportDto.ThrowIsNull();
        var valueDto = ConvertToAuthenticatorValueDto(exportDto);
        AuthenticatorDTO dto = new AuthenticatorDTO()
        {
            ServerId = authenticatorResponse.Id,
            Value = valueDto,
            Name = exportDto.Name,
            Index = (int)authenticatorResponse.Order,
            //LastUpdate = DateTimeOffset.Now,
        };
        return dto;
    }

    static IAuthenticatorValueDTO? ConvertToAuthenticatorValueDto(AuthenticatorExportDTO authenticatorExportDto)
    {
        IAuthenticatorValueDTO? valueDto = null;

        switch (authenticatorExportDto.Platform)
        {
            case AuthenticatorPlatform.Steam:
                valueDto = new SteamAuthenticator()
                {
                    DeviceId = authenticatorExportDto.DeviceId,
                    SteamData = authenticatorExportDto.SteamData,
                };
                break;
            case AuthenticatorPlatform.Microsoft:
                valueDto = new MicrosoftAuthenticator();
                break;
            case AuthenticatorPlatform.BattleNet:
                valueDto = new BattleNetAuthenticator() { Serial = authenticatorExportDto.Serial };
                break;
            case AuthenticatorPlatform.Google:
                valueDto = new GoogleAuthenticator();
                break;
            case AuthenticatorPlatform.HOTP:
                valueDto = new HOTPAuthenticator() { Counter = authenticatorExportDto.Counter };
                break;
        }
        if (valueDto == null) return null;

        valueDto.Issuer = authenticatorExportDto.Issuer;
        valueDto.HMACType = authenticatorExportDto.HMACType;
        valueDto.SecretKey = authenticatorExportDto.SecretKey;
        valueDto.Period = authenticatorExportDto.Period;
        if (valueDto.Period == 0) valueDto.Period = 30;
        valueDto.CodeDigits = authenticatorExportDto.CodeDigits;

        return valueDto;
    }

    public static bool ReadXml(AuthenticatorDTO authenticatorDto, XmlReader reader, string? password)
    {
        var changed = false;

        var authenticatorType = reader.GetAttribute("type");
        switch (authenticatorType)
        {
            case "WinAuth.SteamAuthenticator":
                authenticatorDto.Value = new SteamAuthenticator();
                break;
            case "WinAuth.BattleNetAuthenticator":
                authenticatorDto.Value = new BattleNetAuthenticator();
                break;
            case "WinAuth.GoogleAuthenticator":
                authenticatorDto.Value = new GoogleAuthenticator();
                break;
            case "WinAuth.HOTPAuthenticator":
                authenticatorDto.Value = new HOTPAuthenticator();
                break;
            case "WinAuth.MicrosoftAuthenticator":
                authenticatorDto.Value = new MicrosoftAuthenticator();
                break;
            default:
                return false;
        }

        reader.MoveToContent();

        if (reader.IsEmptyElement)
        {
            reader.Read();
            return changed;
        }

        reader.Read();
        while (reader.EOF == false)
            if (reader.IsStartElement())
                switch (reader.Name)
                {
                    case "name":
                        authenticatorDto.Name = reader.ReadElementContentAsString();
                        break;

                    case "created":
                        var t = reader.ReadElementContentAsLong();
                        t += Convert.ToInt64(new TimeSpan(new DateTime(1970, 1, 1).Ticks).TotalMilliseconds);
                        t *= TimeSpan.TicksPerMillisecond;
                        authenticatorDto.Created = new DateTimeOffset(new DateTime(t).ToLocalTime());
                        break;

                    case "authenticatordata":
                        try
                        {
                            // we don't pass the password as they are locked till clicked
                            changed = authenticatorDto.Value.ReadXml(reader) || changed;
                        }
                        catch (WinAuthEncryptedSecretDataException)
                        {
                            // no action needed
                        }
                        catch (WinAuthBadPasswordException)
                        {
                            // no action needed
                        }

                        break;

                    // v2
                    case "authenticator":
                        var auth = AuthenticatorValueDTO.ReadXmlv2(reader, password);
                        if (auth != null)
                            authenticatorDto.Value = auth;
                        break;
                    // v2
                    case "servertimediff":
                        authenticatorDto.Value!.ServerTimeDiff = reader.ReadElementContentAsLong();
                        break;

                    default:
                        reader.Skip();
                        break;
                }
            else
            {
                reader.Read();
                break;
            }

        return changed;
    }

    public static async Task<string?> SelectFolderPath(string? fileExtension = null)
    {
        var options = new PickOptions();
        if (!string.IsNullOrEmpty(fileExtension))
        {
            AvaloniaFilePickerFileTypeFilter fileTypes = new AvaloniaFilePickerFileTypeFilter.Item[] {
                new($"{fileExtension} Files") {
                    Patterns = new[] { $"*{fileExtension}", },
                    //MimeTypes
                    //AppleUniformTypeIdentifiers = 
                },
            };
            options.FileTypes = fileTypes;
        }
        var filePath = (await FilePicker2.PickAsync(options))?.FullPath;

        if (!File.Exists(filePath))
            Toast.Show(ToastIcon.Warning, Strings.FilePathNotExist);
        return filePath;
    }

    public static async Task<IEnumerable<IFileResult>?> MultipleSelectFolderPath(string? fileExtension = null)
    {
        var options = new PickOptions();
        if (!string.IsNullOrEmpty(fileExtension))
        {
            AvaloniaFilePickerFileTypeFilter fileTypes = new AvaloniaFilePickerFileTypeFilter.Item[] {
                new($"{fileExtension} Files") {
                    Patterns = new[] { $"*{fileExtension}", },
                    //MimeTypes
                    //AppleUniformTypeIdentifiers = 
                },
            };
            options.FileTypes = fileTypes;
        }
        var filePaths = await FilePicker2.PickMultipleAsync(options);

        if (!filePaths.Any_Nullable())
            Toast.Show(ToastIcon.Warning, Strings.FilePathNotExist);
        return filePaths;
    }

    public static async Task<bool> VerifyMaxValue()
    {
        var auths = await AuthenticatorHelper.GetAllSourceAuthenticatorAsync();
        if (auths.Length < IAccountPlatformAuthenticatorRepository.MaxValue) return true;
        Toast.Show(ToastIcon.Info, Strings.Info_AuthMaximumQuantity);
        return false;
    }

    public static async Task<bool> SaveAuthenticator(IAuthenticatorDTO authenticatorDto, string? password = null,
        bool? isLocalProtect = null, bool verifiedWithHello = false)
    {
        try
        {
            if (isLocalProtect == null)
            {
                var access = await GetCurrentPassword();
                if (!access.authorized) return false;
                password = access.password;
                isLocalProtect = access.hasLocalPcEncrypt;
                verifiedWithHello = access.verifiedWithHello;
            }

            var result = await AddOrUpdateSaveAuthenticatorsAsync(authenticatorDto, password, isLocalProtect.Value, verifiedWithHello);
            if (result.isSuccess && !result.isUpdate)
                Toast.Show(ToastIcon.Success, Strings.LocalAuth_SteamAppAddSuccess);
            return result.isSuccess;
        }
        catch (AuthenticatorReadException ex)
        {
            Toast.Show(ToastIcon.Warning, ReadErrorText(ex.ResultCode));
            return false;
        }
    }

    public static async Task<(bool authorized, bool hasLocalPcEncrypt, string? password, bool verifiedWithHello)> GetCurrentPassword()
    {
        var sources = await GetAllSourceAuthenticatorAsync();
        var (hasLocal, hasPassword) = HasEncrypt(sources);
        if (!hasLocal && !hasPassword && !sources.Any(AccountPlatformAuthenticatorRepository.RequiresWindowsHello))
            return (true, false, null, false);
        var access = await ReadAuthorizedVaultAsync(sources);
        return access.HasValue ? (true, hasLocal, access.Value.password, access.Value.verifiedWithHello) : default;
    }
}
