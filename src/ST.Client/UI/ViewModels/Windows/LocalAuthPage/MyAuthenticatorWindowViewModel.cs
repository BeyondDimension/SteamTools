using System;
using System.Application.Models;
using System.Collections.Generic;
using System.Text;

// ReSharper disable once CheckNamespace
namespace System.Application.UI.ViewModels
{
    public partial class MyAuthenticatorWindowViewModel : WindowViewModel, IExplicitHasValue
    {
        protected readonly MyAuthenticator? MyAuthenticator;
        protected readonly GAPAuthenticatorValueDTO.SteamAuthenticator? _Authenticator;

        public MyAuthenticatorWindowViewModel()
        {
            InitializeComponent();
        }

        public MyAuthenticatorWindowViewModel(MyAuthenticator? auth) : this()
        {
            MyAuthenticator = auth;
            if (auth?.AuthenticatorData.Value is GAPAuthenticatorValueDTO.SteamAuthenticator authenticator)
            {
                _Authenticator = authenticator;
            }
        }

        protected virtual void InitializeComponent() { }

        public virtual bool ExplicitHasValue() => MyAuthenticator != null && _Authenticator != null;
    }
}
