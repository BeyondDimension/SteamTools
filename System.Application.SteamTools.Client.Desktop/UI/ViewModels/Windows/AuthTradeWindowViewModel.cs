using Newtonsoft.Json.Linq;
using ReactiveUI;
using System.Application.Models;
using System.Application.Services;
using System.Application.UI.Resx;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Properties;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using WinAuth;

namespace System.Application.UI.ViewModels
{
    public class AuthTradeWindowViewModel : WindowViewModel
    {
        private MyAuthenticator MyAuthenticator;
        private GAPAuthenticatorValueDTO.SteamAuthenticator _Authenticator => MyAuthenticator.AuthenticatorData.Value as GAPAuthenticatorValueDTO.SteamAuthenticator;
        public AuthTradeWindowViewModel()
        {
        }

        public AuthTradeWindowViewModel(MyAuthenticator auth)
        {
            MyAuthenticator = auth;
            Title = ThisAssembly.AssemblyTrademark + " | " + AppResources.LocalAuth_SteamAuthTrade;

            Refresh_Click();
        }

        #region LoginData
        private string _UserName;
        public string UserName
        {
            get => this._UserName;
            set
            {
                if (this._UserName != value)
                {
                    this._UserName = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        private string _Password;
        public string Password
        {
            get => this._Password;
            set
            {
                if (this._Password != value)
                {
                    this._Password = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        private bool _RememberMe;
        public bool RememberMe
        {
            get => this._RememberMe;
            set
            {
                if (this._RememberMe != value)
                {
                    this._RememberMe = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        private Stream? _CodeImage;
        public Stream? CodeImage
        {
            get => this._CodeImage;
            set
            {
                if (this._CodeImage != value)
                {
                    this._CodeImage = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        private string _CodeImageChar;
        public string CodeImageChar
        {
            get => this._CodeImageChar;
            set
            {
                if (this._CodeImageChar != value)
                {
                    this._CodeImageChar = value;
                    this.RaisePropertyChanged();
                }
            }
        }
        #endregion

        public bool IsLoggedIn
        {
            get => this._Authenticator.GetClient().IsLoggedIn();
            set
            {
                this.RaisePropertyChanged();
            }
        }
        public bool IsRequiresCaptcha
        {
            get => this._Authenticator.GetClient().RequiresCaptcha;
            set
            {
                this.RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Cancellation token for confirm all
        /// </summary>
        private CancellationTokenSource CancelComfirmAll;

        /// <summary>
        /// Cancellation token for cancel all
        /// </summary>
        private CancellationTokenSource CancelCancelAll;

        /// <summary>
        /// Trade info state
        /// </summary>
        private IList<WinAuthSteamClient.Confirmation> _Confirmations;
        public IList<WinAuthSteamClient.Confirmation> Confirmations
        {
            get => this._Confirmations;
            set
            {
                if (this._Confirmations != value)
                {
                    this._Confirmations = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        public bool IsConfirmationsEmpty
        {
            get => this.Confirmations.Any_Nullable();
        }

        public void LoginButton_Click()
        {
            if (CodeImageChar?.Trim().Length > 0)
            {
                Process(_Authenticator.GetClient().CaptchaId, CodeImageChar);
            }
            else
            {
                if (UserName?.Trim().Length > 0 || Password?.Trim().Length > 0)
                {
                    Process();
                }
                else
                {
                    ToastService.Current.Notify("请输入您的账号和密码");
                    return;
                }
            }
        }

        public void Refresh_Click()
        {
            if (IsLoggedIn)
            {
                Process();
            }
        }
        public void Logout_Click()
        {
            var steam = this._Authenticator.GetClient();
            steam.Logout();

            if (String.IsNullOrEmpty(_Authenticator.SessionData) == false)
            {
                IsLoggedIn = false;
                _Authenticator.SessionData = null;
                AuthService.AddOrUpdateSaveAuthenticators(MyAuthenticator);
            }
        }

        private void Process(string? captchaId = null, string? codeChar = null)
        {
            Task.Run(async () =>
            {
                var steam = _Authenticator.GetClient();
                if (!steam.IsLoggedIn())
                {
                    if (steam.Login(UserName, Password, captchaId, codeChar, R.GetCurrentCultureSteamLanguageName()) == false)
                    {
                        if (steam.Error == "Incorrect Login")
                        {
                            //this.Dialog("账号或密码错误");
                            return;
                        }

                        if (steam.Requires2FA == true)
                        {
                            //this.Dialog("无效令牌：您确定登录的是您当前令牌的账号？");
                            return;
                        }

                        if (steam.RequiresCaptcha == true)
                        {
                            IsRequiresCaptcha = steam.RequiresCaptcha;
                            //this.Dialog("请输入图片验证码");
                            CodeImage = await DI.Get<IHttpService>().GetImageAsync(steam.CaptchaUrl);
                            return;
                        }
                        //loginButton.Enabled = true;
                        //captchaGroup.Visible = false;

                        if (string.IsNullOrEmpty(steam.Error) == false)
                        {
                            //this.Dialog(steam.Error);
                            return;
                        }

                        return;
                    }
                    IsLoggedIn = true;
                    _Authenticator.SessionData = RememberMe ? steam.Session.ToString() : null;
                    AuthService.AddOrUpdateSaveAuthenticators(MyAuthenticator);
                }
                try
                {
                    Confirmations = new ObservableCollection<WinAuthSteamClient.Confirmation>(steam.GetConfirmations());

                    // 获取新交易后保存
                    if (!string.IsNullOrEmpty(_Authenticator.SessionData))
                    {
                        AuthService.AddOrUpdateSaveAuthenticators(MyAuthenticator);
                    }
                }
                catch (WinAuthUnauthorisedSteamRequestException)
                {
                    // Family view is probably on
                    ToastService.Current.Notify("You are not allowed to view confirmations. Have you enabled 'community-generated content' in Family View?");
                    return;
                }
                catch (WinAuthInvalidSteamRequestException)
                {
                    // likely a bad session so try a refresh first
                    try
                    {
                        steam.Refresh();
                        Confirmations = new ObservableCollection<WinAuthSteamClient.Confirmation>(steam.GetConfirmations());
                    }
                    catch (Exception)
                    {
                        // reset and show normal login
                        steam.Clear();
                        return;
                    }
                }
            }).ForgetAndDispose();
        }

        public void ConfirmTrade_Click(WinAuthSteamClient.Confirmation trade)
        {
            OperationTrade(true, trade);
        }
        public void CancelTrade_Click(WinAuthSteamClient.Confirmation trade)
        {
            OperationTrade(false, trade);
        }

        private void OperationTrade(bool accept, WinAuthSteamClient.Confirmation trade)
        {
            Task.Run(async () =>
            {
                bool result = false;
                if (accept)
                    result = await AcceptTrade(trade.Id);
                else
                    result = await RejectTrade(trade.Id);

                if (result)
                {
                    ToastService.Current.Notify($"已{(accept ? "同意" : "取消")}{trade.Details}");
                    MainThreadDesktop.BeginInvokeOnMainThread(() =>
                    {
                        Confirmations.Remove(trade);
                    });
                    //Refresh_Click();
                }
            }).ForgetAndDispose();
        }

        /// <summary>
        /// Accept the trade Confirmation
        /// </summary>
        /// <param name="tradeId">Id of Confirmation</param>
        private async Task<bool> AcceptTrade(string tradeId)
        {
            try
            {
                var trade = Confirmations.Where(t => t.Id == tradeId).FirstOrDefault();
                if (trade == null)
                {
                    throw new ApplicationException("无效的交易");
                }

                var result = await Task.Factory.StartNew<bool>((t) =>
                {
                    return this._Authenticator.GetClient().ConfirmTrade(((WinAuthSteamClient.Confirmation)t).Id, ((WinAuthSteamClient.Confirmation)t).Key, true);
                }, trade);
                if (result == false)
                {
                    throw new ApplicationException("无法确认此交易");
                }

                return true;
            }

            catch (WinAuthInvalidTradesResponseException ex)
            {
                Log.Error(nameof(RejectTrade), ex, nameof(AuthTradeWindowViewModel));
                ToastService.Current.Notify(ex.Message);
                return false;
            }
            catch (ApplicationException ex)
            {
                Log.Error(nameof(RejectTrade), ex, nameof(AuthTradeWindowViewModel));
                ToastService.Current.Notify(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Reject the trade Confirmation
        /// </summary>
        /// <param name="tradeId">ID of Confirmation</param>
        private async Task<bool> RejectTrade(string tradeId)
        {
            try
            {
                var trade = Confirmations.Where(t => t.Id == tradeId).FirstOrDefault();
                if (trade == null)
                {
                    throw new ApplicationException("无效的交易");
                }
                var result = await Task.Factory.StartNew<bool>((t) =>
                {
                    return this._Authenticator.GetClient().ConfirmTrade(((WinAuthSteamClient.Confirmation)t).Id, ((WinAuthSteamClient.Confirmation)t).Key, false);
                }, trade);
                if (result == false)
                {
                    throw new ApplicationException("无法取消的交易");
                }

                return true;
            }

            catch (WinAuthInvalidTradesResponseException ex)
            {
                Log.Error(nameof(RejectTrade), ex, nameof(AuthTradeWindowViewModel));
                ToastService.Current.Notify(ex.Message);
                return false;
            }
            catch (ApplicationException ex)
            {
                Log.Error(nameof(RejectTrade), ex, nameof(AuthTradeWindowViewModel));
                ToastService.Current.Notify(ex.Message);
                return false;
            }
        }

        public void ConfirmAllButton_Click()
        {
            OperationTrades(true);
        }
        public void CancelAllButton_Click()
        {
            OperationTrades(false);
        }

        private void OperationTrades(bool accept)
        {
            if (Confirmations.Count == 0)
            {
                ToastService.Current.Notify($"当前没有任何交易");
                return;
            }

            if (CancelComfirmAll != null)
            {
                return;
            }

            if (CancelCancelAll != null)
            {
                return;
            }

            var str = (accept ? "同意" : "取消");

            var result = MessageBoxCompat.ShowAsync("这将" + str + "您当前所有的交易确认。" + Environment.NewLine + Environment.NewLine +
                "确定要继续吗?", ThisAssembly.AssemblyTrademark, MessageBoxButtonCompat.OKCancel).ContinueWith(s =>
            {
                if (s.Result == MessageBoxResultCompat.OK)
                {
                    ToastService.Current.Set($"正在{str}所有交易中...");

                    if (accept)
                        AcceptAllTrade();
                    else
                        RejectAllTrade();
                }
            });
        }

        private async void AcceptAllTrade()
        {
            if (CancelComfirmAll != null)
            {
                CancelComfirmAll.Cancel();
                return;
            }

            CancelComfirmAll = new CancellationTokenSource();

            try
            {
                var rand = new Random();
                var tradeIds = Confirmations.Reverse().ToArray();
                for (var i = tradeIds.Length - 1; i >= 0; i--)
                {
                    if (CancelComfirmAll.IsCancellationRequested)
                    {
                        break;
                    }
                    DateTime start = DateTime.Now;

                    var result = await AcceptTrade(tradeIds[i].Id);
                    if (result == false || CancelComfirmAll.IsCancellationRequested == true)
                    {
                        break;
                    }
                    Confirmations.Remove(tradeIds[i]);
                    if (i != 0)
                    {
                        var duration = (int)DateTime.Now.Subtract(start).TotalMilliseconds;
                        var delay = WinAuthSteamClient.CONFIRMATION_EVENT_DELAY + rand.Next(WinAuthSteamClient.CONFIRMATION_EVENT_DELAY / 2); // delay is 100%-150% of CONFIRMATION_EVENT_DELAY
                        if (delay > duration)
                        {
                            await Task.Delay(delay - duration);
                        }
                    }
                }

            }
            finally
            {
                CancelComfirmAll = null;
                ToastService.Current.Notify($"已成功同意所有交易");

                AuthService.AddOrUpdateSaveAuthenticators(MyAuthenticator);
            }
        }

        private async void RejectAllTrade()
        {
            if (CancelCancelAll != null)
            {
                CancelCancelAll.Cancel();
                return;
            }

            CancelCancelAll = new CancellationTokenSource();

            try
            {
                var rand = new Random();
                var tradeIds = Confirmations.Reverse().ToArray();
                for (var i = tradeIds.Length - 1; i >= 0; i--)
                {
                    if (CancelCancelAll.IsCancellationRequested)
                    {
                        break;
                    }

                    DateTime start = DateTime.Now;

                    var result = await RejectTrade(tradeIds[i].Id);
                    if (result == false || CancelCancelAll.IsCancellationRequested == true)
                    {
                        break;
                    }
                    Confirmations.Remove(tradeIds[i]);
                    if (i != 0)
                    {
                        var duration = (int)DateTime.Now.Subtract(start).TotalMilliseconds;
                        var delay = WinAuthSteamClient.CONFIRMATION_EVENT_DELAY + rand.Next(WinAuthSteamClient.CONFIRMATION_EVENT_DELAY / 2); // delay is 100%-150% of CONFIRMATION_EVENT_DELAY
                        if (delay > duration)
                        {
                            await Task.Delay(delay - duration);
                        }
                    }
                }

            }
            finally
            {
                CancelCancelAll = null;
                ToastService.Current.Notify($"已成功取消所有交易");
                AuthService.AddOrUpdateSaveAuthenticators(MyAuthenticator);
            }
        }

    }
}