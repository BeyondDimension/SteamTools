using Avalonia.Media;
using WinAuth;

namespace BD.WTTS.Models;

public partial class AuthenticatorItemModel
{
    const string DefaultCode = "-----";

    public bool IsCloudAuth => AuthData.ServerId != null;

    public bool IsSteamAuthenticator => AuthData.Platform == AuthenticatorPlatform.Steam;

    [Reactive]
    public bool IsSelected { get; set; }

    [Reactive]
    public string AuthName { get; set; }

    [Reactive]
    public string? Code { get; set; } = DefaultCode;

    [Reactive]
    public double Value { get; set; }

    [Reactive]
    public bool IsShowCode { get; set; }
}