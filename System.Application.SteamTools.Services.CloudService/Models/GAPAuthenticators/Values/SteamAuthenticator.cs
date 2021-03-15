using MessagePack;

namespace System.Application.Models
{
    partial class GAPAuthenticatorValueDTO
    {
        [MessagePackObject(keyAsPropertyName: true)]
        public partial class SteamAuthenticator : GAPAuthenticatorValueDTO
        {
            /// <summary>
            /// Create a new Authenticator object
            /// </summary>
            [SerializationConstructor]
            public SteamAuthenticator() : base(CODE_DIGITS)
            {
                Issuer = STEAM_ISSUER;
            }

            public override GamePlatform Platform => GamePlatform.Steam;
        }
    }
}