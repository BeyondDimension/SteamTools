using ReactiveUI;
using System.Application.Mvvm;
using System.Application.Services;
using System.Application.UI.Resx;
using System.Application.UI.ViewModels;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using WinAuth;

namespace System.Application.Models
{
    public sealed partial class MyAuthenticator : ReactiveObject
    {
        public static List<MyAuthenticator> Get(IEnumerable<IGAPAuthenticatorDTO> items)
            => items.Select(x => new MyAuthenticator(x)).ToList();

        public MyAuthenticator()
        {
            AuthenticatorData = new GAPAuthenticatorDTO();
            OriginName = string.Empty;
        }

        public MyAuthenticator(IGAPAuthenticatorDTO data)
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

        public const int CodeCountdownMax = 100;
        private int _CodeCountdown = CodeCountdownMax;
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

        string GetCurrentCode()
        {
            try
            {
                return AuthenticatorData.Value.CurrentCode;
            }
            catch
            {
                return string.Empty;
            }
        }

        public Task<Stream?> QrCodeStream => GetQrCodeStream();

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
            var authenticatorType = reader.GetAttribute("type");
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

        public Task<Stream?> GetQrCodeStream() => AuthService.GetQrCodeStreamAsync(AuthenticatorData);

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
                MaxLength = IGAPAuthenticatorDTO.MaxLength_Name,
            });
            if (value == null)
                return;
            Name = value;
            await AuthService.Current.SaveEditNameByAuthenticatorAsync(this);
        }

        public void CopyCodeCilp()
        {
            Clipboard2.SetText(CurrentCode);
            Toast.Show(AppResources.LocalAuth_CopyAuthTip + Name);
        }

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
    }
}
