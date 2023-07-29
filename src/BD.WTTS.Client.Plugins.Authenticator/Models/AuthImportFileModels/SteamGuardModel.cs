namespace BD.WTTS.Models;

public class SteamGuardModel
{
    [JsonPropertyName("shared_secret")]
    public string SharedSecret { get; set; } = string.Empty;

    [JsonPropertyName("serial_number")]
    public string SerialNumber { get; set; } = string.Empty;
}