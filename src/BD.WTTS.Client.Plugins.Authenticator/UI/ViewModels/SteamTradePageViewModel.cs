using AppResources = BD.WTTS.Client.Resources.Strings;

using BD.SteamClient.Models;
using BD.SteamClient.Services;
using WinAuth;

namespace BD.WTTS.UI.ViewModels;

public sealed partial class SteamTradePageViewModel
{
    WinAuth.SteamClient? _steamClient;
    readonly SteamAuthenticator? _steamAuthenticator;
    readonly ISteamAccountService _steamAccountService = Ioc.Get<ISteamAccountService>();

    public SteamTradePageViewModel()
    {
    }

    public SteamTradePageViewModel(ref IAuthenticatorDTO authenticatorDto)
    {
        if (authenticatorDto.Value is SteamAuthenticator steamAuthenticator)
        {
            _steamAuthenticator = steamAuthenticator;

            _steamClient = _steamAuthenticator.GetClient();

            Initialize();

            UserNameText = _steamAuthenticator.AccountName ?? "";

            authenticatorDto.Value = _steamAuthenticator;
        }
    }

    async void Initialize()
    {
        if (_steamClient!.IsLoggedIn() == false)
        {
            IsLoading = false;
            IsLogged = false;
        }
        else
        {
            IsLogged = true;
            await GetConfirmations(_steamClient);
        }
    }

    public async Task Refresh()
    {
        if (_steamClient == null) return;
        _ = await GetConfirmations(_steamClient);
    }

    public void SelectAll(bool isSelect)
    {
        foreach (var item in Confirmations)
        {
            item.IsSelected = isSelect;
        }
    }

    public async Task Login()
    {
        if (string.IsNullOrEmpty(UserNameText) || string.IsNullOrEmpty(PasswordText))
        {
            Toast.Show(ToastIcon.Warning, Strings.User_LoginError_Null);
            return;
        }
        if (IsLoading || IsLogged || _steamAuthenticator == null) return;

        _steamClient ??= _steamAuthenticator.GetClient();

        IsLoading = true;
        IsLogged = true;
        //var result = await _steamClient;

        if (_steamClient == null) return;

        //if (!result)
        //{
        //    IsLogged = result;
        //    if (_steamClient.Error == "Incorrect Login")
        //    {
        //        Toast.Show(ToastIcon.Warning, Strings.User_LoginError);
        //        return;
        //    }

        //    // if (_steamClient.RequiresCaptcha == true)
        //    // {
        //    //     _captchaId = _steamClient.CaptchaId;
        //    //     CaptchaImageUrlText = _steamClient.CaptchaUrl;
        //    //     Toast.Show(ToastIcon.None, Strings.User_LoginError_CodeImage);
        //    //     SelectIndex = 1;
        //    //     return;
        //    // }

        //    Toast.Show(ToastIcon.Error, AppResources.Error_UnknownLogin_.Format(_steamClient.Error));
        //    IsLoading = false;
        //    return;
        //}

        Toast.Show(ToastIcon.Success, string.Format(Strings.Success_, Strings.User_Login));

        //_steamAuthenticator.SessionData = RemenberLogin ? result.steamClient.Session.ToString() : null;
        _ = await GetConfirmations(_steamClient);

        IsLoading = false;
    }

    async Task<IEnumerable<SteamTradeConfirmationModel>?> GetConfirmations(WinAuth.SteamClient steamClient)
    {
        IsLoading = true;

        var result = await steamClient.GetConfirmations();

        if (result == null) return null;

        //var models = result.Select(item => new SteamTradeConfirmationModel(_steamAuthenticator!, item)).ToList();

        Confirmations.Clear();
        foreach (var item in result)
        {
            Confirmations.Add(new SteamTradeConfirmationModel(_steamAuthenticator!, item));
        }
        //Confirmations.AddRange(models);

        IsLoading = false;

        return Confirmations;
    }

    // public async Task Logout()
    // {
    //     if (_steamClient == null) return;
    //     if (await IWindowManager.Instance.ShowTaskDialogAsync(
    //             new MessageBoxWindowViewModel() { Content = Strings.LocalAuth_LogoutTip, }, isCancelButton: true,
    //             isDialog: false))
    //     {
    //         _steamClient.Logout();
    //     }
    // }

    // void RefreshConfirmationsList()
    // {
    //     var items = _confirmationsSourceList.Items.Where(s => s.IsOperate == 0);
    //     _confirmationsSourceList.Clear();
    //     var confirmations = items.ToList();
    //     if (confirmations.Any())
    //         _confirmationsSourceList.AddRange(confirmations);
    // }

    public async Task ConfirmTrade(object sender)
    {
        if (sender is not SteamTradeConfirmationModel trade) return;
        if (await IWindowManager.Instance.ShowTaskDialogAsync(
                new MessageBoxWindowViewModel()
                {
                    Content = AppResources.ModelContent_ConfirmTrade___.Format(trade.SendSummary,
                        trade.ReceiveSummary,
                        trade.ReceiveNoItems
                            ? $"您尚未让 {trade.Headline} 选择任何物品以交换您的物品。如果 {trade.Headline}  接受此交易，您将失去您提供的物品，但不会收到任何物品。"
                            : string.Empty, trade.Warn ?? string.Empty)
                }, AppResources.ConfirmTransaction,
                isCancelButton: true, isDialog: false))
        {
            await OperationTrade(true, trade);
        }
    }

    public async Task ConfirmAllTrade()
    {
        if (await IWindowManager.Instance.ShowTaskDialogAsync(
                new MessageBoxWindowViewModel() { Content = AppResources.ModelContent_ConfirmAllTrade },
                isCancelButton: true, isDialog: false))
        {
            await OperationTrade(true);
        }
    }

    public async Task CancelTrade(object sender)
    {
        if (sender is not SteamTradeConfirmationModel trade) return;
        if (await IWindowManager.Instance.ShowTaskDialogAsync(
                new MessageBoxWindowViewModel() { Content = AppResources.ModelContent_CancelTrade },
                isCancelButton: true, isDialog: false))
        {
            await OperationTrade(false, trade);
        }
    }

    public async Task CancelAllTrade()
    {
        if (await IWindowManager.Instance.ShowTaskDialogAsync(
                new MessageBoxWindowViewModel() { Content = AppResources.ModelContent_CancelAllTrade },
                isCancelButton: true, isDialog: false))
        {
            await OperationTrade(false);
        }
    }

    async Task<bool> OperationTrade(bool status, SteamTradeConfirmationModel trade)
    {
        var result = await ChangeTradeStatus(status, trade);
        if (result)
        {
            Toast.Show(ToastIcon.Success, $"{(status ? Strings.Agree : Strings.Cancel)}{trade.TypeName}");
            //trade.IsOperate = status ? 1 : 2;
            //RefreshConfirmationsList();
        }

        return result;
    }

    async Task OperationTrade(bool status)
    {
        if (!Confirmations.Any_Nullable())
        {
            Toast.Show(ToastIcon.Warning, Strings.LocalAuth_AuthTrade_Null);
            return;
        }

        // if (_operationTradeAllCancelToken != null)
        // {
        //     Toast.Show(ToastIcon.Warning, AppResources.Warning_PleaseWaitExecuteFinish);
        //     return;
        // }
        var selectedList = Confirmations.Where(c => c.IsSelected).ToList();

        if (!selectedList.Any_Nullable())
        {
            Toast.Show(ToastIcon.Warning, Strings.LocalAuth_AuthTrade_SelectNull);
            return;
        }

        var statusText = status ? Strings.BatchPass : Strings.BatchReject;

        if (await IWindowManager.Instance.ShowTaskDialogAsync(
                new MessageBoxWindowViewModel()
                {
                    Content = Strings.LocalAuth_AuthTrade_MessageBoxTip_.Format(statusText)
                }, isCancelButton: true, isDialog: false))
        {
            Toast.Show(Strings.LocalAuth_AuthTrade_ConfirmTip_.Format(statusText));

            if (!await ChangeTradeStatus(status, selectedList)) return;
            Toast.Show(ToastIcon.Success,
                AppResources.Success_ExecuteAllAuthEnd___.Format(statusText, selectedList.Count, 0));
        }
    }

    async Task<bool> ChangeTradeStatus(bool status, SteamTradeConfirmationModel trade)
    {
        if (!await ChangeTradeStatus(status, new[] { trade })) return false;
        Confirmations.Remove(trade);
        return true;

    }

    async Task<bool> ChangeTradeStatus(bool status, IEnumerable<SteamTradeConfirmationModel> trades)
    {
        var steamTradeConfirmationModels = trades.ToList();
        var ids = steamTradeConfirmationModels.ToDictionary(item => item.Id, item => item.Nonce);

        var task = Task.Run(() =>
        {
            var result = _steamClient!.ConfirmTrade(ids, status);
            Confirmations.Remove(steamTradeConfirmationModels);
            return result;
        });
        var result = await task;
        if (!result) Toast.Show(ToastIcon.Error, Strings.LocalAuth_AuthTrade_ConfirmError);
        if (task.Exception == null) return result;
        Log.Error(nameof(SteamTradePageViewModel), task.Exception, nameof(ChangeTradeStatus));
        Toast.Show(ToastIcon.Error, task.Exception.Message);
        return result;
    }
}