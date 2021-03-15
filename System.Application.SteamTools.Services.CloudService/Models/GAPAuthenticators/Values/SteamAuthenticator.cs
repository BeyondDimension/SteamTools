using MessagePack;
using MPIgnore = MessagePack.IgnoreMemberAttribute;
using N_JsonIgnore = Newtonsoft.Json.JsonIgnoreAttribute;
using S_JsonIgnore = System.Text.Json.Serialization.JsonIgnoreAttribute;

namespace System.Application.Models
{
    partial class GAPAuthenticatorValueDTO
    {
        [MessagePackObject(keyAsPropertyName: true)]
        public partial class SteamAuthenticator : GAPAuthenticatorValueDTO
        {
            /// <summary>
            /// Number of characters in code
            /// </summary>
            const int CODE_DIGITS = 5;

            /// <summary>
            /// Steam issuer for KeyUri
            /// </summary>
            const string STEAM_ISSUER = "Steam";

            /// <summary>
            /// Create a new Authenticator object
            /// </summary>
            [SerializationConstructor]
            public SteamAuthenticator() : base(CODE_DIGITS)
            {
                Issuer = STEAM_ISSUER;
            }

            [MPIgnore, N_JsonIgnore, S_JsonIgnore]
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

            protected override bool ExplicitHasValue()
            {
                return base.ExplicitHasValue() && Serial != null && DeviceId != null && SteamData != null && SessionData != null;
            }
        }
    }
}