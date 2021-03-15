using ReactiveUI;
using System.Collections.Generic;
using System.Linq;

namespace System.Application.Models
{
    public class MyAuthenticator : ReactiveObject
    {
        public MyAuthenticator()
        {
            IsHide = true;
        }

        public MyAuthenticator(IGAPAuthenticatorDTO data)
        {
            IsHide = true;
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

        private bool _IsHide;
        public bool IsHide
        {
            get
            {
                return _IsHide;
            }
            set
            {
                _IsHide = value;
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