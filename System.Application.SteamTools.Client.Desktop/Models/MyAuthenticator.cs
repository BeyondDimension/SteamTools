using ReactiveUI;
using System.Collections.Generic;
using System.Linq;

namespace System.Application.Models
{
    public class MyAuthenticator : ReactiveObject
    {
        public MyAuthenticator()
        {
        }

        public MyAuthenticator(IGAPAuthenticatorDTO data)
        {
            AuthenticatorData = data;
        }

        public IGAPAuthenticatorDTO AuthenticatorData { get; set; }

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

        public static List<MyAuthenticator> Get(IEnumerable<IGAPAuthenticatorDTO> items)
            => items.Select(x => new MyAuthenticator(x)).ToList();
    }
}