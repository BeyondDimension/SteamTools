using BD.SteamClient.Models;

namespace BD.WTTS.Models;

public sealed class SteamQrLoginRequestDetails
{
    public SteamQrLoginRequestDetails(CAuthentication_GetAuthSessionInfo_Response info, SteamLoginQrChallenge challenge, string? readAt = null)
    {
        DeviceName = Display(info.DeviceFriendlyName);
        HasDeviceName = !string.IsNullOrWhiteSpace(info.DeviceFriendlyName);
        Platform = info.PlatformType switch
        {
            EAuthTokenPlatformType.KEauthTokenPlatformTypeSteamClient => Strings.L10nAuthIdle_Text010,
            EAuthTokenPlatformType.KEauthTokenPlatformTypeWebBrowser => Strings.L10nAuthIdle_Text011,
            EAuthTokenPlatformType.KEauthTokenPlatformTypeMobileApp => Strings.L10nAuthIdle_Text012,
            _ => Strings.L10nAuthIdle_Text013,
        };
        Location = string.Join(" / ", new[] { info.Country, info.State, info.City }.Where(x => !string.IsNullOrWhiteSpace(x)));
        if (Location.Length == 0) Location = Strings.L10nAuthIdle_Text014;
        Geolocation = Display(info.Geoloc);
        HasLocation = !string.IsNullOrWhiteSpace(info.Country) || !string.IsNullOrWhiteSpace(info.State) || !string.IsNullOrWhiteSpace(info.City);
        HasGeolocation = info.HasRequestorLocationMismatch && info.RequestorLocationMismatch && !string.IsNullOrWhiteSpace(info.Geoloc);
        HasIpAddress = !string.IsNullOrWhiteSpace(info.Ip);
        HasPlatform = info.PlatformType is EAuthTokenPlatformType.KEauthTokenPlatformTypeSteamClient or
            EAuthTokenPlatformType.KEauthTokenPlatformTypeWebBrowser or EAuthTokenPlatformType.KEauthTokenPlatformTypeMobileApp;
        IpAddress = Display(info.Ip);
        LoginHistory = info.HasLoginHistory ? info.LoginHistory switch
        {
            EAuthSessionSecurityHistory.KEauthSessionSecurityHistoryUsedPreviously => Strings.L10nAuthIdle_Text015,
            EAuthSessionSecurityHistory.KEauthSessionSecurityHistoryNoPriorHistory => Strings.L10nAuthIdle_Text016,
            _ => null,
        } : null;
        LocationAssessment = info.HasRequestorLocationMismatch && info.RequestorLocationMismatch
            ? Strings.L10nAuthIdle_Text017 : null;
        UsageAssessment = info.HasHighUsageLogin && info.HighUsageLogin
            ? Strings.L10nAuthIdle_Text018 : null;
        SecuritySummary = string.Join(" · ", new[]
        {
            !info.HasLoginHistory ? null : info.LoginHistory switch
            {
                EAuthSessionSecurityHistory.KEauthSessionSecurityHistoryUsedPreviously => Strings.L10nAuthIdle_Text019,
                EAuthSessionSecurityHistory.KEauthSessionSecurityHistoryNoPriorHistory => Strings.L10nAuthIdle_Text020,
                _ => null,
            },
            info.HasRequestorLocationMismatch && info.RequestorLocationMismatch ? Strings.L10nAuthIdle_Text021 : null,
            info.HasHighUsageLogin && info.HighUsageLogin ? Strings.L10nAuthIdle_Text022 : null,
        }.Where(x => x != null));
        HasRequestedPersistence = info.HasRequestedPersistence && info.RequestedPersistence is
            ESessionPersistence.KEsessionPersistencePersistent or ESessionPersistence.KEsessionPersistenceEphemeral;
        RequestedPersistence = info.RequestedPersistence switch
        {
            ESessionPersistence.KEsessionPersistencePersistent => Strings.L10nAuthIdle_Text023,
            ESessionPersistence.KEsessionPersistenceEphemeral => Strings.L10nAuthIdle_Text024,
            _ => Strings.L10nAuthIdle_Text025,
        };
        IsRiskReported = (info.HasRequestorLocationMismatch && info.RequestorLocationMismatch) ||
            (info.HasHighUsageLogin && info.HighUsageLogin) ||
            (info.HasLoginHistory && info.LoginHistory == EAuthSessionSecurityHistory.KEauthSessionSecurityHistoryNoPriorHistory);
        RiskSummary = IsRiskReported ? Strings.L10nAuthIdle_Text026 : Strings.L10nAuthIdle_Text027;
        RequestId = challenge.ClientId.ToString(CultureInfo.InvariantCulture);
        ProtocolVersion = challenge.Version.ToString(CultureInfo.InvariantCulture);
        ReadAt = readAt ?? DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm:ss zzz", CultureInfo.InvariantCulture);
    }

    public string DeviceName { get; }
    public bool HasDeviceName { get; }
    public string Platform { get; }
    public string Location { get; }
    public string Geolocation { get; }
    public string IpAddress { get; }
    public bool HasLocation { get; }
    public bool HasGeolocation { get; }
    public bool HasIpAddress { get; }
    public bool HasPlatform { get; }
    public bool HasRequestedPersistence { get; }
    public string? LoginHistory { get; }
    public string? LocationAssessment { get; }
    public string? UsageAssessment { get; }
    public bool HasLoginHistory => LoginHistory != null;
    public bool HasLocationAssessment => LocationAssessment != null;
    public bool HasUsageAssessment => UsageAssessment != null;
    public bool HasSecuritySummary => SecuritySummary.Length != 0;
    public string RequestedPersistence { get; }
    public bool IsRiskReported { get; }
    public string RiskSummary { get; }
    public string SecuritySummary { get; }
    public string RequestId { get; }
    public string ProtocolVersion { get; }
    public string ReadAt { get; }

    static string Display(string? value) => string.IsNullOrWhiteSpace(value) ? Strings.L10nAuthIdle_Text028 : value;
}
