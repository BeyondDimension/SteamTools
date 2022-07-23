using System.Application.Models;
using System.Collections.Generic;
using System.Text;

// ReSharper disable once CheckNamespace
namespace System.Application.UI.ViewModels;

public partial class MyAuthenticatorWindowViewModel : WindowViewModel, IExplicitHasValue, IMyAuthenticatorWrapper
{
    protected readonly MyAuthenticator? _MyAuthenticator;
    protected readonly GAPAuthenticatorValueDTO.SteamAuthenticator? _Authenticator;

    public MyAuthenticator? MyAuthenticator => _MyAuthenticator;

    public MyAuthenticatorWindowViewModel()
    {
        InitializeComponent();
    }

    public MyAuthenticatorWindowViewModel(MyAuthenticator? auth)
    {
        _MyAuthenticator = auth;
        if (auth?.AuthenticatorData.Value is GAPAuthenticatorValueDTO.SteamAuthenticator authenticator)
        {
            _Authenticator = authenticator;
        }

        InitializeComponent();
    }

    protected virtual void InitializeComponent() { }

    public virtual bool ExplicitHasValue()
        => _MyAuthenticator != null &&
            (_MyAuthenticator.AuthenticatorData.Platform != GamePlatform.Steam
                || _Authenticator != null);
}

public interface IMyAuthenticatorWrapper
{
    MyAuthenticator? MyAuthenticator { get; }
}

public sealed class MyAuthenticatorWrapper : WindowViewModel, IMyAuthenticatorWrapper
{
    public MyAuthenticator? MyAuthenticator { get; }

    public MyAuthenticatorWrapper(MyAuthenticator? auth) => MyAuthenticator = auth;
}
