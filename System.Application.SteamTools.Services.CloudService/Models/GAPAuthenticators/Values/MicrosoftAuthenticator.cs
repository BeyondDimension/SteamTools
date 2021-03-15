using MessagePack;

namespace System.Application.Models
{
    partial class GAPAuthenticatorValueDTO
    {
        [MessagePackObject(keyAsPropertyName: true)]
        public partial class MicrosoftAuthenticator : GAPAuthenticatorValueDTO
        {
            public override GamePlatform Platform => GamePlatform.MicrosoftStore;

            public override void Sync()
            {
                throw new NotImplementedException();
            }
        }
    }
}