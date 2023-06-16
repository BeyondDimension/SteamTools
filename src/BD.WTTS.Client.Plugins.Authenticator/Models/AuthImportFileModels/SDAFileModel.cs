namespace BD.WTTS.Models;

public class SdaFileModel
{
    [JsonPropertyName("shared_secret")] 
    public string SharedSecret { get; set; } = string.Empty;

    [JsonPropertyName("serial_number")] 
    public string SerialNumber { get; set; } = string.Empty;
    
    [JsonPropertyName("revocation_code")]
    public string RevocationCode { get; set; } = string.Empty;
    
    [JsonPropertyName("uri")]
    public string Uri { get; set; } = string.Empty;
    
    [JsonPropertyName("server_time")]
    public int ServerTime { get; set; }
    
    [JsonPropertyName("account_name")]
    public string AccountName { get; set; } = string.Empty;
    
    [JsonPropertyName("token_gid")]
    public string TokenGid { get; set; } = string.Empty;
    
    [JsonPropertyName("identity_secret")]
    public string IdentitySecret { get; set; } = string.Empty;
    
    [JsonPropertyName("secret_1")]
    public string Secret1 { get; set; } = string.Empty;
    
    [JsonPropertyName("status")]
    public int Status { get; set; }
    
    [JsonIgnore]
    [JsonPropertyName("device_id")]
    public string DeviceId { get; set; } = string.Empty;
    
    [JsonIgnore]
    [JsonPropertyName("fully_enrolled")]
    public bool FullyEnrolled { get; set; }
    
    [JsonIgnore]
    [JsonPropertyName("Session")]
    public Session? Session { get; set; }
}

public class Session
{
    [JsonPropertyName("SessionID")]
    public string SessionId { get; set; } = string.Empty;
    
    [JsonPropertyName("SteamLogin")]
    public string SteamLogin { get; set; } = string.Empty;
    
    [JsonPropertyName("SteamLoginSecure")]
    public string SteamLoginSecure { get; set; } = string.Empty;
    
    [JsonPropertyName("WebCookie")]
    public string WebCookie { get; set; } = string.Empty;
    
    [JsonPropertyName("OAuthToken")]
    public string OAuthToken { get; set; } = string.Empty;
    
    [JsonPropertyName("SteamID")]
    public long SteamId { get; set; }
}


