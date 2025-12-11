using WinAuth;
using AppResources = BD.WTTS.Client.Resources.Strings;

namespace BD.WTTS.Services;

public static class AuthenticatorHelper
{
    static IAccountPlatformAuthenticatorRepository repository = Ioc.Get<IAccountPlatformAuthenticatorRepository>();

    public static async Task<(bool isSuccess, bool isUpdate)> AddOrUpdateSaveAuthenticatorsAsync(IAuthenticatorDTO authenticatorDto, string? password, bool isLocal)
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
        return await repository.InsertOrUpdateAsync(authenticatorDto, isLocal, password);
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
        string? password = null)
    {
        return await repository.ConvertToListAsync(source, password);
    }

    public static async Task DeleteAllAuthenticatorsAsync()
    {
        var list = await repository.GetAllSourceAsync();
        foreach (var item in list)
            await repository.DeleteAsync(item.Id);
    }

    public static async void DeleteAuth(IAuthenticatorDTO authenticatorDto)
    {
        if (authenticatorDto.ServerId.HasValue)
            await repository.DeleteAsync(authenticatorDto.ServerId.Value);
        await repository.DeleteAsync(authenticatorDto.Id);
    }

    public static async Task SaveEditAuthNameAsync(IAuthenticatorDTO authenticatorDto, string newName)
    {
        var isLocal = await repository.HasLocalAsync();
        await repository.RenameAsync(authenticatorDto.Id, newName, isLocal);
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

    // public static async Task<bool> HasPassword()
    // {
    //     return await repository.HasSecondaryPasswordAsync();
    // }
    //
    // public static async Task<bool> HasLocal()
    // {
    //     return await repository.HasLocalAsync();
    // }

    public static async Task<bool> ValidatePassword(AccountPlatformAuthenticator sourceData, string password)
    {
        return (await repository.ConvertToListAsync(new[] { sourceData }, password)).Any_Nullable();
    }

    public static async Task<bool> SwitchEncryptionAuthenticators(bool hasLocal, IEnumerable<IAuthenticatorDTO>? auths, string? password = null)
    {
        try
        {
            await repository.SwitchEncryptionModeAsync(hasLocal, password, auths);
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(nameof(AuthenticatorHelper), ex, nameof(SwitchEncryptionAuthenticators));
            return false;
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

            var filestream = exportFile.OpenWrite();

            if (filestream.CanSeek && filestream.Position != 0) filestream.Position = 0;

            await repository.ExportAsync(filestream, isLocal, password, items);

            await filestream.FlushAsync();
            await filestream.DisposeAsync();
        }

        return exportFile;
    }

    /// <summary>
    /// 上移或下移令牌排序值
    /// </summary>
    /// <param name="convert">集合元素ConvertToAuthenticator的委托</param>
    /// <param name="items">令牌所在的集合</param>
    /// <param name="index">被操作令牌所在集合的Index</param>
    /// <param name="upOrDown">true为上移false为下移</param>
    /// <param name="answer">操作云令牌所需的安全问题答案</param>
    /// /// <typeparam name="T"></typeparam>
    public static async Task<int> MoveAuthenticatorIndex<T>(Func<T, IAuthenticatorDTO> convert, IReadOnlyList<T> items,
        int index, bool upOrDown, string? answer = null)
    {
        var newIndex = upOrDown ? index - 1 : index + 1;
        return await ChangeAuthenticatorIndex(convert, items, index, newIndex, answer);
    }

    /// <summary>
    /// 调整令牌排序值
    /// </summary>
    /// <param name="convert">集合元素ConvertToAuthenticator的委托</param>
    /// <param name="items">令牌所在的集合</param>
    /// <param name="oldIndex">被操作令牌所在集合的Index</param>
    /// <param name="newIndex">被操作令牌需要调整到所在集合的Index</param>
    /// <param name="answer">操作云令牌所需的安全问题答案</param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static async Task<int> ChangeAuthenticatorIndex<T>(Func<T, IAuthenticatorDTO> convert, IReadOnlyList<T> items,
        int oldIndex, int newIndex, string? answer = null)
    {
        var item = items[oldIndex];
        var item2Index = newIndex;
        if (item2Index <= -1 || item2Index >= items.Count || oldIndex == newIndex) return 0;
        var item2 = items[item2Index];
        var itemC = convert(item);
        var itemC2 = convert(item2);
        var orderIndex = itemC.Index;
        var orderIndex2 = itemC2.Index;
        itemC.Index = orderIndex2;
        itemC2.Index = orderIndex;
        var result = (await Task.WhenAll(UpdateAuthenticatorIndex(itemC, answer), UpdateAuthenticatorIndex(itemC2, answer)))
            .Sum();
        if (result < 2)
        {
            itemC.Index = orderIndex;
            itemC2.Index = orderIndex2;
            result = (await Task.WhenAll(UpdateAuthenticatorIndex(itemC, answer),
                    UpdateAuthenticatorIndex(itemC2, answer)))
                .Sum();
        }
        return result;
    }

    static async Task<int> UpdateAuthenticatorIndex(IAuthenticatorDTO authenticatorDto,
        string? answer = null)
    {
        if (authenticatorDto.ServerId == null) return await repository.UpdateIndexByItemAsync(authenticatorDto);
        if (string.IsNullOrEmpty(answer)) return 0;
        var response = await IMicroServiceClient.Instance.AuthenticatorClient.SyncAuthenticatorsToCloud(new()
        {
            Difference = new[]
            {
                new UserAuthenticatorPushItem()
                {
                    Id = authenticatorDto.ServerId,
                    Order = authenticatorDto.Index,
                    Name = authenticatorDto.Name,
                },
            },
            Answer = answer,
        });
        response.Content.ThrowIsNull();
        if (response.IsSuccess && response.Content.Result) return await repository.UpdateIndexByItemAsync(authenticatorDto);
        Toast.Show(ToastIcon.Warning, AppResources.Error_UpdateCloudData);
        return 0;
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
        var exportDto = MemoryPackSerializer.Deserialize<AuthenticatorExportDTO>(authenticatorResponse.Token);
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

    public static async Task<bool> SaveAuthenticator(IAuthenticatorDTO authenticatorDto, string? password = null, bool? isLocalProtect = null)
    {
        if (!string.IsNullOrEmpty(password) && isLocalProtect != null)
        {
            var result = await AuthenticatorHelper.AddOrUpdateSaveAuthenticatorsAsync(authenticatorDto, password, isLocalProtect.Value);
            if (result.isSuccess)
            {
                if (!result.isUpdate)
                    Toast.Show(ToastIcon.Success, Strings.LocalAuth_SteamAppAddSuccess);
            }
            return result.isSuccess;
        }
        else
        {
            var r = await GetCurrentPassword();
            if (r.hasPasswordEncrypt && r.password == null) return false;
            var result = await AuthenticatorHelper.AddOrUpdateSaveAuthenticatorsAsync(authenticatorDto, r.password, r.hasLocalPcEncrypt);
            if (result.isSuccess)
            {
                if (!result.isUpdate)
                    Toast.Show(ToastIcon.Success, Strings.LocalAuth_SteamAppAddSuccess);
            }
            return result.isSuccess;
        }
    }

    public static async Task<(bool hasLocalPcEncrypt, bool hasPasswordEncrypt, string? password)> GetCurrentPassword()
    {
        var sourceList = await AuthenticatorHelper.GetAllSourceAuthenticatorAsync();

        var (hasLocalPcEncrypt, hasPasswordEncrypt) = AuthenticatorHelper.HasEncrypt(sourceList);

        if (hasPasswordEncrypt)
        {
            var textViewmodel = new TextBoxWindowViewModel()
            {
                InputType = TextBoxWindowViewModel.TextBoxInputType.Password,
            };
            if (await IWindowManager.Instance.ShowTaskDialogAsync(textViewmodel, Strings.Title_InputAuthPassword, isDialog: false,
                    isCancelButton: true) &&
                textViewmodel.Value != null)
            {
                if (await AuthenticatorHelper.ValidatePassword(sourceList[0], textViewmodel.Value))
                {
                    return (hasLocalPcEncrypt, hasPasswordEncrypt, textViewmodel.Value);
                }
                else
                {
                    Toast.Show(ToastIcon.Warning, Strings.Warning_PasswordError);
                }
            }
        }

        return (hasLocalPcEncrypt, hasPasswordEncrypt, null);
    }
}
