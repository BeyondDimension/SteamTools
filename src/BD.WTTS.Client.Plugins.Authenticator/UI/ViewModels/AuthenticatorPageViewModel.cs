using Avalonia.Controls;
using Avalonia.Media.Imaging;
using BD.WTTS.Client.Resources;
using BD.WTTS.UI.Views.Pages;
using Splat;
using System.Drawing.Drawing2D;
using System.Linq;

namespace BD.WTTS.UI.ViewModels;

public sealed partial class AuthenticatorPageViewModel : ViewModelBase
{
    string? _currentPassword;

    DateTime _initializeTime;

    public AuthenticatorPageViewModel()
    {
        HasPasswordEncrypt = false;
        HasLocalPcEncrypt = false;
        Auths = new();
        BorderBottomIsActive = false;
        AuthenticatorItemModel.OnAuthenticatorItemIsSelectedChanged += AuthenticatorItemModel_OnAuthenticatorItemIsSelectedChanged;
        Initialize();
    }

    //该功能待完善，目前仅测试业务功能用
    private void AuthenticatorItemModel_OnAuthenticatorItemIsSelectedChanged(AuthenticatorItemModel sender)
    {
        if (sender.IsSelected)
        {
            AuthenticatorId = sender.AuthData.Id;
            BorderBottomIsActive = true;
        }
        else
        {
            BorderBottomIsActive = false;
            AuthenticatorId = -1;
        }
    }

    public async void Initialize()
    {
        Auths = new();
        //await AuthenticatorService.DeleteAllAuthenticatorsAsync();
        if (_initializeTime > DateTime.Now.AddSeconds(-5))
        {
            Toast.Show("请勿频繁操作");
            return;
        }
        _initializeTime = DateTime.Now;
        
        var sourcelist = await AuthenticatorService.GetAllSourceAuthenticatorAsync();
        if (sourcelist.Length < 1) return;

        (HasLocalPcEncrypt, HasPasswordEncrypt) = AuthenticatorService.HasEncrypt(sourcelist);

        if (HasPasswordEncrypt)
        {
            if (!await EnterPassword(sourcelist[0])) return;
        }

        // var list = await AuthenticatorService.GetAllAuthenticatorsAsync(sourcelist,
        //     _currentPassword.Base64Encode_Nullable());
        
        var list = await AuthenticatorService.GetAllAuthenticatorsAsync(sourcelist,
            _currentPassword);
        
        foreach (var item in list)
        {
            Auths.Add(new AuthenticatorItemModel(item));
        }

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
        if (await IWindowManager.Instance.ShowTaskDialogAsync(textViewmodel, "请输入令牌保护密码", isDialog: false) &&
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

        if (await AuthenticatorService.SwitchEncryptionAuthenticators(HasLocalPcEncrypt, null,
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
        if (await AuthenticatorService.SwitchEncryptionAuthenticators(HasLocalPcEncrypt, null,
                null))
        {
            Toast.Show("令牌密码保护移除成功。");
            _currentPassword = null;
        }
        else Toast.Show("令牌密码保护移除失败。");

        HasPasswordEncrypt = false;
    }

    public async Task ToggleLocalProtection()
    {
        bool newStatus = HasLocalPcEncrypt == false;

        if (await AuthenticatorService.SwitchEncryptionAuthenticators(newStatus, null,
                _currentPassword)) Toast.Show($"令牌本机电脑保护{(newStatus ? "开启" : "关闭")}成功");
        else Toast.Show($"令牌本机电脑保护{(newStatus ? "开启" : "关闭")}失败");

        HasLocalPcEncrypt = newStatus;
    }

    public async Task KeepDisplay()
    {
    }

    public async Task DeleteAuthAsync()
    {
        if (CurrentSelectedAuth == null) return;
        var messageviewmodel =
            new MessageBoxWindowViewModel { Content = Strings.LocalAuth_DeleteAuthTip };
        if (await IWindowManager.Instance.ShowTaskDialogAsync(messageviewmodel, "删除令牌", isDialog: false,
                isCancelButton: true))
        {
            AuthenticatorService.DeleteAuth(CurrentSelectedAuth.AuthData);
            Auths.Remove(CurrentSelectedAuth);
        }
    }

    public async Task EditAuthNameAsync()
    {
        if (CurrentSelectedAuth == null) return;
        string? newname = null;

        var textviewmodel = new TextBoxWindowViewModel { InputType = TextBoxWindowViewModel.TextBoxInputType.TextBox, Value = CurrentSelectedAuth.AuthName };
        if (await IWindowManager.Instance.ShowTaskDialogAsync(textviewmodel, "请输入新令牌名或取消", isDialog: false,
                isCancelButton: true))
        {
            newname = textviewmodel.Value;
        }

        if (string.IsNullOrEmpty(newname)) return;

        CurrentSelectedAuth.AuthName = newname;
        await AuthenticatorService.SaveEditAuthNameAsync(CurrentSelectedAuth.AuthData, newname);
    }
}
