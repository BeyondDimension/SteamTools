using MessagePack;

namespace System.Application.Models
{
    partial class GAPAuthenticatorValueDTO
    {
        [MessagePackObject(keyAsPropertyName: true)]
        public partial class GoogleAuthenticator : GAPAuthenticatorValueDTO
        {
            /// <summary>
            /// Create a new Authenticator object
            /// </summary>
            [SerializationConstructor]
            public GoogleAuthenticator() : base(CODE_DIGITS)
            {
            }

            public override GamePlatform Platform => GamePlatform.Google;
        }
    }
}