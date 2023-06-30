using BD.WTTS.Client.Resources;
using BD.WTTS.UI.Views.Pages;

namespace BD.WTTS.UI.ViewModels;

public sealed partial class AuthenticatorPageViewModel : ViewModelBase
{
    string? _currentPassword;

    DateTime _initializeTime;

    AuthenticatorItemModel? PrevSelectedAuth { get; set; }

    AuthenticatorItemModel? CurrentSelectedAuth { get; set; }

    public AuthenticatorPageViewModel()
    {
        Auths = new();
        BorderBottomIsActive = false;
        AuthenticatorItemModel.OnAuthenticatorItemIsSelectedChanged += AuthenticatorItemModel_OnAuthenticatorItemIsSelectedChanged;
        Initialize();
    }

    private void AuthenticatorItemModel_OnAuthenticatorItemIsSelectedChanged(AuthenticatorItemModel sender)
    {
        if (sender.IsSelected)
        {
            CurrentSelectedAuth = sender;

            if (PrevSelectedAuth != null) PrevSelectedAuth.IsSelected = false;

            PrevSelectedAuth = sender;

            BorderBottomIsActive = true;
        }
        else
        {
            if (PrevSelectedAuth == CurrentSelectedAuth) PrevSelectedAuth = null;
            if (PrevSelectedAuth != null) return;
            BorderBottomIsActive = false;
            CurrentSelectedAuth = null;
        }
    }

    public async void Initialize()
    {
        if (_initializeTime > DateTime.Now.AddSeconds(-5))
        {
            Toast.Show("请勿频繁操作");
            return;
        }
        _initializeTime = DateTime.Now;
        
        Auths.Clear();
        BorderBottomIsActive = false;
        
        var sourceList = await AuthenticatorService.GetAllSourceAuthenticatorAsync();
        if (sourceList.Length < 1) return;

        AuthenticatorIsEmpty = false;
        
        (HasLocalPcEncrypt, HasPasswordEncrypt) = AuthenticatorService.HasEncrypt(sourceList);

        if (HasPasswordEncrypt)
        {
            if (!await EnterPassword(sourceList[0])) return;
        }
        else
        {
            IsVerificationPass = true;
        }

        // var list = await AuthenticatorService.GetAllAuthenticatorsAsync(sourcelist,
        //     _currentPassword.Base64Encode_Nullable());
        
        var list = await AuthenticatorService.GetAllAuthenticatorsAsync(sourceList,
            _currentPassword);
        
        foreach (var item in list)
        {
            Auths.Add(new AuthenticatorItemModel(item));
        }

        Toast.Show("令牌加载成功");

        //var test5 = await IMicroServiceClient.Instance.AuthenticatorClient.GetAuthenticatorDeleteBackups();

        //string rspquestion = "";
        //var rsp1 = await IMicroServiceClient.Instance.AuthenticatorClient.GetIndependentPasswordQuestion();
        //if (rsp1.Content != null) rspquestion = rsp1.Content;
        //var setpassword = await IMicroServiceClient.Instance.AuthenticatorClient.SetIndependentPassword(new() { PwdQuestion = "测试", Answer = "123" });

        //var test1 = await IMicroServiceClient.Instance.AuthenticatorClient.SyncAuthenticatorsToCloud(new()
        //{
        //    Difference = new UserAuthenticatorPushItem[]
        //    {
        //        new()
        //        {
        //            GamePlatform = (int)GamePlatform.Windows,
        //            TokenType = UserAuthenticatorTokenType.TOTP,
        //            Name = list[0].Name,
        //            Token = MemoryPackSerializer.Serialize(AuthenticatorDTOExtensions.ToExport(list[0])),  //await AuthenticatorService.ExportAuthAsync(new IAuthenticatorDTO(){  }),
        //            Order = 1,
        //            Remarks = ""
        //        },
        //    },
        //    Answer = "123"
        //});

        //var test = await IMicroServiceClient.Instance.Advertisement.All(AdvertisementType.Banner);
        //var test2 = await IMicroServiceClient.Instance.Script.Query();
        //var test3 = await IMicroServiceClient.Instance.AuthenticatorClient.GetAuthenticators();
    }

    public async Task<bool> EnterPassword(AccountPlatformAuthenticator sourceData)
    {
        var textViewmodel = new TextBoxWindowViewModel()
        {
            InputType = TextBoxWindowViewModel.TextBoxInputType.Password,
        };
        if (await IWindowManager.Instance.ShowTaskDialogAsync(textViewmodel, "请输入令牌保护密码", isDialog: false,
                isCancelButton: true) &&
            textViewmodel.Value != null)
        {
            if (await AuthenticatorService.ValidatePassword(sourceData, textViewmodel.Value))
            {
                //_currentPassword = textViewmodel.Value.Base64DecodeToByteArray_Nullable();
                _currentPassword = textViewmodel.Value;
                IsVerificationPass = true;
                return true;
            }
            else Toast.Show("密码错误，请刷新重试");
        }

        IsVerificationPass = false;
        return false;
    }

    public async Task SetPasswordProtection()
    {
        if (Auths.Count < 1 || IsVerificationPass == false)
        {
            Toast.Show("拒绝操作");
            return;
        }
        string? newPassword = null;
        var textViewmodel = new TextBoxWindowViewModel()
        {
            InputType = TextBoxWindowViewModel.TextBoxInputType.Password,
        };
        if (await IWindowManager.Instance.ShowTaskDialogAsync(textViewmodel, "请输入令牌保护密码",  isDialog: false, isCancelButton: true) &&
            textViewmodel.Value != null)
        {
            newPassword = textViewmodel.Value;
            textViewmodel.Value = null;
            if (!(await IWindowManager.Instance.ShowTaskDialogAsync(textViewmodel, "请再次输入密码以确认",  isDialog: false) &&
                  textViewmodel.Value == newPassword)) return;
        }
        else return;

        if (await AuthenticatorService.SwitchEncryptionAuthenticators(HasLocalPcEncrypt, Auths.Select(i => i.AuthData),
                newPassword))
        {
            Toast.Show("令牌密码保护设置成功。");
            //_currentPassword = newPassword.Base64DecodeToByteArray_Nullable();
            _currentPassword = newPassword;
        }
        else Toast.Show("令牌密码保护设置失败。");

        HasPasswordEncrypt = true;
    }

    public async Task RemovePasswordProtection()
    {
        if (Auths.Count < 1 || IsVerificationPass == false)
        {
            Toast.Show("拒绝操作");
            return;
        }
        if (await AuthenticatorService.SwitchEncryptionAuthenticators(HasLocalPcEncrypt, Auths.Select(i => i.AuthData),
                null))
        {
            Toast.Show("令牌密码保护移除成功。");
            _currentPassword = null;
        }
        else Toast.Show("令牌密码保护移除失败。");

        HasPasswordEncrypt = false;
        IsVerificationPass = true;
    }

    public async Task ToggleLocalProtection()
    {
        if (Auths.Count < 1 || IsVerificationPass == false)
        {
            Toast.Show("拒绝操作");
            return;
        }
        bool newStatus = HasLocalPcEncrypt == false;

        if (await AuthenticatorService.SwitchEncryptionAuthenticators(newStatus, Auths.Select(i => i.AuthData),
                _currentPassword)) Toast.Show($"令牌本机电脑保护{(newStatus ? "开启" : "关闭")}成功");
        else Toast.Show($"令牌本机电脑保护{(newStatus ? "开启" : "关闭")}失败");

        HasLocalPcEncrypt = newStatus;
    }

    public async Task EncryptHelp()
    {
        var messageViewmodel = new MessageBoxWindowViewModel();
        messageViewmodel.Content += Strings.LocalAuth_ProtectionAuth_Info + "\r\n\r\n";
        messageViewmodel.Content += Strings.LocalAuth_ProtectionAuth_EnablePassword + ":\r\n\r\n";
        messageViewmodel.Content += Strings.LocalAuth_ProtectionAuth_EnablePasswordTip + "\r\n\r\n";
        messageViewmodel.Content += Strings.LocalAuth_ProtectionAuth_IsOnlyCurrentComputerEncrypt + ":\r\n\r\n";
        messageViewmodel.Content += Strings.LocalAuth_ProtectionAuth_IsOnlyCurrentComputerEncryptTip + "\r\n\r\n";
        await IWindowManager.Instance.ShowTaskDialogAsync(messageViewmodel, "令牌加密帮助");
    }

    public void ReLockAuthenticator()
    {
        if (!HasPasswordEncrypt)
        {
            Toast.Show("没有加密令牌可供操作");
            return;
        }
        Auths.Clear();
        BorderBottomIsActive = false;
        IsVerificationPass = false;
    }

    public async Task KeepDisplay()
    {
        
    }

    public async Task DeleteAuthAsync()
    {
        if (CurrentSelectedAuth == null) return;
        var messageViewmodel =
            new MessageBoxWindowViewModel { Content = Strings.LocalAuth_DeleteAuthTip };
        if (await IWindowManager.Instance.ShowTaskDialogAsync(messageViewmodel, "删除令牌", isDialog: false,
                isCancelButton: true))
        {
            AuthenticatorService.DeleteAuth(CurrentSelectedAuth.AuthData);
            Auths.Remove(CurrentSelectedAuth);
        }
    }

    public async Task EditAuthNameAsync()
    {
        if (CurrentSelectedAuth == null) return;
        string? newName = null;

        var textViewmodel = new TextBoxWindowViewModel { InputType = TextBoxWindowViewModel.TextBoxInputType.TextBox, Value = CurrentSelectedAuth.AuthName };
        if (await IWindowManager.Instance.ShowTaskDialogAsync(textViewmodel, "请输入新令牌名或取消", isDialog: false,
                isCancelButton: true))
        {
            newName = textViewmodel.Value;
        }

        if (string.IsNullOrEmpty(newName)) return;

        CurrentSelectedAuth.AuthName = newName;
        await AuthenticatorService.SaveEditAuthNameAsync(CurrentSelectedAuth.AuthData, newName);
    }

    public async Task OpenSteamLoginImportWindow()
    {
        await IWindowManager.Instance.ShowTaskDialogAsync(new SteamLoginImportViewModel(_currentPassword), "Steam登入导入",
            pageContent: new SteamLoginImportPage(), isCancelButton: true);
    }
    
    public async Task OpenSteamOtherImportWindow()
    {
        await IWindowManager.Instance.ShowTaskDialogAsync(new SteamOtherImportViewModel(_currentPassword), "令牌导入",
            pageContent: new SteamOtherImportPage(), isCancelButton: true);
    }
    
    public async Task OpenExportWindow()
    {
        await IWindowManager.Instance.ShowTaskDialogAsync(new AuthenticatorExportViewModel(), "导出令牌",
            pageContent: new AuthenticatorExportPage(), isCancelButton: true);
    }

    public void ShowQrCode()
    {
        if (CurrentSelectedAuth == null) return;
        var dto = CurrentSelectedAuth.AuthData.ToExport();
        var bytes = Serializable.SMP(dto);
        
        var bytes_compress_br = bytes.CompressByteArrayByBrotli();
        
        var (result, stream, e) = QRCodeHelper.Create(bytes_compress_br);
        switch (result)
        {
            case QRCodeHelper.QRCodeCreateResult.DataTooLong:
                Toast.Show(Strings.AuthLocal_ExportToQRCodeTooLongErrorTip);
                break;
            case QRCodeHelper.QRCodeCreateResult.Exception:
                Toast.Show(e.Message);
                Log.Error(nameof(AuthenticatorPageViewModel), e, nameof(ShowQrCode));
                break;
        }

        QrCodeStream = stream;
    }

    public async Task ShowReplyWindow()
    {
        if (CurrentSelectedAuth == null || CurrentSelectedAuth.AuthData.Platform != AuthenticatorPlatform.Steam) return;
        await IWindowManager.Instance.ShowTaskDialogAsync(new SteamTradePageViewModel(CurrentSelectedAuth.AuthData),
            "确认交易",
            pageContent: new SteamTradePage(), isCancelButton: true);
    }

    //未完善
    public async Task ShowAuthJsonData()
    {
        if (CurrentSelectedAuth == null) return;
        await IWindowManager.Instance.ShowTaskDialogAsync(new ShowSteamDataViewModel(CurrentSelectedAuth.AuthData),
            "查看令牌详细数据",
            pageContent: new ShowSteamDataPage(), isCancelButton: true);
    }

    protected override void Dispose(bool disposing)
    {
        AuthenticatorItemModel.OnAuthenticatorItemIsSelectedChanged -= AuthenticatorItemModel_OnAuthenticatorItemIsSelectedChanged;
        base.Dispose(disposing);
    }
}
