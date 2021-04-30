using MessagePack;
using MPIgnore = MessagePack.IgnoreMemberAttribute;
using N_JsonIgnore = Newtonsoft.Json.JsonIgnoreAttribute;
using S_JsonIgnore = System.Text.Json.Serialization.JsonIgnoreAttribute;

namespace System.Application.Models
{
    partial class GAPAuthenticatorValueDTO
    {
        [MessagePackObject(keyAsPropertyName: true)]
        public partial class HOTPAuthenticator : GAPAuthenticatorValueDTO
        {
            /// <summary>
            /// Create a new Authenticator object optionally with a specified number of digits
            /// </summary>
            [SerializationConstructor]
            public HOTPAuthenticator() : base(DEFAULT_CODE_DIGITS)
            {
            }

            [MPIgnore, N_JsonIgnore, S_JsonIgnore]
            public override GamePlatform Platform => default;

            /// <summary>
            /// Counter for authenticator input
            /// </summary>
            public long Counter { get; set; }
        }
    }
}