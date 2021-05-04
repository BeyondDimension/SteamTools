using Newtonsoft.Json.Linq;
using System.Application.Models;
using System.Application.UI.Resx;
using System.Properties;

namespace System.Application.UI.ViewModels
{
    public class ShowAuthWindowViewModel : WindowViewModel
    {
        private readonly GAPAuthenticatorValueDTO.SteamAuthenticator? _Authenticator;

        public ShowAuthWindowViewModel() : base()
        {
            Title = ThisAssembly.AssemblyTrademark + " | " + AppResources.LocalAuth_AuthData;
        }
        public ShowAuthWindowViewModel(MyAuthenticator auth) : this()
        {
            if (auth.AuthenticatorData.Value is GAPAuthenticatorValueDTO.SteamAuthenticator authenticator)
            {
                _Authenticator = authenticator;
            }
        }

        public string? RecoveryCode => _Authenticator?.RecoveryCode;

        public string? SteamData => _Authenticator?.SteamData;

        public string? DeviceId => _Authenticator?.DeviceId;



    }
}