using DynamicData;
using ReactiveUI;
using System;
using System.Application.Models;
using System.Application.Repositories;
using System.Application.Services;
using System.Application.UI.Resx;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Properties;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WinAuth;

// ReSharper disable once CheckNamespace
namespace System.Application.UI.ViewModels
{
    public partial class AuthTradeWindowViewModel : MyAuthenticatorWindowViewModel
    {
        const string TAG = "AuthTradeWindowVM";

        public AuthTradeWindowViewModel() : base()
        {
        }

        public AuthTradeWindowViewModel(MyAuthenticator? auth) : base(auth)
        {
        }

        public static string DisplayName => AppResources.LocalAuth_SteamAuthTrade;

        protected override void InitializeComponent()
        {
            Title = DisplayName;

            _ConfirmationsSourceList
              .Connect()
              .ObserveOn(RxApp.MainThreadScheduler)
              //.Sort(SortExpressionComparer<WinAuthSteamClient.Confirmation>.Descending(x => x.))
              .Bind(out _Confirmations)
              .Subscribe(_ =>
              {
                  this.RaisePropertyChanged(nameof(IsConfirmationsAny));
                  this.RaisePropertyChanged(nameof(ConfirmationsConutMessage));
              });

            RegisterSelectAllObservable();

            Initialize();

            if (_Authenticator != null)
            {
                UserName = _Authenticator.AccountName;
                if (!string.IsNullOrEmpty(_MyAuthenticator?.Name))
                {
                    Title = $"{Title} | {_MyAuthenticator.Name}";
                }
                Refresh_Click();
            }
            else if (MyAuthenticator != null)
            {
                // 非 Steam 令牌无法弹出确认交易框
                throw new NotSupportedException("Authenticator is not SteamAuthenticator");
            }
        }

        /// <summary>
        /// 是否使用 <see cref="IHttpService"/> 加载确认物品图片 <see cref="Stream"/>
        /// </summary>
        static bool IsLoadImage
        {
            get
            {
                // 此页面当前使用 Square.Picasso 库加载图片
                if (OperatingSystem2.IsAndroid) return false;
                return true;
            }
        }

        private string? AuthPassword;
        private bool AuthIsLocal;

        private new async void Initialize()
        {
            var repository = DI.Get<IGameAccountPlatformAuthenticatorRepository>();
            var (success, password) = await AuthService.Current.HasPasswordEncryptionShowPassWordWindow();
            if (success)
            {
                AuthPassword = password;
            }
            else
            {
                AuthPassword = null;
            }
            var auths = await repository.GetAllSourceAsync();
            AuthIsLocal = repository.HasLocal(auths);
        }
        public override void Deactivation()
        {
            var steam = _Authenticator!.GetClient();
            if (!steam.IsLoggedIn())
            {
                steam.Clear();
            }
            base.Deactivation();
        }

        #region LoginData

        private string? _UserName;
        public string? UserName
        {
            get => _UserName;
            set
            {
                if (_UserName != value)
                {
                    _UserName = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        private string? _Password;
        public string? Password
        {
            get => _Password;
            set
            {
                if (_Password != value)
                {
                    _Password = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        private bool _RememberMe;
        public bool RememberMe
        {
            get => _RememberMe;
            set
            {
                if (_RememberMe != value)
                {
                    _RememberMe = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        private string? _CaptchaImage;
        public string? CaptchaImage
        {
            get => _CaptchaImage;
            set
            {
                if (_CaptchaImage != value)
                {
                    _CaptchaImage = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        public void CaptchaUrlButton_Click()
        {
            AuthService.ShowCaptchaUrl(CaptchaImage);
        }

        private string? _CaptchaText;
        public string? CaptchaText
        {
            get => _CaptchaText;
            set
            {
                if (_CaptchaText != value)
                {
                    _CaptchaText = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        #endregion

        public bool IsLoggedIn
        {
            get => _Authenticator?.GetClient().IsLoggedIn() ?? false;
            set
            {
                this.RaisePropertyChanged();
            }
        }

        public bool IsRequiresCaptcha
        {
            get => _Authenticator!.GetClient().RequiresCaptcha;
            set
            {
                this.RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Cancellation token for confirm all
        /// </summary>
        private CancellationTokenSource? CancelComfirmAll;

        /// <summary>
        /// Cancellation token for cancel all
        /// </summary>
        private CancellationTokenSource? CancelCancelAll;

        private ReadOnlyObservableCollection<WinAuthSteamClient.Confirmation>? _Confirmations;
        public ReadOnlyObservableCollection<WinAuthSteamClient.Confirmation> Confirmations => _Confirmations ?? throw new ArgumentNullException(nameof(_Confirmations));

        private readonly SourceList<WinAuthSteamClient.Confirmation> _ConfirmationsSourceList = new();
        public SourceList<WinAuthSteamClient.Confirmation> ConfirmationsSourceList =>
            _ConfirmationsSourceList;

        private bool _IsLoading;
        public bool IsLoading
        {
            get => _IsLoading;
            set
            {
                if (_IsLoading != value)
                {
                    _IsLoading = value;
                    this.RaisePropertyChanged();
                    this.RaisePropertyChanged(nameof(ConfirmationsConutMessage));
                }
            }
        }

        /// <summary>
        /// Confirmations SourceList Any
        /// </summary>
        public bool IsConfirmationsAny => _ConfirmationsSourceList.Items.Any_Nullable();

        public string ConfirmationsConutMessage
        {
            get
            {
                if (IsLoading)
                {
                    return string.Empty;
                }
                if (!IsConfirmationsAny)
                {
                    return AppResources.LocalAuth_AuthTrade_ListNullTip;
                }
                return AppResources.LocalAuth_AuthTrade_ListCountTip.Format(_ConfirmationsSourceList.Count);
            }
        }

        public void LoginButton_Click()
        {
            if (!string.IsNullOrWhiteSpace(UserName) && !string.IsNullOrWhiteSpace(Password))
            {
                if (!string.IsNullOrWhiteSpace(CaptchaText))
                {
                    Process(CaptchaText);
                }
                else
                {
                    Process();
                }
            }
            else
            {
                Toast.Show(AppResources.User_LoginError_Null);
                return;
            }
        }

        public void Refresh_Click()
        {
            Process();
        }

        private void RefreshConfirmationsList()
        {
            var items = _ConfirmationsSourceList.Items.Where(s => s.IsOperate == 0);
            _ConfirmationsSourceList.Clear();
            if (items.Any())
                _ConfirmationsSourceList.AddRange(items);
        }

        public async void Logout_Click()
        {
            var r = await MessageBox.ShowAsync(AppResources.LocalAuth_LogoutTip, button: MessageBox.Button.OKCancel);
            if (r == MessageBox.Result.OK)
            {
                var steam = _Authenticator!.GetClient();
                steam.Logout();

                if (string.IsNullOrEmpty(_Authenticator.SessionData) == false)
                {
                    IsLoggedIn = false;
                    _Authenticator.SessionData = null;
                    AuthService.Current.AddOrUpdateSaveAuthenticators(MyAuthenticator!, AuthIsLocal, AuthPassword);
                }
            }
        }


        private string? captchaId;

        private void Process(string? codeChar = null)
        {
            if (IsLoading || _Authenticator == null)
                return;
            IsLoading = true;
            Task.Run(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                var steam = _Authenticator.GetClient();
                try
                {
                    if (!IsLoggedIn)
                    {
                        MainThread2.BeginInvokeOnMainThread(() =>
                        {
                            IsLoading = false;
                            if (string.IsNullOrEmpty(Password) || string.IsNullOrEmpty(UserName))
                            {
                                return;
                            }
                            if (ToastService.IsSupported)
                            {
                                ToastService.Current.Set(AppResources.Logining);
                            }
                            LoadingText = AppResources.Logining;
                            if (string.IsNullOrEmpty(codeChar))
                            {
                                captchaId = null;
                            }
                        });
                        var loginResult = steam.Login(UserName, Password, captchaId, codeChar, R.GetCurrentCultureSteamLanguageName());
                        MainThread2.BeginInvokeOnMainThread(() =>
                        {
                            LoadingText = null;
                            if (!loginResult)
                            {
                                if (steam.Error == "Incorrect Login")
                                {
                                    Toast.Show(AppResources.User_LoginError);
                                    return;
                                }

                                if (steam.Requires2FA == true)
                                {
                                    Toast.Show(AppResources.User_LoginError_Auth);
                                    return;
                                }

                                if (steam.RequiresCaptcha == true)
                                {
                                    IsRequiresCaptcha = steam.RequiresCaptcha;
                                    captchaId = steam.CaptchaId;
                                    CaptchaImage = steam.CaptchaUrl;
                                    Toast.Show(AppResources.User_LoginError_CodeImage);
                                    return;
                                }
                                //loginButton.Enabled = true;
                                //captchaGroup.Visible = false;

                                if (string.IsNullOrEmpty(steam.Error) == false)
                                {
                                    Toast.Show(steam.Error);
                                    return;
                                }
                                return;
                            }
                            Toast.Show(AppResources.User_LoiginSuccess);
                            IsLoggedIn = true;
                            _Authenticator.SessionData = RememberMe ? steam.Session.ToString() : null;
                            AuthService.Current.AddOrUpdateSaveAuthenticators(MyAuthenticator!, AuthIsLocal, AuthPassword);
                        });
                    }

                    MainThread2.BeginInvokeOnMainThread(() =>
                    {
                        IsLoading = true;
                        LoadingText = AppResources.LocalAuth_AuthTrade_GetTip;
                        //Toast.Show(AppResources.LocalAuth_AuthTrade_GetTip);
                    });
                    var list = steam.GetConfirmations();

                    if (IsLoadImage)
                    {
                        Parallel.ForEach(list, confirmation =>
                        {
                            confirmation.ImageStream = IHttpService.Instance.GetImageAsync(confirmation.Image, ImageChannelType.SteamEconomys);
                        });
                    }
                    MainThread2.BeginInvokeOnMainThread(() =>
                    {
                        _ConfirmationsSourceList.Clear();
                        _ConfirmationsSourceList.AddRange(list);

                        // 获取新交易后保存
                        if (!string.IsNullOrEmpty(_Authenticator.SessionData))
                        {
                            AuthService.Current.AddOrUpdateSaveAuthenticators(MyAuthenticator!, AuthIsLocal, AuthPassword);
                        }
                    });
                }
                catch (WinAuthUnauthorisedSteamRequestException)
                {
                    MainThread2.BeginInvokeOnMainThread(() =>
                    {
                        // Family view is probably on
                        Toast.Show(AppResources.LocalAuth_AuthTrade_GetError);
                    });
                    return;
                }
                catch (WinAuthInvalidSteamRequestException e)
                {
                    // likely a bad session so try a refresh first
                    try
                    {
                        if (steam.IsLoggedIn())
                        {
                            steam.Refresh();
                            var list = steam.GetConfirmations();

                            if (IsLoadImage)
                            {
                                Parallel.ForEach(list, confirmation =>
                                {
                                    confirmation.ImageStream = IHttpService.Instance.GetImageAsync(confirmation.Image, ImageChannelType.SteamEconomys);
                                });
                            }

                            MainThread2.BeginInvokeOnMainThread(() =>
                            {
                                _ConfirmationsSourceList.Clear();
                                _ConfirmationsSourceList.AddRange(list);
                            });
                        }
                        else throw e;
                    }
                    catch (Exception ex)
                    {
                        // reset and show normal login
                        Log.Error(nameof(Process), ex, "可能是没有开加速器导致无法连接Steam社区登录地址");

                        if (ex.InnerException != null &&
                            ex.Message.Contains("302"))
                        {
                            MainThread2.BeginInvokeOnMainThread(() =>
                            {
                                Toast.Show(AppResources.LocalAuth_AuthTrade_GetError3);
                                IsLoading = false;
                            });
                            steam.Clear();
                            return;
                        }

                        MainThread2.BeginInvokeOnMainThread(() =>
                        {
#if DEBUG
                            MessageBox.Show($"发生错误：{ex.Message}{Environment.NewLine}堆栈信息：{ex.StackTrace}");
#else
                            Toast.Show(AppResources.LocalAuth_AuthTrade_GetError2);
#endif
                        });
                        return;
                    }
                }
                finally
                {
                    MainThread2.BeginInvokeOnMainThread(() =>
                    {
                        IsLoading = false;
                    });
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
                    result = await AcceptTrade(trade);
                else
                    result = await RejectTrade(trade);

                if (result)
                {
                    Toast.Show($"{(accept ? AppResources.Agree : AppResources.Cancel)}{trade.Details}");
                    MainThread2.BeginInvokeOnMainThread(() =>
                    {
                        trade.IsOperate = accept ? 1 : 2;
                        RefreshConfirmationsList();
                        //Confirmations.Remove(trade);
                    });
                    //Refresh_Click();
                }
            }).ForgetAndDispose();
        }

        /// <summary>
        /// Accept the trade Confirmation
        /// </summary>
        /// <param name="tradeId">Id of Confirmation</param>
        private async Task<bool> AcceptTrade(WinAuthSteamClient.Confirmation trade)
        {
            try
            {
                if (trade == null)
                {
                    //throw new ApplicationException(AppResources.LocalAuth_AuthTrade_TradeError);
                    Toast.Show(AppResources.LocalAuth_AuthTrade_TradeError);
                    return false;
                }

                var result = await Task.Run<bool>(() =>
                {
                    return _Authenticator!.GetClient().ConfirmTrade(trade.Id, trade.Key, true);
                });
                if (result == false)
                {
                    //throw new ApplicationException(AppResources.LocalAuth_AuthTrade_ConfirmError);
                    Toast.Show(AppResources.LocalAuth_AuthTrade_ConfirmError);
                    return false;
                }

                return true;
            }
            catch (WinAuthInvalidTradesResponseException ex)
            {
                ex.LogAndShowT(TAG);
                return false;
            }
            catch (ApplicationException ex)
            {
                ex.LogAndShowT(TAG);
                return false;
            }
        }

        /// <summary>
        /// Reject the trade Confirmation
        /// </summary>
        /// <param name="tradeId">ID of Confirmation</param>
        private async Task<bool> RejectTrade(WinAuthSteamClient.Confirmation trade)
        {
            try
            {
                if (trade == null)
                {
                    //throw new ApplicationException(AppResources.LocalAuth_AuthTrade_TradeError);
                    Toast.Show(AppResources.LocalAuth_AuthTrade_TradeError);
                    return false;
                }
                var result = await Task.Run(() =>
                {
                    return _Authenticator!.GetClient().ConfirmTrade(trade.Id, trade.Key, false);
                });
                if (result == false)
                {
                    //throw new ApplicationException(AppResources.LocalAuth_AuthTrade_CancelError);
                    Toast.Show(AppResources.LocalAuth_AuthTrade_CancelError);
                    return false;
                }

                return true;
            }
            catch (WinAuthInvalidTradesResponseException ex)
            {
                ex.LogAndShowT(TAG);
                return false;
            }
            catch (ApplicationException ex)
            {
                ex.LogAndShowT(TAG);
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

        private async void OperationTrades(bool accept)
        {
            if (!_ConfirmationsSourceList.Items.Any_Nullable())
            {
                Toast.Show(AppResources.LocalAuth_AuthTrade_Null);
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

            var str = accept ? AppResources.Agree : AppResources.Cancel;

            var result = await MessageBox.ShowAsync(AppResources.LocalAuth_AuthTrade_MessageBoxTip.Format(str), button: MessageBox.Button.OKCancel);

            if (result == MessageBox.Result.OK)
            {
                var text = AppResources.LocalAuth_AuthTrade_ConfirmTip.Format(str);
                if (ToastService.IsSupported)
                {
                    ToastService.Current.Set(text);
                }
                LoadingText = text;

                if (accept)
                    AcceptAllTrade();
                else
                    RejectAllTrade();
            }
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
                foreach (var item in _ConfirmationsSourceList.Items)
                {
                    if (CancelComfirmAll.IsCancellationRequested)
                    {
                        return;
                    }

                    if (item.IsOperate != 0 || item.NotChecked)
                    {
                        continue;
                    }

                    DateTime start = DateTime.Now;

                    var result = await AcceptTrade(item);
                    if (result == false || CancelComfirmAll.IsCancellationRequested == true)
                    {
                        return;
                    }
                    await MainThread2.InvokeOnMainThreadAsync(() =>
                    {
                        item.IsOperate = 1;
                        //Confirmations.Remove(trades[i]);
                    });

                    var duration = (int)DateTime.Now.Subtract(start).TotalMilliseconds;
                    var delay = WinAuthSteamClient.CONFIRMATION_EVENT_DELAY + Random2.Next(WinAuthSteamClient.CONFIRMATION_EVENT_DELAY / 2); // delay is 100%-150% of CONFIRMATION_EVENT_DELAY
                    if (delay > duration)
                    {
                        await Task.Delay(delay - duration);
                    }
                }
            }
            finally
            {
                CancelComfirmAll = null;
                RefreshConfirmationsList();
                LoadingText = null;
                Toast.Show(AppResources.LocalAuth_AuthTrade_ConfirmSuccess);
                AuthService.Current.AddOrUpdateSaveAuthenticators(MyAuthenticator!, AuthIsLocal, AuthPassword);
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
                foreach (var item in _ConfirmationsSourceList.Items)
                {
                    if (CancelCancelAll.IsCancellationRequested)
                    {
                        break;
                    }

                    if (item.IsOperate != 0 || item.NotChecked)
                    {
                        continue;
                    }

                    DateTime start = DateTime.Now;

                    var result = await RejectTrade(item);
                    if (result == false || CancelCancelAll.IsCancellationRequested == true)
                    {
                        break;
                    }
                    await MainThread2.InvokeOnMainThreadAsync(() =>
                    {
                        item.IsOperate = 2;
                        //Confirmations.Remove(tradeIds[i]);
                    });

                    var duration = (int)DateTime.Now.Subtract(start).TotalMilliseconds;
                    var delay = WinAuthSteamClient.CONFIRMATION_EVENT_DELAY + Random2.Next(WinAuthSteamClient.CONFIRMATION_EVENT_DELAY / 2); // delay is 100%-150% of CONFIRMATION_EVENT_DELAY
                    if (delay > duration)
                    {
                        await Task.Delay(delay - duration);
                    }
                }
            }
            finally
            {
                CancelCancelAll = null;
                RefreshConfirmationsList();
                LoadingText = null;
                Toast.Show(AppResources.LocalAuth_AuthTrade_ConfirmCancel);
                AuthService.Current.AddOrUpdateSaveAuthenticators(MyAuthenticator!, AuthIsLocal, AuthPassword);
            }
        }

        private string? _LoadingText;
        public string? LoadingText
        {
            get => _LoadingText;
            set => this.RaiseAndSetIfChanged(ref _LoadingText, value);
        }
    }
}
