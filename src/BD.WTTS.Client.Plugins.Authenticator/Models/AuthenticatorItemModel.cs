using Avalonia.Gif;
using BD.WTTS.UI.Views.Pages;
using System.Threading;

namespace BD.WTTS.Models;

public partial class AuthenticatorItemModel : ReactiveObject, IDisposable
{
    public IAuthenticatorDTO AuthData { get; set; }

    Timer? _progressTimer;

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

        this.WhenValueChanged(x => x.IsSelected)
            .Subscribe(x =>
        {
            if (x)
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
            Value = AuthData.Value.Period -
                        (int)((AuthData.Value.ServerTime - (AuthData.Value.CodeInterval * 30000L)) / 1000L);
            Code = AuthData.Value.CurrentCode;
        }
        catch (Exception ex)
        {
            Code = null;
            Toast.Show(ToastIcon.Error, Strings.Error_AuthSynchronizationFailed_.Format(ex.Message));
            Log.Error(nameof(AuthenticatorItemModel), ex, nameof(ShowCode));
        }

        if (string.IsNullOrEmpty(Code))
        {
            DisableShowCode();
        }
    }

    void EnableShowCode()
    {
        if (AuthData.Value == null) return;
        IsShowCode = true;

        if (string.IsNullOrEmpty(Code)) return;

        _progressTimer ??= new Timer(ShowCode, AuthData.Id, 0, 1000);
    }

    void DisableShowCode()
    {
        if (_progressTimer != null)
        {
            _progressTimer.Dispose();
            _progressTimer = null;
            Code = DefaultCode;
            IsShowCode = false;
        }
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

        await IWindowManager.Instance.ShowTaskDialogAsync(new SteamTradePageViewModel(ref authData),
            Strings.ConfirmTransaction, pageContent: new SteamTradePage(), isOkButton: false);
        AuthData = authData;
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
        DisableShowCode();
    }
}