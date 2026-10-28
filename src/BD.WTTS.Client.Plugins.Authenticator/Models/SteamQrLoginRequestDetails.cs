using BD.SteamClient.Models;

namespace BD.WTTS.Models;

public sealed class SteamQrLoginRequestDetails
{
    public SteamQrLoginRequestDetails(CAuthentication_GetAuthSessionInfo_Response info, SteamLoginQrChallenge challenge)
    {
        DeviceName = Display(info.DeviceFriendlyName);
        HasDeviceName = !string.IsNullOrWhiteSpace(info.DeviceFriendlyName);
        Platform = info.PlatformType switch
        {
            EAuthTokenPlatformType.KEauthTokenPlatformTypeSteamClient => "Steam 桌面客户端",
            EAuthTokenPlatformType.KEauthTokenPlatformTypeWebBrowser => "网页浏览器",
            EAuthTokenPlatformType.KEauthTokenPlatformTypeMobileApp => "移动应用",
            _ => "Steam 未提供设备类型",
        };
        Location = string.Join(" / ", new[] { info.Country, info.State, info.City }.Where(x => !string.IsNullOrWhiteSpace(x)));
        if (Location.Length == 0) Location = "Steam 未提供位置";
        Geolocation = Display(info.Geoloc);
        HasLocation = !string.IsNullOrWhiteSpace(info.Country) || !string.IsNullOrWhiteSpace(info.State) || !string.IsNullOrWhiteSpace(info.City);
        HasGeolocation = info.HasRequestorLocationMismatch && info.RequestorLocationMismatch && !string.IsNullOrWhiteSpace(info.Geoloc);
        HasIpAddress = !string.IsNullOrWhiteSpace(info.Ip);
        HasPlatform = info.PlatformType is EAuthTokenPlatformType.KEauthTokenPlatformTypeSteamClient or
            EAuthTokenPlatformType.KEauthTokenPlatformTypeWebBrowser or EAuthTokenPlatformType.KEauthTokenPlatformTypeMobileApp;
        IpAddress = Display(info.Ip);
        LoginHistory = info.HasLoginHistory ? info.LoginHistory switch
        {
            EAuthSessionSecurityHistory.KEauthSessionSecurityHistoryUsedPreviously => "此 IP 曾成功登录此账号",
            EAuthSessionSecurityHistory.KEauthSessionSecurityHistoryNoPriorHistory => "此 IP 没有成功登录此账号的历史",
            _ => null,
        } : null;
        LocationAssessment = info.HasRequestorLocationMismatch && info.RequestorLocationMismatch
            ? "登录请求位置与当前确认位置不一致" : null;
        UsageAssessment = info.HasHighUsageLogin && info.HighUsageLogin
            ? "此登录请求近期使用频繁" : null;
        SecuritySummary = string.Join(" · ", new[]
        {
            !info.HasLoginHistory ? null : info.LoginHistory switch
            {
                EAuthSessionSecurityHistory.KEauthSessionSecurityHistoryUsedPreviously => "IP 曾登录",
                EAuthSessionSecurityHistory.KEauthSessionSecurityHistoryNoPriorHistory => "IP 无登录记录",
                _ => null,
            },
            info.HasRequestorLocationMismatch && info.RequestorLocationMismatch ? "位置不一致" : null,
            info.HasHighUsageLogin && info.HighUsageLogin ? "登录请求频繁" : null,
        }.Where(x => x != null));
        HasRequestedPersistence = info.HasRequestedPersistence && info.RequestedPersistence is
            ESessionPersistence.KEsessionPersistencePersistent or ESessionPersistence.KEsessionPersistenceEphemeral;
        RequestedPersistence = info.RequestedPersistence switch
        {
            ESessionPersistence.KEsessionPersistencePersistent => "目标设备请求保持登录",
            ESessionPersistence.KEsessionPersistenceEphemeral => "目标设备请求临时登录",
            _ => "Steam 未提供保持登录偏好",
        };
        IsRiskReported = (info.HasRequestorLocationMismatch && info.RequestorLocationMismatch) ||
            (info.HasHighUsageLogin && info.HighUsageLogin) ||
            (info.HasLoginHistory && info.LoginHistory == EAuthSessionSecurityHistory.KEauthSessionSecurityHistoryNoPriorHistory);
        RiskSummary = IsRiskReported ? "请特别核实此登录请求" : "请确认这是你本人发起的登录";
        RequestId = challenge.ClientId.ToString(CultureInfo.InvariantCulture);
        ProtocolVersion = challenge.Version.ToString(CultureInfo.InvariantCulture);
        ReadAt = DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm:ss zzz", CultureInfo.InvariantCulture);
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

    static string Display(string? value) => string.IsNullOrWhiteSpace(value) ? "Steam 未提供" : value;
}
