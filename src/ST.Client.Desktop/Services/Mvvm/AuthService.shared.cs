using System;
using System.Application.Models;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyAuthenticatorBase = System.Application.Models.Abstractions.MyAuthenticator;

// ReSharper disable once CheckNamespace
namespace System.Application.Services
{
    public partial class AuthService : Abstractions.AuthService<AuthService>
    {
        public AuthService() { }

        protected override MyAuthenticatorBase GetMyAuthenticator(IGAPAuthenticatorDTO dto)
            => new MyAuthenticator(dto);

        protected override MyAuthenticatorBase GetMyAuthenticator()
            => new MyAuthenticator();
    }
}
