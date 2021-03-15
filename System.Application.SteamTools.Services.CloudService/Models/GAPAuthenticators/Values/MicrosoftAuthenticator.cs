using MessagePack;
using MPIgnore = MessagePack.IgnoreMemberAttribute;
using N_JsonIgnore = Newtonsoft.Json.JsonIgnoreAttribute;
using S_JsonIgnore = System.Text.Json.Serialization.JsonIgnoreAttribute;

namespace System.Application.Models
{
    partial class GAPAuthenticatorValueDTO
    {
        [MessagePackObject(keyAsPropertyName: true)]
        public partial class MicrosoftAuthenticator : GoogleAuthenticator
        {
            [SerializationConstructor]
            public MicrosoftAuthenticator() : base()
            {
            }

            [MPIgnore, N_JsonIgnore, S_JsonIgnore]
            public override GamePlatform Platform => GamePlatform.MicrosoftStore;
        }
    }
}