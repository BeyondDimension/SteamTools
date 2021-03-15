using MessagePack;

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

            public override GamePlatform Platform => GamePlatform.MicrosoftStore;
        }
    }
}