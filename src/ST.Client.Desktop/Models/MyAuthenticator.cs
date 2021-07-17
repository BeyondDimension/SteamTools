using ReactiveUI;
using System.Application.Mvvm;
using System.Application.UI.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using WinAuth;

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

        public string CurrentCode
        {
            get
            {
                return AuthenticatorData.Value.CurrentCode;
            }
            set
            {
                this.RaisePropertyChanged();
            }
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
        /// <summary>
        /// 开始自动刷新一次性密码代码
        /// </summary>
        /// <param name="token"></param>
        void StartAutoRefreshCode(CancellationToken token)
        {

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
                i.AutoRefreshCode.Dispose();
                i.AutoRefreshCode.RemoveTo(i);
                if (!noSetNull) i.AutoRefreshCode = null;
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
        /// 编辑令牌自定义名称，仅修改VM对象，需调用 <see cref="Services.AuthService.SaveEditNameAuthenticators"/> 保存到本地数据库中
        /// </summary>
        /// <returns></returns>
        public async Task EditNameAsync()
        {
            var value = await TextBoxWindowViewModel.ShowDialogAsync(new()
            {
                Value = Name,
            });
            Name = value ?? string.Empty;
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