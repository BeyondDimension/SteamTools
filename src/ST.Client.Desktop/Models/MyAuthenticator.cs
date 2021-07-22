using ReactiveUI;
using System.Application.Mvvm;
using System.Application.Services;
using System.Application.UI.Resx;
using System.Application.UI.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using WinAuth;
using static System.Application.Models.GAPAuthenticatorValueDTO;

namespace System.Application.Models
{
    public class MyAuthenticator : ReactiveObject
    {
#if __MOBILE__
        public static string CodeFormat(string? code)
        {
            if (string.IsNullOrEmpty(code))
            {
                return string.Empty;
            }
            var c3 = code.Length / 3;
            if (code.Length % 3 == 0 && c3 > 1)
            {
                var list = code.ToCharArray().ToList();
                for (int i = 1; i < c3; i++)
                {
                    list.Insert(i * 3, ' ');
                }
                return new string(list.ToArray());
            }
            else
            {
                var arr = code.ToCharArray();
                return string.Join(' ', arr);
            }
        }
#endif

        public MyAuthenticator()
        {
        }

        public MyAuthenticator(IGAPAuthenticatorDTO data) : this()
        {
            AuthenticatorData = data;
            OriginName = AuthenticatorData.Name;
        }

        public IGAPAuthenticatorDTO AuthenticatorData { get; set; }

        public ushort Id
        {
            get
            {
                return AuthenticatorData.Id;
            }
            set
            {
                AuthenticatorData.Id = value;
                this.RaisePropertyChanged();
            }
        }

        public string OriginName;
        public string Name
        {
            get
            {
                return AuthenticatorData.Name;
            }
            set
            {
                AuthenticatorData.Name = value;
                this.RaisePropertyChanged();
            }
        }


        public int Index
        {
            get
            {
                return AuthenticatorData.Index;
            }
            set
            {
                AuthenticatorData.Index = value;
                this.RaisePropertyChanged();
            }
        }

        public DateTimeOffset Create
        {
            get
            {
                return AuthenticatorData.Created;
            }
            set
            {
                AuthenticatorData.Created = value;
                this.RaisePropertyChanged();
            }
        }

        public int Period
        {
            get
            {
                return AuthenticatorData.Value.Period;
            }
            set
            {
                AuthenticatorData.Value.Period = value;
                this.RaisePropertyChanged();
            }
        }

#if !__MOBILE__
        private bool _IsShowCode;
        public bool IsShowCode
        {
            get
            {
                return _IsShowCode;
            }
            set
            {
                _IsShowCode = value;
                this.RaisePropertyChanged();
            }
        }
#endif

        private int _CodeCountdown;
        public int CodeCountdown
        {
            get
            {
                return _CodeCountdown;
            }
            set
            {
                _CodeCountdown = value;
                this.RaisePropertyChanged();
            }
        }

        string _CurrentCode = string.Empty;
        string? _CurrentCodeCache;

        string GetCurrentCode() => AuthenticatorData.Value.CurrentCode;

        public string CurrentCode
        {
            get
            {
                if (_CurrentCodeCache == null)
                {
                    _CurrentCode = GetCurrentCode();
                    return _CurrentCode;
                }
                else
                {
                    var r = _CurrentCodeCache;
                    _CurrentCodeCache = null;
                    return r;
                }
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _CurrentCode, value);
            }
        }

        public string CurrentCodeCache
        {
            set => _CurrentCodeCache = value;
        }

        public string? GetNextCode()
        {
            var value = GetCurrentCode();
            if (value != _CurrentCode) return value;
            return null;
        }

        public void RefreshCode(string? value = null)
        {
            value ??= GetCurrentCode();
            CurrentCodeCache = value;
            CurrentCode = value;
        }

        public void Sync() => AuthenticatorData.Value.Sync();

        public bool ReadXml(XmlReader reader, string? password)
        {
            bool changed = false;

            //if (Guid.TryParse(reader.GetAttribute("id"), out Guid id) == true)
            //{
            //    Id = id;
            //}
            AuthenticatorData = new GAPAuthenticatorDTO();
            string authenticatorType = reader.GetAttribute("type");
            switch (authenticatorType)
            {
                case "WinAuth.SteamAuthenticator":
                    AuthenticatorData.Value = new GAPAuthenticatorValueDTO.SteamAuthenticator();
                    break;
                case "WinAuth.BattleNetAuthenticator":
                    AuthenticatorData.Value = new GAPAuthenticatorValueDTO.BattleNetAuthenticator();
                    break;
                case "WinAuth.GoogleAuthenticator":
                    AuthenticatorData.Value = new GAPAuthenticatorValueDTO.GoogleAuthenticator();
                    break;
                case "WinAuth.HOTPAuthenticator":
                    AuthenticatorData.Value = new GAPAuthenticatorValueDTO.HOTPAuthenticator();
                    break;
                case "WinAuth.MicrosoftAuthenticator":
                    AuthenticatorData.Value = new GAPAuthenticatorValueDTO.MicrosoftAuthenticator();
                    break;
                default:
                    return false;
            }

            //string encrypted = reader.GetAttribute("encrypted");
            //if (string.IsNullOrEmpty(encrypted) == false)
            //{
            //	// read the encrypted text from the node
            //	string data = reader.ReadElementContentAsString();
            //	// decrypt
            //	Authenticator.PasswordTypes passwordType;
            //	data = Authenticator.DecryptSequence(data, encrypted, password, out passwordType);

            //	using (MemoryStream ms = new MemoryStream(Authenticator.StringToByteArray(data)))
            //	{
            //		reader = XmlReader.Create(ms);
            //		ReadXml(reader, password);
            //	}
            //	this.PasswordType = passwordType;
            //	this.Password = password;

            //	return;
            //}

            reader.MoveToContent();

            if (reader.IsEmptyElement)
            {
                reader.Read();
                return changed;
            }

            reader.Read();
            while (reader.EOF == false)
            {
                if (reader.IsStartElement())
                {
                    switch (reader.Name)
                    {
                        case "name":
                            Name = reader.ReadElementContentAsString();
                            OriginName = Name;
                            break;

                        case "created":
                            long t = reader.ReadElementContentAsLong();
                            t += Convert.ToInt64(new TimeSpan(new DateTime(1970, 1, 1).Ticks).TotalMilliseconds);
                            t *= TimeSpan.TicksPerMillisecond;
                            Create = new DateTimeOffset(new DateTime(t).ToLocalTime());
                            break;

                        case "authenticatordata":
                            try
                            {
                                // we don't pass the password as they are locked till clicked
                                changed = AuthenticatorData.Value.ReadXml(reader) || changed;
                            }
                            catch (WinAuthEncryptedSecretDataException)
                            {
                                // no action needed
                            }
                            catch (WinAuthBadPasswordException)
                            {
                                // no action needed
                            }
                            break;

                        // v2
                        case "authenticator":
                            AuthenticatorData.Value = GAPAuthenticatorValueDTO.ReadXmlv2(reader, password);
                            break;
                        // v2
                        case "servertimediff":
                            AuthenticatorData.Value.ServerTimeDiff = reader.ReadElementContentAsLong();
                            break;


                        default:
                            reader.Skip();
                            break;
                    }
                }
                else
                {
                    reader.Read();
                    break;
                }
            }

            return changed;
        }

        public static List<MyAuthenticator> Get(IEnumerable<IGAPAuthenticatorDTO> items)
            => items.Select(x => new MyAuthenticator(x)).ToList();

#if __MOBILE__

        private int _AutoRefreshCodeTimingCurrent;
        public int AutoRefreshCodeTimingCurrent
        {
            get => _AutoRefreshCodeTimingCurrent;
            set => this.RaiseAndSetIfChanged(ref _AutoRefreshCodeTimingCurrent, value);
        }

        int TimingCycle
        {
            get
            {
                var value = AuthenticatorData.Value.ServerTime % Convert.ToInt64(TimeSpan.FromSeconds(Period).TotalMilliseconds);
                return Period - Convert.ToInt32(TimeSpan.FromMilliseconds(value).TotalSeconds);
            }
        }

        /// <summary>
        /// 开始自动刷新一次性密码代码
        /// </summary>
        /// <param name="token"></param>
        async void StartAutoRefreshCode(CancellationToken token)
        {
            try
            {
                await Task.Run(async () =>
                {
                    var value = TimingCycle;
                    while (true)
                    {
                        var isZero = value-- <= 0;
                        if (isZero)
                        {
                            value = Period;
                        }
#if DEBUG
                        Log.Debug("AutoRefreshCode", "while({1}), name: {0}", Name, value);
#endif
                        token.ThrowIfCancellationRequested();
                        string? code = null;
                        if (isZero)
                        {
                            while (true)
                            {
                                code = GetNextCode();
                                if (code != null) break;
                                await Task.Delay(100, token);
                            }
                        }
                        await MainThread2.InvokeOnMainThreadAsync(() =>
                        {
                            AutoRefreshCodeTimingCurrent = value;
                            if (isZero)
                            {
                                RefreshCode(code);
                            }
                        });
                        await Task.Delay(1000, token);
                    }
                });
            }
            catch (OperationCanceledException)
            {

            }
        }

        /// <summary>
        /// 停止当前自动刷新一次性密码代码
        /// </summary>
        /// <param name="i"></param>
        /// <param name="noSetNull"></param>
        public static void StopAutoRefreshCode(IAutoRefreshCode i, bool noSetNull = false)
        {
            if (i.AutoRefreshCode != null)
            {
                i.AutoRefreshCode.Cancel();
                i.AutoRefreshCode.Dispose();
                i.AutoRefreshCode.RemoveTo(i);
                if (!noSetNull) i.AutoRefreshCode = null;
#if DEBUG
                Log.Debug("AutoRefreshCode", "Stop, name: {0}", i.ViewModel?.Name);
#endif
            }
        }

        /// <summary>
        /// 开始自动刷新一次性密码代码
        /// </summary>
        /// <param name="i"></param>
        /// <param name="noStop"></param>
        public static void StartAutoRefreshCode(IAutoRefreshCode i, bool noStop = false)
        {
            if (!noStop) StopAutoRefreshCode(i, noSetNull: true);
            i.AutoRefreshCode = new();
            i.AutoRefreshCode.AddTo(i);
            i.ViewModel!.StartAutoRefreshCode(i.AutoRefreshCode.Token);
#if DEBUG
            Log.Debug("AutoRefreshCode", "Start, name: {0}", i.ViewModel?.Name);
#endif
        }

        /// <summary>
        /// 自动刷新一次性密码代码
        /// </summary>
        public interface IAutoRefreshCode : IDisposableHolder, IReadOnlyViewFor<MyAuthenticator>
        {
            CancellationTokenSource? AutoRefreshCode { get; set; }
        }
#endif

        /// <summary>
        /// 编辑令牌自定义名称
        /// </summary>
        /// <returns></returns>
        public async Task EditNameAsync()
        {
            var value = await TextBoxWindowViewModel.ShowDialogAsync(new()
            {
                Value = Name,
                Title = AppResources.EditName,
            });
            Name = value ?? string.Empty;
            await AuthService.Current.SaveEditNameByAuthenticatorAsync(this);
        }

        public void CopyCodeCilp()
        {
            Clipboard2.SetText(CurrentCode);
            Toast.Show(AppResources.LocalAuth_CopyAuthTip + Name);
        }
    }

    public class ImportedSDAEntry
    {
        public const int PBKDF2_ITERATIONS = 50000;
        public const int SALT_LENGTH = 8;
        public const int KEY_SIZE_BYTES = 32;
        public const int IV_LENGTH = 16;

        public string Username;
        public string SteamId;
        public string json;

        public override string ToString()
        {
            return Username + " (" + SteamId + ")";
        }
    }
}