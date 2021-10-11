using System;
using System.Application.Models.Abstractions;
using System.Application.UI.Resx;
using System.Collections.Generic;
using System.Text;

// ReSharper disable once CheckNamespace
namespace System.Application.UI.ViewModels.Abstractions
{
    public partial class ShowAuthWindowViewModel : MyAuthenticatorWindowViewModel
    {
        public ShowAuthWindowViewModel() : base()
        {

        }

        public ShowAuthWindowViewModel(MyAuthenticator? auth) : base(auth)
        {

        }

        public static string DisplayName => AppResources.LocalAuth_AuthData;

        protected override void InitializeComponent()
        {
            Title = GetTitleByDisplayName(DisplayName);
        }

        public string? SteamDataIndented => Serializable.GetIndented(_Authenticator?.SteamData, Serializable.JsonImplType.NewtonsoftJson);

        public string? RecoveryCode => _Authenticator?.RecoveryCode;

        public string? DeviceId => _Authenticator?.DeviceId;
    }
}
