using MPUnion = MessagePack.UnionAttribute;

namespace System.Application.Models
{
    [MPUnion((int)GamePlatform.Steam, typeof(SteamAuthenticatorValueDTO))]
    public interface IGameAccountPlatformAuthenticatorValueDTO
    {
        GamePlatform Platform { get; }
    }
}