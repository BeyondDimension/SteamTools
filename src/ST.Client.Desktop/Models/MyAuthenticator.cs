using ReactiveUI;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using WinAuth;

namespace System.Application.Models
{
    public class MyAuthenticator : ReactiveObject
    {
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