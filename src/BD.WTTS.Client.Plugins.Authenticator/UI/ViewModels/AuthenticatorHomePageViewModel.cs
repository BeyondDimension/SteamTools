using AppResources = BD.WTTS.Client.Resources.Strings;
using SJsonSerializer = System.Text.Json.JsonSerializer;

using BD.SteamClient.Models;
using BD.SteamClient.Services;
using BD.WTTS.UI.Views.Pages;
using WinAuth;
using AngleSharp.Text;
using Avalonia.Controls;
using SteamKit2.Authentication;
using BD.WTTS.UI.Views.Controls;

namespace BD.WTTS.UI.ViewModels;

public sealed partial class AuthenticatorHomePageViewModel
{
    const int MAX_SYNC_VALUE = 100;

    string? _currentAnswer;

    string? _currentPassword;

    //DateTime _initializeTime;
    readonly Dictionary<string, string[]> dictPinYinArray = new();

    Func<AuthenticatorItemModel, bool> PredicateName(string? serachText)
    {
        return s =>
        {
            if (s == null)
                return false;
            if (string.IsNullOrEmpty(serachText))
                return true;
            if (s.AuthName.Contains(serachText, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            var pinyinArray = Pinyin.GetPinyin(s.AuthName, dictPinYinArray);
            if (Pinyin.SearchCompare(serachText, s.AuthName, pinyinArray))
            {
                return true;
            }

            return false;
        };
    }

    public AuthenticatorHomePageViewModel()
    {
        AuthSource = new(t => t.AuthData.Id);

        //this.WhenAnyValue(v => v.Auths)
        //    .Subscribe(items => items?
        //        .ToObservableChangeSet()
        //        .AutoRefresh(x => x.IsSelected)
        //        .WhenPropertyChanged(x => x.IsSelected, false)
        //        .Subscribe(s =>
        //        {
        //            if (s.Value)
        //                SelectedAuth = s.Sender;
        //            else
        //                SelectedAuth = null;
        //        }));

        var textFilter = this.WhenAnyValue(x => x.SearchText).Select(PredicateName);

        this.AuthSource
            .Connect()
            .Filter(textFilter)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Sort(SortExpressionComparer<AuthenticatorItemModel>.Ascending(x => x.AuthData.Index).ThenBy(x => x.AuthName))
            .Bind(out _Auths)
            .Subscribe();
    }

    public async void Initialize()
    {
        if (IsLoading) return;

        IsLoading = true;
        //await AuthenticatorService.DeleteAllAuthenticatorsAsync();
        //if (_initializeTime > DateTime.Now.AddSeconds(-1))
        //{
        //    Toast.Show(ToastIcon.Warning, AppResources.Warning_DoNotOperateFrequently);
        //    return;
        //}

        //_initializeTime = DateTime.Now;

        AuthSource.Clear();

        var sourceList = await AuthenticatorHelper.GetAllSourceAuthenticatorAsync();
        if (sourceList.Any_Nullable())
        {
            (HasLocalPcEncrypt, HasPasswordEncrypt) = AuthenticatorHelper.HasEncrypt(sourceList);

            if (HasPasswordEncrypt && IsVerificationPass == false)
            {
                if (!await EnterPassword(sourceList[0]))
                {
                    IsLoading = false;
                    return;
                }
            }
            else
            {
                IsVerificationPass = true;
            }

            // var list = await AuthenticatorService.GetAllAuthenticatorsAsync(sourcelist,
            //     _currentPassword.Base64Encode_Nullable());

            var list = (await AuthenticatorHelper.GetAllAuthenticatorsAsync(sourceList,
                _currentPassword)).OrderBy(i => i.Index);

            foreach (var item in list)
            {
                AuthSource.AddOrUpdate(new AuthenticatorItemModel(item));
            }

            var trayMenus = AuthSource.Items.Select(s => new TrayMenuItem
            {
                Name = s.AuthName,
                Command = ReactiveCommand.Create(async () =>
                {
                    if (IsVerificationPass)
                    {
                        await s.CopyCode();
                        INotificationService.Instance.Notify(Strings.LocalAuth_CopyAuthTip + s.AuthName, NotificationType.Message);
                    }
                    else
                    {
                        INotificationService.Instance.Notify(Strings.Auth_PasswordProtectedVerifyFirst, NotificationType.Message);
                    }
                })
            }).ToList();

            IApplication.Instance.UpdateMenuItems(Plugin.Instance.UniqueEnglishName, new TrayMenuItem
            {
                Name = Plugin.Instance.Name,
                Items = trayMenus,
            });
            //Toast.Show(ToastIcon.Success, AppResources.Success_AuthloadedSuccessfully);
        }
        else
        {
            IsVerificationPass = true;
        }

        IsLoading = false;
    }

    public async Task<bool> EnterPassword(AccountPlatformAuthenticator sourceData)
    {
        var textViewmodel = new TextBoxWindowViewModel()
        {
            InputType = TextBoxWindowViewModel.TextBoxInputType.Password,
        };
        if (await IWindowManager.Instance.ShowTaskDialogAsync(textViewmodel, AppResources.Title_InputAuthPassword, isDialog: false,
                isCancelButton: true) &&
            textViewmodel.Value != null)
        {
            if (await AuthenticatorHelper.ValidatePassword(sourceData, textViewmodel.Value))
            {
                //_currentPassword = textViewmodel.Value.Base64DecodeToByteArray_Nullable();
                _currentPassword = textViewmodel.Value;
                IsVerificationPass = true;
                return true;
            }
            else
            {
                Toast.Show(ToastIcon.Warning, AppResources.Warning_PasswordError);
                return await EnterPassword(sourceData);
            }
        }

        IsVerificationPass = false;
        return false;
    }

    public async Task SetPasswordProtection()
    {
        if (!Auths.Any() || IsVerificationPass == false)
        {
            Toast.Show(ToastIcon.Warning, AppResources.Warning_RefuseOperate);
            return;
        }

        string? newPassword;
        var textViewmodel = new TextBoxWindowViewModel()
        {
            InputType = TextBoxWindowViewModel.TextBoxInputType.Password,
        };
        if (await IWindowManager.Instance.ShowTaskDialogAsync(textViewmodel, AppResources.Title_InputAuthPassword, isDialog: false,
                isCancelButton: true) &&
            textViewmodel.Value != null)
        {
            newPassword = textViewmodel.Value;
            textViewmodel.Value = null;
            if (!(await IWindowManager.Instance.ShowTaskDialogAsync(textViewmodel, AppResources.Title_PasswordConfirm, isDialog: false, isCancelButton: true) &&
                  textViewmodel.Value == newPassword))
                return;
        }
        else return;

        if (await AuthenticatorHelper.SwitchEncryptionAuthenticators(HasLocalPcEncrypt, Auths.Select(i => i.AuthData),
                newPassword))
        {
            Toast.Show(ToastIcon.Success, AppResources.Success_AuthPasswordSetSuccessfully);
            //_currentPassword = newPassword.Base64DecodeToByteArray_Nullable();
            _currentPassword = newPassword;
        }
        else Toast.Show(ToastIcon.Error, AppResources.Error_TokenPasswordSetFailed);

        HasPasswordEncrypt = true;
    }

    public async Task RemovePasswordProtection()
    {
        if (!Auths.Any() || IsVerificationPass == false)
        {
            Toast.Show(ToastIcon.Warning, AppResources.Warning_RefuseOperate);
            return;
        }

        var sourceList = await AuthenticatorHelper.GetAllSourceAuthenticatorAsync();
        if (await EnterPassword(sourceList[0]))
        {
            if (await AuthenticatorHelper.SwitchEncryptionAuthenticators(HasLocalPcEncrypt, Auths.Select(i => i.AuthData)))
            {
                Toast.Show(ToastIcon.Success, AppResources.Success_AuthPasswordRemovedSuccessfully);
                _currentPassword = null;
            }
            else Toast.Show(ToastIcon.Error, AppResources.Error_TokenPasswordRemovedFailed);

            HasPasswordEncrypt = false;
            IsVerificationPass = true;
        }
    }

    public async Task ToggleLocalProtection()
    {
        if (!Auths.Any() || IsVerificationPass == false)
        {
            Toast.Show(ToastIcon.Warning, AppResources.Warning_RefuseOperate);
            return;
        }

        bool newStatus = HasLocalPcEncrypt == false;

        if (await AuthenticatorHelper.SwitchEncryptionAuthenticators(newStatus, Auths.Select(i => i.AuthData), _currentPassword))
            Toast.Show(ToastIcon.Success, AppResources.Success_AuthProtectSuccessfully_.Format(newStatus ? AppResources.Enable : AppResources.Disable));
        else Toast.Show(ToastIcon.Error, AppResources.Error_AuthProtectFailed_.Format(newStatus ? AppResources.Enable : AppResources.Disable));

        HasLocalPcEncrypt = newStatus;
    }

    public async Task EncryptHelp()
    {
        var messageViewmodel = new MessageBoxWindowViewModel
        {
            Content = $"""
                {Strings.LocalAuth_ProtectionAuth_Info}

                {Strings.LocalAuth_ProtectionAuth_EnablePassword}：

                {Strings.LocalAuth_ProtectionAuth_EnablePasswordTip}

                {Strings.LocalAuth_ProtectionAuth_IsOnlyCurrentComputerEncrypt}：

                {Strings.LocalAuth_ProtectionAuth_IsOnlyCurrentComputerEncryptTip}
            """
        };
        await IWindowManager.Instance.ShowTaskDialogAsync(messageViewmodel, AppResources.Title_AuthEncryption);
    }

    public void ReLockAuthenticator()
    {
        if (!HasPasswordEncrypt)
        {
            Toast.Show(ToastIcon.Warning, AppResources.Warning_NotAuthProvided);
            return;
        }

        AuthSource.Clear();
        IsVerificationPass = false;
    }

    public async Task SyncAuthenticators()
    {
        var response = await IMicroServiceClient.Instance.AuthenticatorClient.GetAuthenticators();
        if (response.Code != ApiRspCode.OK)
        {
            switch (response.Code)
            {
                case ApiRspCode.Unauthorized:
                    Toast.Show(ToastIcon.Error, AppResources.Error_PleaseLoginWattToolKit);
                    break;
            }

            return;
        }

        #region 安全问题

        if (string.IsNullOrEmpty(_currentAnswer))
        {
            var answer = await AuthenticatorHelper.VerifyIndependentPassword();
            if (!string.IsNullOrEmpty(answer)) _currentAnswer = answer;
            else return;
        }

        #endregion

        response.Content.ThrowIsNull();
        var cloudAuths = response.Content.Select(AuthenticatorHelper.ConvertToAuthenticatorDto).ToList();

        if (cloudAuths.Count >= Auths.Count)
        {
            int changes = 0;
            int additions = 0;
            foreach (var cloudAuth in cloudAuths)
            {
                //var cloudAuth = AuthenticatorService.ConvertToAuthenticatorDto(item);
                // var localAuth = (Auths.FirstOrDefault(i => i.AuthData.ServerId == cloudAuth.ServerId) ??
                //                  Auths.FirstOrDefault(i => i.AuthData.Index == cloudAuth.Index))?.AuthData;
                var localAuth = Auths.FirstOrDefault(i => i.AuthData.ServerId == cloudAuth.ServerId)?.AuthData;
                if (localAuth != null)
                {
                    bool isChanged = false;
                    if (localAuth.Name != cloudAuth.Name)
                    {
                        localAuth.Name = cloudAuth.Name;
                        isChanged = true;
                    }

                    if (localAuth.Index != cloudAuth.Index)
                    {
                        localAuth.Index = cloudAuth.Index;
                        isChanged = true;
                    }

                    if (isChanged)
                    {
                        localAuth.LastUpdate = DateTimeOffset.Now;
                        await AuthenticatorHelper.AddOrUpdateSaveAuthenticatorsAsync(localAuth, _currentPassword,
                            HasLocalPcEncrypt);
                        changes++;
                    }

                    continue;
                }

                additions++;
                await AuthenticatorHelper.AddOrUpdateSaveAuthenticatorsAsync(cloudAuth, _currentPassword,
                    HasLocalPcEncrypt);
            }

            string changeMessage = AppResources.Auth_Sync_UpdateTips.Format(additions, changes);
            if (changes == 0 && additions == 0)
            {
                changeMessage = AppResources.Auth_Sync_LatestData;
            }
            Toast.Show(ToastIcon.Success, AppResources.Success_CloudSynchronizationSuccessful.Format(changeMessage));
            Initialize();
            return;
        }

        var pushItems = (
            from item in Auths
            let cloudAuth = cloudAuths.FirstOrDefault(i => i.ServerId == item.AuthData.ServerId)
            where cloudAuth == null
            select new UserAuthenticatorPushItem()
            {
                Id = item.AuthData.ServerId,
                TokenType = item.AuthData.Platform == AuthenticatorPlatform.HOTP
                    ? UserAuthenticatorTokenType.HOTP
                    : UserAuthenticatorTokenType.TOTP,
                Name = item.AuthData.Name,
                Order = item.AuthData.Index,
                Token = MemoryPackSerializer.Serialize(item.AuthData.ToExport()),
            }).ToArray();

        if ((cloudAuths.Count + pushItems.Length) > MAX_SYNC_VALUE)
        {
            Toast.Show(ToastIcon.Error, AppResources.Error_CloudAuthMaximumQuantity___.Format(MAX_SYNC_VALUE, cloudAuths.Count, pushItems.Length));
            return;
        }

        var syncResponse = await IMicroServiceClient.Instance.AuthenticatorClient.SyncAuthenticatorsToCloud(new()
        {
            Difference = pushItems,
            Answer = _currentAnswer,
        });

        if (!syncResponse.IsSuccess)
        {
            Toast.Show(ToastIcon.Error, AppResources.Error_FailedToSynchronizeAuth);
            return;
        }
        syncResponse.Content.ThrowIsNull();
        if (!syncResponse.Content.Result)
        {
            Toast.Show(ToastIcon.Error, syncResponse.Content.Message);
            return;
        }
        response = await IMicroServiceClient.Instance.AuthenticatorClient.GetAuthenticators();
        if (response.Content?.Length != Auths.Count)
        {
            Toast.Show(ToastIcon.Error, AppResources.Error_DataNotUnified);
            return;
        }
        foreach (var item in response.Content)
        {
            var localAuth = Auths.FirstOrDefault(i => i.AuthData.Index == item.Order)?.AuthData;
            // var localAuth = Auths
            //     .FirstOrDefault(i => item.Token!.SequenceEqual(MemoryPackSerializer.Serialize(i.AuthData.ToExport())))
            //     ?.AuthData;
            localAuth.ThrowIsNull(AppResources.Error_localAuthNotEmpty);
            localAuth.ServerId ??= item.Id;
            localAuth.LastUpdate = DateTimeOffset.Now;
            await AuthenticatorHelper.AddOrUpdateSaveAuthenticatorsAsync(localAuth, _currentPassword,
                HasLocalPcEncrypt);
        }

        Initialize();
        Toast.Show(ToastIcon.Success, AppResources.Success_AuthUpload__.Format(syncResponse.Content?.Message, pushItems.Length));
    }

    public async Task ResetIndependentPassword()
    {
        var passwordQuestionResponse =
            await IMicroServiceClient.Instance.AuthenticatorClient.GetIndependentPasswordQuestion();
        if (!passwordQuestionResponse.IsSuccess || passwordQuestionResponse.Content == null)
        {
            Toast.Show(ToastIcon.Warning, AppResources.Auth_Sync_NoHasPassword);
            return;
        }
        _currentAnswer = await AuthenticatorHelper.VerifyIndependentPassword();
        if (string.IsNullOrEmpty(_currentAnswer)) return;
        var textViewModel = new TextBoxWindowViewModel();
        if (!await IWindowManager.Instance.ShowTaskDialogAsync(textViewModel, AppResources.Title_SetSecurityIssues,
                subHeader: AppResources.SubHeader_SyncReSetAuth, isCancelButton: true)) return;
        var question = textViewModel.Value;
        textViewModel = new TextBoxWindowViewModel();
        if (!await IWindowManager.Instance.ShowTaskDialogAsync(textViewModel, AppResources.Title_SetSecurityIssues, subHeader: AppResources.SubHeader_PleaseEnterTheAnswerAgain,
                isCancelButton: true)) return;
        var answer = textViewModel.Value;
        if (string.IsNullOrEmpty(question) || string.IsNullOrEmpty(answer)) return;
        var resetPassword = await IMicroServiceClient.Instance.AuthenticatorClient.ResetIndependentPassword(new()
        {
            Answer = _currentAnswer,
            NewPwdQuestion = question,
            NewAnswer = answer,
        });
        if (!resetPassword.IsSuccess)
        {
            Toast.Show(ToastIcon.Error, AppResources.Error_SetSecurityIssuesFailed);
            return;
        }
        _currentAnswer = answer;
    }

    public async Task DeleteAuthAsync(object sender)
    {
        if (sender is not AuthenticatorItemModel authenticatorItemModel) return;
        var messageViewmodel =
            new MessageBoxWindowViewModel { Content = Strings.LocalAuth_DeleteAuthTip2 };
        if (await IWindowManager.Instance.ShowTaskDialogAsync(messageViewmodel, AppResources.Title_DeleteAuth, isDialog: false,
                isCancelButton: true))
        {
            if (authenticatorItemModel.AuthData.ServerId != null)
            {
                if (string.IsNullOrEmpty(_currentAnswer))
                {
                    _currentAnswer = await AuthenticatorHelper.VerifyIndependentPassword();
                    if (string.IsNullOrEmpty(_currentAnswer)) return;
                }
                var response = await IMicroServiceClient.Instance.AuthenticatorClient.SyncAuthenticatorsToCloud(new()
                {
                    Difference = new[]
                    {
                        new UserAuthenticatorPushItem()
                        {
                            Id = authenticatorItemModel.AuthData.ServerId, IsDeleted = true,
                        },
                    },
                    Answer = _currentAnswer,
                });
                response.Content.ThrowIsNull();
                if (response.IsSuccess && response.Content.Result)
                    Toast.Show(ToastIcon.Success, AppResources.Success_DelCloudData);
                else
                    Toast.Show(ToastIcon.Warning, AppResources.Error_DelCloudData);
            }
            AuthenticatorHelper.DeleteAuth(authenticatorItemModel.AuthData);
            AuthSource.Remove(authenticatorItemModel);
            Toast.Show(ToastIcon.Success, AppResources.Success_LocalAuthDelSuccessful);
        }
    }

    public async Task OpenExportWindow()
    {
        if (!Auths.Any_Nullable())
        {
            Toast.Show(ToastIcon.Warning, AppResources.Warning_RefuseOperate);
            return;
        }

        await IWindowManager.Instance.ShowTaskDialogAsync(new AuthenticatorExportViewModel(), AppResources.ExportAuth,
            pageContent: new AuthenticatorExportPage(), isOkButton: false);
    }

    public async void ShowAddWindow()
    {
        await IWindowManager.Instance.ShowTaskDialogAsync(new AuthenticatorImportPageViewModel(), AppResources.LocalAuth_AddAuth,
        pageContent: new AuthenticatorImportPage(), isOkButton: false);

        Initialize();
    }

    public async Task ShowRecoverWindow()
    {
        if (string.IsNullOrEmpty(_currentAnswer))
            _currentAnswer = await AuthenticatorHelper.VerifyIndependentPassword();
        if (string.IsNullOrEmpty(_currentAnswer)) return;
        await IWindowManager.Instance.ShowTaskDialogAsync(new AuthenticatorRecoverPageViewModel(_currentAnswer), AppResources.Auth_Sync_Recover,
            pageContent: new AuthenticatorRecoverPage(), isOkButton: false);
    }

    public async void ExportAuthWithSdaFile(object sender)
    {
        if (sender is not AuthenticatorItemModel authenticatorItemModel) return;

        if (authenticatorItemModel.AuthData.Value is SteamAuthenticator steamAuthenticator)
        {
            if (string.IsNullOrEmpty(steamAuthenticator.SteamData)) return;

            var steamdata = SJsonSerializer.Deserialize(steamAuthenticator.SteamData,
                ImportFileModelJsonContext.Default.SdaFileModel);

            steamdata.ThrowIsNull();
            if (steamAuthenticator.SecretKey == null) return;
            var sdafilemodel = new SdaFileModel
            {
                DeviceId = steamAuthenticator.DeviceId ?? string.Empty,
                FullyEnrolled = steamdata.FullyEnrolled,
                Session = steamdata.Session,
                SerialNumber = steamAuthenticator.Serial ?? string.Empty,
                RevocationCode = steamdata.RevocationCode,
                Uri = steamdata.Uri,
                ServerTime = steamdata.ServerTime,
                AccountName = steamdata.AccountName,
                TokenGid = steamdata.TokenGid,
                IdentitySecret = steamdata.IdentitySecret,
                Secret1 = steamdata.Secret1,
                Status = steamdata.Status,
                SharedSecret = Convert.ToBase64String(steamAuthenticator.SecretKey),
            };

            var jsonString = SJsonSerializer.Serialize(sdafilemodel, ImportFileModelJsonContext.Default.SdaFileModel);

            //...导出至文件目录

            if (CommonEssentials.IsSupportedSaveFileDialog)
            {
                AvaloniaFilePickerFileTypeFilter fileTypes = new AvaloniaFilePickerFileTypeFilter.Item[] {
                    new("maFile Files") {
                        Patterns = new[] { $"*{FileEx.maFile}", },
                        //MimeTypes
                        //AppleUniformTypeIdentifiers = 
                    },
                };
                var exportFile = await FilePicker2.SaveAsync(new PickOptions
                {
                    FileTypes = fileTypes,
                    InitialFileName = $"{steamAuthenticator.AccountName}{FileEx.maFile}",
                    PickerTitle = "Watt Toolkit",
                });
                if (exportFile == null) return;

                var filestream = exportFile.OpenWrite();

                if (filestream.CanSeek && filestream.Position != 0) filestream.Position = 0;

                var data = Encoding.UTF8.GetBytes(jsonString);

                await filestream.WriteAsync(data);

                await filestream.FlushAsync();
                await filestream.DisposeAsync();

                Toast.Show(ToastIcon.Success, Strings.ExportedToPath_.Format(exportFile.ToString()));
            }
        }
        else
        {
            Toast.Show(ToastIcon.Warning, AppResources.Auth_OnlyMafileFormat);
        }
    }

    public async Task AuthenticatorIndexMoveUp(object sender)
    {
        if (sender is not AuthenticatorItemModel authenticatorItemModel) return;
        var index = Auths.IndexOf(authenticatorItemModel);
        if (index == 0)
            return;
        if (authenticatorItemModel.AuthData.ServerId != null)
        {
            if (string.IsNullOrEmpty(_currentAnswer))
                _currentAnswer = await AuthenticatorHelper.VerifyIndependentPassword();
        }
        var result = await AuthenticatorHelper.MoveAuthenticatorIndex<AuthenticatorItemModel>((a) => a.AuthData, Auths, index, true,
            _currentAnswer);
        if (result <= 1)
        {
            Toast.Show(ToastIcon.Error, AppResources.Auth_Sync_MoveError);
            return;
        }
        //Auths.Move(index, index - 1);
    }

    public async Task AuthenticatorIndexMoveDown(object sender)
    {
        if (sender is not AuthenticatorItemModel authenticatorItemModel) return;
        var index = Auths.IndexOf(authenticatorItemModel);
        if (index >= Auths.Count - 1)
            return;
        if (authenticatorItemModel.AuthData.ServerId != null)
        {
            if (string.IsNullOrEmpty(_currentAnswer))
                _currentAnswer = await AuthenticatorHelper.VerifyIndependentPassword();
        }
        var result = await AuthenticatorHelper.MoveAuthenticatorIndex<AuthenticatorItemModel>((a) => a.AuthData, Auths, index, false,
            _currentAnswer);
        if (result <= 1)
        {
            Toast.Show(ToastIcon.Error, AppResources.Auth_Sync_MoveError);
            return;
        }
        //Auths.Move(index, index + 1);
    }

    public async Task AuthenticatorIndexSticky(object sender)
    {
        if (sender is not AuthenticatorItemModel authenticatorItemModel) return;
        var index = Auths.IndexOf(authenticatorItemModel);
        if (authenticatorItemModel.AuthData.ServerId != null)
        {
            if (string.IsNullOrEmpty(_currentAnswer))
                _currentAnswer = await AuthenticatorHelper.VerifyIndependentPassword();
        }
        var result = await AuthenticatorHelper.ChangeAuthenticatorIndex<AuthenticatorItemModel>((a) => a.AuthData,
            Auths, index, 0,
            _currentAnswer);
        if (result <= 1)
        {
            Toast.Show(ToastIcon.Error, AppResources.Auth_Sync_MoveError);
            return;
        }
        var tmp = Auths.OrderBy(a => a.AuthData.Index).ToList();
        AuthSource.Clear();
        AuthSource.AddOrUpdate(tmp);
    }

    public async Task DefaultExport(object sender)
    {
        if (sender is not AuthenticatorItemModel authenticatorItemModel) return;
        var exportFile = await AuthenticatorHelper.ExportAsync($"{authenticatorItemModel.AuthName}{FileEx.MPO}", false,
            new[] { authenticatorItemModel.AuthData });
        if (exportFile == null) return;
        Toast.Show(ToastIcon.Success, Strings.ExportedToPath_.Format(exportFile.ToString()));
    }

    public async Task UnbindingSteamAuthAsync(object sender)
    {
        if (sender is not AuthenticatorItemModel authenticatorItemModel) return;

        if (authenticatorItemModel.AuthData.Platform != AuthenticatorPlatform.Steam)
        {
            Toast.Show(ToastIcon.Warning, AppResources.Warning_OnlySupportSteamAuth);
            return;
        }

        if (await IWindowManager.Instance.ShowTaskDialogAsync(
                new MessageBoxWindowViewModel() { Content = AppResources.ModelContent_ConfirmUnbinding, Title = AppResources.LocalAuth_RemoveAuth },
                isDialog: false, isCancelButton: true))
        {
            if (authenticatorItemModel.AuthData.Value is SteamAuthenticator steamAuthenticator)
            {
                string? password;
                var textViewmodel = new TextBoxWindowViewModel()
                {
                    InputType = TextBoxWindowViewModel.TextBoxInputType.Password,
                };
                if (await IWindowManager.Instance.ShowTaskDialogAsync(textViewmodel, AppResources.Title_PleaseEnterLoginPassword,
                        isDialog: false, isCancelButton: true))
                {
                    password = textViewmodel.Value;
                }
                else return;

                if (string.IsNullOrEmpty(password)) return;

                var steamAccountService = Ioc.Get<ISteamAccountService>();
                SteamLoginState loginState = new() { Username = steamAuthenticator.AccountName, Password = password, };
                await steamAccountService.DoLoginV2Async(loginState);
                if (!loginState.Success)
                {
                    loginState.TwofactorCode = steamAuthenticator.CurrentCode;
                    await steamAccountService.DoLoginV2Async(loginState);
                }

                if (string.IsNullOrEmpty(loginState.AccessToken))
                {
                    Toast.Show(ToastIcon.Warning, AppResources.Warning_UnbindFailed);
                    return;
                }

                if (await steamAuthenticator.RemoveAuthenticatorAsync(loginState.AccessToken))
                {
                    Toast.Show(ToastIcon.Success, AppResources.Success_AuthUnbindSuccessful);
                    return;
                }

                Toast.Show(ToastIcon.Error, AppResources.Error_AuthUnbindFailed);
            }
        }
    }

    public override void Activation()
    {
        base.Activation();
        Initialize();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
    }
}