using MessagePack;

namespace System.Application.Models
{
    partial class GAPAuthenticatorValueDTO
    {
        public partial class HOTPAuthenticator : GAPAuthenticatorValueDTO
        {
            /// <summary>
            /// Create a new Authenticator object optionally with a specified number of digits
            /// </summary>
            [SerializationConstructor]
            public HOTPAuthenticator() : base(DEFAULT_CODE_DIGITS)
            {
            }

            public override GamePlatform Platform => default;

            /// <summary>
            /// Counter for authenticator input
            /// </summary>
            public long Counter { get; set; }
        }
    }
}