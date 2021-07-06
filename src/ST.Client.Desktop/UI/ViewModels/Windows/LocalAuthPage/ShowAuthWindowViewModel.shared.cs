using System.Application.Models;
using System.Application.UI.Resx;
using System.Properties;

namespace System.Application.UI.ViewModels
{
    public partial class ShowAuthWindowViewModel
    {
        public static string DisplayName => AppResources.LocalAuth_AuthData;

#if !__MOBILE__
        private readonly GAPAuthenticatorValueDTO.SteamAuthenticator? _Authenticator;
#endif

        public ShowAuthWindowViewModel() : base()
        {
            Title =
#if !__MOBILE__
                ThisAssembly.AssemblyTrademark + " | " +
#endif
                DisplayName;
        }

        public ShowAuthWindowViewModel(MyAuthenticator auth) : this()
        {
#if __MOBILE__
            MyAuthenticator = auth;
#endif
#if !__MOBILE__
            if (auth.AuthenticatorData.Value is GAPAuthenticatorValueDTO.SteamAuthenticator authenticator)
            {
                _Authenticator = authenticator;
            }
#endif
        }
    }
}