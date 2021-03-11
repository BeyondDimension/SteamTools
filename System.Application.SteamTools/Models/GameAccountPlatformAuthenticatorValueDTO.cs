using MPIgnore = MessagePack.IgnoreMemberAttribute;
using N_JsonIgnore = Newtonsoft.Json.JsonIgnoreAttribute;
using S_JsonIgnore = System.Text.Json.Serialization.JsonIgnoreAttribute;

namespace System.Application.Models
{
    public abstract class GameAccountPlatformAuthenticatorValueDTO : IGameAccountPlatformAuthenticatorValueDTO
    {
        [MPIgnore, N_JsonIgnore, S_JsonIgnore]
        public abstract GamePlatform Platform { get; }

        /// <summary>
        /// Secret key used for Authenticator
        /// </summary>
        public byte[]? SecretKey { get; set; }

        /// <summary>
        /// Time difference in milliseconds of our machine and server
        /// </summary>
        public long ServerTimeDiff { get; set; }

        /// <summary>
        /// Time of last synced
        /// </summary>
        public long LastServerTime { get; set; }
    }
}