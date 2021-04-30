using Microsoft.Win32;
using SteamTools.Models;
using SteamTools.Models.Settings;
using SteamTools.Properties;
using SteamTools.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using SteamTool.Core.Common;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using WinAuth;
using System.Net;
using SteamTools.Win32;
using SteamTool.Model;
using System.Drawing;
using System.Threading;

namespace SteamTools.ViewModels
{
    public class AuthTradeWindowViewModel : MainWindowViewModelBase
    {
        private readonly WinAuthAuthenticator _Authenticator;
        private SteamAuthenticator _AuthenticatorData => _Authenticator.AuthenticatorData as SteamAuthenticator;

        public AuthTradeWindowViewModel(WinAuthAuthenticator auth)
        {
            _Authenticator = auth;
            this.Title = ProductInfo.Title + " | " + Resources.Auth_TradeTitle;

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

        private string _CodeImage;
        public string CodeImage
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
            get => this._AuthenticatorData.GetClient().IsLoggedIn();
            set
            {
                this.RaisePropertyChanged();
            }
        }
        public bool IsRequiresCaptcha
        {
            get => this._AuthenticatorData.GetClient().RequiresCaptcha;
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
        private BindingList<SteamClient.Confirmation> _Confirmations;
        public BindingList<SteamClient.Confirmation> Confirmations
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

        public void LoginButton_Click()
        {
            if (UserName?.Trim().Length > 0 || Password?.Trim().Length > 0)
            {
                Process();
            }
            else
            {
                this.Dialog("请输入您的账号和密码");
                return;
            }

        }

        public void CaptchaButton_Click()
        {
            if (CodeImageChar?.Trim().Length > 0)
            {
                Process(_AuthenticatorData.GetClient().CaptchaId, CodeImageChar);
                return;
            }
            else
            {
                this.Dialog("请输入正确的令牌");
                return;
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
            var steam = this._AuthenticatorData.GetClient();
            steam.Logout();

            if (String.IsNullOrEmpty(_AuthenticatorData.SessionData) == false)
            {
                IsLoggedIn = false;
                _AuthenticatorData.SessionData = null;
                AuthService.Current.SaveCurrentAuth();
            }
        }

        private void Process(string captchaId = null, string codeChar = null)
        {
            Task.Run(() =>
            {
                var steam = _AuthenticatorData.GetClient();
                if (!steam.IsLoggedIn())
                {
                    if (steam.Login(UserName, Password, captchaId, codeChar, ResourceService.Current.GetCurrentCultureSteamLanguageName()) == false)
                    {
                        if (steam.Error == "Incorrect Login")
                        {
                            this.Dialog("账号或密码错误");
                            return;
                        }

                        if (steam.Requires2FA == true)
                        {
                            this.Dialog("无效令牌：您确定登录的是您当前令牌的账号？");
                            return;
                        }

                        if (steam.RequiresCaptcha == true)
                        {
                            IsRequiresCaptcha = steam.RequiresCaptcha;
                            this.Dialog("请输入图片验证码");
                            CodeImage = steam.CaptchaUrl;
                            return;
                        }
                        //loginButton.Enabled = true;
                        //captchaGroup.Visible = false;

                        if (string.IsNullOrEmpty(steam.Error) == false)
                        {
                            this.Dialog(steam.Error);
                            return;
                        }

                        return;
                    }
                    IsLoggedIn = true;
                    _AuthenticatorData.SessionData = RememberMe ? steam.Session.ToString() : null;
                    AuthService.Current.SaveCurrentAuth();
                }
                try
                {
                    Confirmations = new BindingList<SteamClient.Confirmation>(steam.GetConfirmations());

                    // 获取新交易后保存
                    if (!string.IsNullOrEmpty(_AuthenticatorData.SessionData))
                    {
                        AuthService.Current.SaveCurrentAuth();
                    }
                }
                catch (SteamClient.UnauthorisedSteamRequestException)
                {
                    // Family view is probably on
                    this.Dialog("You are not allowed to view confirmations. Have you enabled 'community-generated content' in Family View?");
                    return;
                }
                catch (SteamClient.InvalidSteamRequestException)
                {
                    // likely a bad session so try a refresh first
                    try
                    {
                        steam.Refresh();
                        Confirmations = new BindingList<SteamClient.Confirmation>(steam.GetConfirmations());
                    }
                    catch (Exception)
                    {
                        // reset and show normal login
                        steam.Clear();
                        return;
                    }
                }
            }).ContinueWith(s => { Logger.Error(s.Exception); WindowService.Current.ShowDialogWindow(s.Exception.Message); }, TaskContinuationOptions.OnlyOnFaulted);
        }

        public void ConfirmTrade_Click(SteamClient.Confirmation trade)
        {
            OperationTrade(true, trade);
        }

        public void CancelTrade_Click(SteamClient.Confirmation trade)
        {
            OperationTrade(false, trade);
        }

        private void OperationTrade(bool accept, SteamClient.Confirmation trade)
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
                    StatusService.Current.Notify($"已{(accept ? "同意" : "取消")}{trade.Details}");
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        Confirmations.Remove(trade);
                    });
                    //Refresh_Click();
                }
            }).ContinueWith(s =>
            {
                Logger.Error(s.Exception); WindowService.Current.ShowDialogWindow(s.Exception.Message);
            }, TaskContinuationOptions.OnlyOnFaulted);
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
                    return this._AuthenticatorData.GetClient().ConfirmTrade(((SteamClient.Confirmation)t).Id, ((SteamClient.Confirmation)t).Key, true);
                }, trade);
                if (result == false)
                {
                    throw new ApplicationException("无法确认此交易");
                }

                return true;
            }

            catch (InvalidTradesResponseException ex)
            {
                Logger.Error("允许交易时出错", ex);
                this.Dialog("允许交易时出错：" + ex);
                return false;
            }
            catch (ApplicationException ex)
            {
                this.Dialog(ex.ToString());
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
                    return this._AuthenticatorData.GetClient().ConfirmTrade(((SteamClient.Confirmation)t).Id, ((SteamClient.Confirmation)t).Key, false);
                }, trade);
                if (result == false)
                {
                    throw new ApplicationException("无法取消的交易");
                }

                return true;
            }

            catch (InvalidTradesResponseException ex)
            {
                Logger.Error("拒绝交易时出错", ex);
                this.Dialog("拒绝交易时出错：" + ex);
                return false;
            }
            catch (ApplicationException ex)
            {
                this.Dialog(ex.ToString());
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
                StatusService.Current.Notify($"当前没有任何交易");
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
            if (!this.Dialog("这将" + str + "您当前所有的交易确认。" + Environment.NewLine + Environment.NewLine +
                "确定要继续吗?"))
            {
                return;
            }

            StatusService.Current.Set($"正在{str}所有交易中...");

            if (accept)
                AcceptAllTrade();
            else
                RejectAllTrade();

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
                        var delay = SteamClient.CONFIRMATION_EVENT_DELAY + rand.Next(SteamClient.CONFIRMATION_EVENT_DELAY / 2); // delay is 100%-150% of CONFIRMATION_EVENT_DELAY
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
                StatusService.Current.Set(Resources.Ready);
                StatusService.Current.Notify($"已成功同意所有交易");
                AuthService.Current.SaveCurrentAuth();
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
                        var delay = SteamClient.CONFIRMATION_EVENT_DELAY + rand.Next(SteamClient.CONFIRMATION_EVENT_DELAY / 2); // delay is 100%-150% of CONFIRMATION_EVENT_DELAY
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
                StatusService.Current.Set(Resources.Ready);
                StatusService.Current.Notify($"已成功取消所有交易");
                AuthService.Current.SaveCurrentAuth();
            }
        }

    }
}
