using MessagePack;
using MPIgnore = MessagePack.IgnoreMemberAttribute;
using N_JsonIgnore = Newtonsoft.Json.JsonIgnoreAttribute;
using S_JsonIgnore = System.Text.Json.Serialization.JsonIgnoreAttribute;

namespace System.Application.Models;

partial class GAPAuthenticatorValueDTO
{
    [MessagePackObject(keyAsPropertyName: true)]
    public partial class GoogleAuthenticator : GAPAuthenticatorValueDTO
    {
        /// <summary>
        /// Number of digits in code
        /// </summary>
        const int CODE_DIGITS = 6;

        /// <summary>
        /// Create a new Authenticator object
        /// </summary>
        [SerializationConstructor]
        public GoogleAuthenticator() : base(CODE_DIGITS)
        {
        }

        [MPIgnore, N_JsonIgnore, S_JsonIgnore]
        public override GamePlatform Platform => GamePlatform.Google;
    }
}