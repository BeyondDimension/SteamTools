using MessagePack;

namespace System.Application.Models
{
    [MessagePackObject(keyAsPropertyName: true)]
    public sealed class SteamAuthenticatorValueDTO : GameAccountPlatformAuthenticatorValueDTO
    {
        public override GamePlatform Platform => GamePlatform.Steam;

        /// <summary>
        /// Returned serial number of authenticator
        /// </summary>
        public string? Serial { get; set; }

        /// <summary>
        /// Random device ID we created and registered
        /// </summary>
        public string? DeviceId { get; set; }

        /// <summary>
        /// JSON steam data
        /// </summary>
        public string? SteamData { get; set; }

        /// <summary>
        /// JSON session data
        /// </summary>
        public string? SessionData { get; set; }

        public string? PollData { get; set; }
    }
}