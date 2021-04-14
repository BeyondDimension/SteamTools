using System.Application.Columns;
using System.Diagnostics;
using MPIgnore = MessagePack.IgnoreMemberAttribute;
using MPKey = MessagePack.KeyAttribute;
using MPObject = MessagePack.MessagePackObjectAttribute;
using N_JsonIgnore = Newtonsoft.Json.JsonIgnoreAttribute;
using N_JsonProperty = Newtonsoft.Json.JsonPropertyAttribute;
using S_JsonIgnore = System.Text.Json.Serialization.JsonIgnoreAttribute;
using S_JsonProperty = System.Text.Json.Serialization.JsonPropertyNameAttribute;

namespace System.Application.Models
{
    [MPObject]
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public sealed class UserInfoDTO : IUserDTO, IBirthDateTimeZone
    {
        string DebuggerDisplay => IUserDTO.GetDebuggerDisplay(this);

        [MPKey(0)]
        [N_JsonProperty("0")]
        [S_JsonProperty("0")]
        public Guid Id { get; set; }

        [MPKey(1)]
        [N_JsonProperty("1")]
        [S_JsonProperty("1")]
        public string NickName { get; set; } = string.Empty;

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
        public uint Experience { get; set; }

        /// <summary>
        /// 机油、体力、疲劳值、积分1
        /// </summary>
        [MPKey(4)]
        [N_JsonProperty("4")]
        [S_JsonProperty("4")]
        public int EngineOil { get; set; }

        /// <summary>
        /// 代币、硬币、积分2
        /// </summary>
        [MPKey(5)]
        [N_JsonProperty("5")]
        [S_JsonProperty("5")]
        public decimal Coin { get; set; }

        /// <summary>
        /// 余额
        /// </summary>
        [MPKey(6)]
        [N_JsonProperty("6")]
        [S_JsonProperty("6")]
        public decimal Balance { get; set; }

        /// <summary>
        /// 账号等级
        /// </summary>
        [MPKey(7)]
        [N_JsonProperty("7")]
        [S_JsonProperty("7")]
        public byte Level { get; set; }

        [MPKey(8)]
        [N_JsonProperty("8")]
        [S_JsonProperty("8")]
        public long? SteamAccountId { get; set; }

        [MPKey(9)]
        [N_JsonProperty("9")]
        [S_JsonProperty("9")]
        public Gender Gender { get; set; }

        #region BirthDate(Only the current login user has value)

        [MPKey(10)]
        [N_JsonProperty("10")]
        [S_JsonProperty("10")]
        public DateTime? BirthDate { get; set; }

        [MPKey(11)]
        [N_JsonProperty("11")]
        [S_JsonProperty("11")]
        public sbyte BirthDateTimeZone { get; set; }

        #endregion

        [MPKey(12)]
        [N_JsonProperty("12")]
        [S_JsonProperty("12")]
        public byte? CalcAge { get; set; }

        [MPIgnore]
        [N_JsonIgnore]
        [S_JsonIgnore]
        public string Age
        {
            get
            {
                byte value;
                if (CalcAge.HasValue) value = CalcAge.Value;
                else value = BirthDateHelper.CalcAge(this.GetBirthDate()?.DateTime);
                return BirthDateHelper.ToString(value);
            }
        }

        /// <summary>
        /// 所在地
        /// </summary>
        [MPKey(13)]
        [N_JsonProperty("13")]
        [S_JsonProperty("13")]
        public int? AreaId { get; set; }
    }
}