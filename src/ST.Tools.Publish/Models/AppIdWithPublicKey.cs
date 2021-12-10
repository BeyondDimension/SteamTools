using MPKey = MessagePack.KeyAttribute;
using MPObject = MessagePack.MessagePackObjectAttribute;

namespace System.Application.Models
{
    [MPObject]
    public class AppIdWithPublicKey
    {
        [MPKey(0)]
        public Guid AppId { get; set; }

        [MPKey(1)]
        public string? PublicKey { get; set; }

        public const string AuthorizationToken = "AuthorizationToken";
    }
}
