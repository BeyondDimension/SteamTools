using MessagePack;

namespace System.Application.Models
{
    partial class GAPAuthenticatorValueDTO
    {
        [MessagePackObject(keyAsPropertyName: true)]
        public partial class GoogleAuthenticator : GAPAuthenticatorValueDTO
        {
            public override GamePlatform Platform => GamePlatform.Google;

            public override void Sync()
            {
                throw new NotImplementedException();
            }
        }
    }
}