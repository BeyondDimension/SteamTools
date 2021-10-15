using System.Application.UI.ViewModels;

namespace System.Application.Models
{
    public class MyAuthenticatorWrapper : PageViewModel, IExplicitHasValue
    {
        public MyAuthenticator? Authenticator { get; }

        public MyAuthenticatorWrapper() : base() { }

        public MyAuthenticatorWrapper(MyAuthenticator authenticator) : base()
        {
            Authenticator = authenticator;
        }

        bool IExplicitHasValue.ExplicitHasValue() => Authenticator != null;

        public static implicit operator MyAuthenticatorWrapper(MyAuthenticator authenticator) => new(authenticator);

        public static implicit operator MyAuthenticator?(MyAuthenticatorWrapper wrapper) => wrapper.Authenticator;
    }
}