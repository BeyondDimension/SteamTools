using MessagePack;
using MPIgnore = MessagePack.IgnoreMemberAttribute;
using N_JsonIgnore = Newtonsoft.Json.JsonIgnoreAttribute;
using S_JsonIgnore = System.Text.Json.Serialization.JsonIgnoreAttribute;

namespace System.Application.Models
{
    partial class GAPAuthenticatorValueDTO
    {
        [MessagePackObject(keyAsPropertyName: true)]
        public partial class BattleNetAuthenticator : GAPAuthenticatorValueDTO
        {
            /// <summary>
            /// Create a new Authenticator object
            /// </summary>
            [SerializationConstructor]
            public BattleNetAuthenticator() : base(CODE_DIGITS)
            {
                Issuer = BATTLENET_ISSUER;
            }

            [MPIgnore, N_JsonIgnore, S_JsonIgnore]
            public override GamePlatform Platform => GamePlatform.BattleNet;

            public string? Serial { get; set; }

            /// <summary>
            /// We can check if the restore code is valid and rememeber so don't have to do it again
            /// </summary>
            public bool RestoreCodeVerified { get; set; }
        }
    }
}