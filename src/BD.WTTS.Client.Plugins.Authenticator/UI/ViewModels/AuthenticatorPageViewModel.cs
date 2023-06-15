using Avalonia.Controls;
using Avalonia.Media.Imaging;
using BD.WTTS.Client.Resources;
using Splat;
using System.Drawing.Drawing2D;
using System.Linq;

namespace BD.WTTS.UI.ViewModels;

public sealed partial class AuthenticatorPageViewModel : TabItemViewModel
{
    public override string Name => "AuthenticatorPage";

    [Reactive]
    public ObservableCollection<AuthenticatorItemModel> Auths { get; set; }

    private AuthenticatorItemModel? CurrentSelectedAuth
    {
        get
        {
            if (Auths.Count > 0 && AuthenticatorId > 0)
            {
                return Auths.Where(i => i.AuthData.Id == AuthenticatorId).First();
            }
            return null;
        }
    }

    private int AuthenticatorId { get; set; } = -1;

    public AuthenticatorPageViewModel()
    {
        Auths = new();
        BorderBottomIsActive = false;
        AuthenticatorItemModel.OnAuthenticatorItemIsSelectedChanged += AuthenticatorItemModel_OnAuthenticatorItemIsSelectedChanged;
        Start();
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

    public async void Start()
    {
        var list = await AuthenticatorService.GetAllAuthenticatorsAsync();
        foreach (var item in list)
        {
            Auths.Add(new AuthenticatorItemModel(item));
        }
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
