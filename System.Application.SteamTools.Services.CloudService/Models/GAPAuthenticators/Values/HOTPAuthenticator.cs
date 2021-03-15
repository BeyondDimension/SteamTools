namespace System.Application.Models
{
    partial class GAPAuthenticatorValueDTO
    {
        [Obsolete]
        public partial class HOTPAuthenticator : GAPAuthenticatorValueDTO
        {
            public override GamePlatform Platform => default;

            public override void Sync()
            {
                throw new NotImplementedException();
            }
        }
    }
}