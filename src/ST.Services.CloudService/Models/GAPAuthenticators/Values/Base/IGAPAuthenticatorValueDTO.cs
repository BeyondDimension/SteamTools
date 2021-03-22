using static System.Application.Models.GAPAuthenticatorValueDTO;
using MPUnion = MessagePack.UnionAttribute;

namespace System.Application.Models
{
    [MPUnion((int)GamePlatform.BattleNet, typeof(BattleNetAuthenticator))]
    [MPUnion((int)GamePlatform.Google, typeof(GoogleAuthenticator))]
    [MPUnion((int)GamePlatform.MicrosoftStore, typeof(MicrosoftAuthenticator))]
    [MPUnion((int)GamePlatform.Steam, typeof(SteamAuthenticator))]
    public partial interface IGAPAuthenticatorValueDTO : IExplicitHasValue
    {
        GamePlatform Platform { get; }

        /// <summary>
        /// 本地机器和服务器的时间差（毫秒）
        /// </summary>
        long ServerTimeDiff { get; set; }

        /// <summary>
        /// 上次同步时间
        /// </summary>
        long LastServerTime { get; set; }

        /// <summary>
        /// 用于身份验证器的密钥
        /// </summary>
        byte[]? SecretKey { get; set; }

        /// <summary>
        /// 代码中返回的位数（默认为6）
        /// </summary>
        int CodeDigits { get; set; }

        /// <summary>
        /// 用于OTP生成的哈希算法（默认为SHA1）
        /// </summary>
        HMACTypes HMACType { get; set; }

        /// <summary>
        /// 下一个代码的周期（秒）
        /// </summary>
        int Period { get; set; }
    }
}