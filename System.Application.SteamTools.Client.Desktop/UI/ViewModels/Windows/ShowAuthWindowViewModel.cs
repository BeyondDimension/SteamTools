using Newtonsoft.Json.Linq;
using System.Application.Models;
using System.Application.UI.Resx;
using System.Properties;

namespace System.Application.UI.ViewModels
{
    public class ShowAuthWindowViewModel : WindowViewModel
    {
        private GAPAuthenticatorValueDTO.SteamAuthenticator _Authenticator;
        public ShowAuthWindowViewModel()
        {
            Title = ThisAssembly.AssemblyTrademark + " | " + AppResources.LocalAuth_AuthData;
        }
        public ShowAuthWindowViewModel(MyAuthenticator auth)
        {
            _Authenticator = auth.AuthenticatorData.Value as GAPAuthenticatorValueDTO.SteamAuthenticator;
            Title = ThisAssembly.AssemblyTrademark + " | " + AppResources.LocalAuth_AuthData;
        }

        public string RecoveryCode => JObject.Parse(_Authenticator.SteamData).SelectToken("revocation_code").Value<string>();

        public string SteamData => _Authenticator.SteamData;

        public string DeviceId => _Authenticator.DeviceId;
    }
}