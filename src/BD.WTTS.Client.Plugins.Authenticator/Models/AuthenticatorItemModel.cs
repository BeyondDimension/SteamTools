using BD.Avalonia8.Image2;
using BD.WTTS.Helpers;
using BD.WTTS.UI.Views.Pages;
using System.Threading;

namespace BD.WTTS.Models;

public partial class AuthenticatorItemModel : ReactiveObject, IDisposable
{
    public IAuthenticatorDTO AuthData { get; set; }

    Timer? _progressTimer;
    readonly IDisposable selectionSubscription;
    bool isVisible;
    bool disposed;

    public void SetIsVisible(bool visible)
    {
        isVisible = visible;
        if (visible && IsSelected && !disposed)
            EnableShowCode();
        else
            DisableShowCode();
    }

    public void Tapped()
    {
        IsSelected = !IsSelected;
    }

    public async Task CopyCode()
    {
        if (AuthData.Value?.CurrentCode != null)
        {
            await Clipboard2.SetTextAsync(AuthData.Value?.CurrentCode);
            Toast.Show(ToastIcon.Success, Strings.LocalAuth_CopyAuthTip + AuthName);
        }
    }

    public AuthenticatorItemModel(IAuthenticatorDTO authenticatorDto)
    {
        AuthData = authenticatorDto;
        AuthName = AuthData.Name;

        selectionSubscription = this.WhenValueChanged(x => x.IsSelected)
            .Subscribe(x =>
        {
            if (x && isVisible && !disposed)
            {
                EnableShowCode();
            }
            else
            {
                DisableShowCode();
            }
        });
    }

    void ShowCode(object? sender)
    {
        try
        {
            var period = AuthData.Value.Period;
            var remaining = period - (int)((AuthData.Value.ServerTime % (period * 1000L)) / 1000L);
            var code = AuthData.Value.CurrentCode;
            MainThread2.BeginInvokeOnMainThread(() =>
            {
                if (!disposed && isVisible && IsSelected)
                {
                    Value = remaining;
                    Code = code;
                }
            });
        }
        catch (Exception ex)
        {
            MainThread2.BeginInvokeOnMainThread(DisableShowCode);
            Toast.Show(ToastIcon.Error, Strings.Error_AuthSynchronizationFailed_.Format(ex.Message));
            Log.Error(nameof(AuthenticatorItemModel), ex, nameof(ShowCode));
        }
    }

    void EnableShowCode()
    {
        if (AuthData.Value == null || disposed || !isVisible) return;
        IsShowCode = true;

        _progressTimer ??= new Timer(ShowCode, AuthData.Id, 0, 1000);
    }

    void DisableShowCode()
    {
        _progressTimer?.Dispose();
        _progressTimer = null;
        Code = DefaultCode;
        IsShowCode = false;
    }

    public async void EditAuthNameAsync()
    {
        string? newName = null;

        var textViewmodel = new TextBoxWindowViewModel
        {
            InputType = TextBoxWindowViewModel.TextBoxInputType.TextBox,
            Value = AuthName
        };
        if (await IWindowManager.Instance.ShowTaskDialogAsync(textViewmodel, Strings.Title_PleaseEnterNewAuthName, isDialog: false,
                isCancelButton: true))
        {
            newName = textViewmodel.Value;
        }

        if (string.IsNullOrEmpty(newName)) return;

        //if (AuthData.ServerId != null)
        //{
        //    if (string.IsNullOrEmpty(_currentAnswer))
        //    {
        //        _currentAnswer = await AuthenticatorService.VerifyIndependentPassword();
        //        if (string.IsNullOrEmpty(_currentAnswer)) return;
        //    }

        //    var response = await IMicroServiceClient.Instance.AuthenticatorClient.SyncAuthenticatorsToCloud(new()
        //    {
        //        Difference = new[]
        //        {
        //            new UserAuthenticatorPushItem()
        //            {
        //                Id = AuthData.ServerId,
        //                Name = newName,
        //                Order = SelectedAuth.AuthData.Index,
        //            },
        //        },
        //        Answer = _currentAnswer,
        //    });
        //    response.Content.ThrowIsNull();
        //    if (response.IsSuccess && response.Content.Result)
        //        Toast.Show(ToastIcon.Success, Strings.Success_UpdateCloudData);
        //    else
        //        Toast.Show(ToastIcon.Warning, Strings.Error_UpdateCloudData);
        //}

        AuthName = newName;
        await AuthenticatorHelper.SaveEditAuthNameAsync(AuthData, newName);
        Toast.Show(ToastIcon.Success, Strings.Success_LocalAuthUpdateSuccessful);
    }

    public async void ShowSteamTradeWindow()
    {
        if (AuthData.Platform != AuthenticatorPlatform.Steam)
        {
            return;
        }
        var authData = AuthData;

        using var viewModel = new SteamTradePageViewModel(ref authData);
        await IWindowManager.Instance.ShowTaskDialogAsync(viewModel,
            Strings.ConfirmTransaction, pageContent: new SteamTradePage(), isOkButton: false);
        AuthData = authData;
    }

    public async Task ShowSteamQrLoginWindow()
    {
        if (!IsSteamAuthenticator)
            return;
        if (AuthData.Value is not WinAuth.SteamAuthenticator authenticator || !SteamQrLoginSession.CanUse(authenticator))
        {
            Toast.Show(ToastIcon.Warning, SteamQrLoginSession.LoginRequiredMessage);
            return;
        }

        using var viewModel = new SteamQrLoginPageViewModel(this);
        await IWindowManager.Instance.ShowTaskDialogAsync(viewModel, "扫描二维码登录 Steam",
            pageContent: new SteamQrLoginPage(), isOkButton: false);
    }

    public async Task ShowDetailData()
    {
        if (AuthData.Platform == AuthenticatorPlatform.Steam)
            await IWindowManager.Instance.ShowTaskDialogAsync(new SteamDetailPageViewModel(AuthData),
                Strings.LocalAuth_ShowAuthInfo,
                pageContent: new SteamDetailPage(), isOkButton: false);
        else
        {
            var temp = AuthData.Value?.SecretKey
                .ThrowIsNull().ToHexString();
            await IWindowManager.Instance.ShowTaskDialogAsync(
                new TextBoxWindowViewModel()
                {
                    InputType = TextBoxWindowViewModel.TextBoxInputType.ReadOnlyText,
                    Value = temp,
                }, Strings.ModelContent_SecretKey_.Format(AuthName), isOkButton: true);
        }
    }

    public void ShowQrCode()
    {
        var dto = AuthData.ToExport();
        var bytes = Serializable.SMP(dto);

        var bytes_compress_br = bytes.CompressByteArrayByBrotli();

        var (result, stream, e) = QRCodeHelper.Create(bytes_compress_br);

        switch (result)
        {
            case QRCodeHelper.QRCodeCreateResult.Success:
                IWindowManager.Instance.ShowTaskDialogAsync(new MessageBoxWindowViewModel()
                {
                    Content = new Image2 { Source = stream!, Width = 400 }
                }, Strings.DisplayQRCode, isOkButton: false);
                break;
            case QRCodeHelper.QRCodeCreateResult.DataTooLong:
                Toast.Show(ToastIcon.Error, Strings.AuthLocal_ExportToQRCodeTooLongErrorTip);
                break;
            case QRCodeHelper.QRCodeCreateResult.Exception:
                Toast.Show(ToastIcon.Error, e!.Message);
                Log.Error(nameof(AuthenticatorHomePageViewModel), e, nameof(ShowQrCode));
                break;
        }
    }

    public void Dispose()
    {
        disposed = true;
        selectionSubscription.Dispose();
        DisableShowCode();
    }
}