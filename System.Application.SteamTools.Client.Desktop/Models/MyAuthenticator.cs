using ReactiveUI;
using System.Collections.Generic;
using System.Linq;

namespace System.Application.Models
{
    public class MyAuthenticator : ReactiveObject
    {
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

        public void Sync() => AuthenticatorData.Value.Sync();

        public static List<MyAuthenticator> Get(IEnumerable<IGAPAuthenticatorDTO> items)
            => items.Select(x => new MyAuthenticator(x)).ToList();
    }
}