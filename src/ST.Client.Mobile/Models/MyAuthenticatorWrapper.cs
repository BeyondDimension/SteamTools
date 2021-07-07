using System.Application.UI.ViewModels;

namespace System.Application.Models
{
    public class MyAuthenticatorWrapper : PageViewModel
    {
        public MyAuthenticator Authenticator { get; }

        public MyAuthenticatorWrapper() => throw new NotSupportedException();

        public MyAuthenticatorWrapper(MyAuthenticator authenticator)
        {
            Authenticator = authenticator;
        }
    }
}