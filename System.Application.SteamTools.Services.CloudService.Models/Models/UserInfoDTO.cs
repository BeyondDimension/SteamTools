using System.Diagnostics;
using MPKey = MessagePack.KeyAttribute;
using MPObject = MessagePack.MessagePackObjectAttribute;
using N_JsonProperty = Newtonsoft.Json.JsonPropertyAttribute;
using S_JsonProperty = System.Text.Json.Serialization.JsonPropertyNameAttribute;

namespace System.Application.Models
{
    [MPObject]
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public sealed class UserInfoDTO : IUserDTO
    {
        string DebuggerDisplay => IUserDTO.GetDebuggerDisplay(this);

        [MPKey(0)]
        [N_JsonProperty("0")]
        [S_JsonProperty("0")]
        public Guid Id { get; set; }

        [MPKey(1)]
        [N_JsonProperty("1")]
        [S_JsonProperty("1")]
        public string? NickName { get; set; }

        [MPKey(2)]
        [N_JsonProperty("2")]
        [S_JsonProperty("2")]
        public Guid? Avatar { get; set; }

        /// <summary>
        /// 经验值
        /// </summary>
        [MPKey(3)]
        [N_JsonProperty("3")]
        [S_JsonProperty("3")]
        public float Experience { get; set; }

        /// <summary>
        /// 体力
        /// </summary>
        [MPKey(4)]
        [N_JsonProperty("4")]
        [S_JsonProperty("4")]
        public int Strength { get; set; }

        /// <summary>
        /// 代币
        /// </summary>
        [MPKey(5)]
        [N_JsonProperty("5")]
        [S_JsonProperty("5")]
        public int Coin { get; set; }

        /// <summary>
        /// 余额
        /// </summary>
        [MPKey(6)]
        [N_JsonProperty("6")]
        [S_JsonProperty("6")]
        public decimal Balance { get; set; }
    }
}