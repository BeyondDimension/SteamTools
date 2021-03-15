using MessagePack;

namespace System.Application.Models
{
    partial class GAPAuthenticatorValueDTO
    {
        [MessagePackObject(keyAsPropertyName: true)]
        public partial class SteamAuthenticator : GAPAuthenticatorValueDTO
        {
            public override GamePlatform Platform => GamePlatform.Steam;

            public override void Sync()
            {
                throw new NotImplementedException();
            }
        }
    }
}