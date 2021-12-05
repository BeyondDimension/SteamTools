using MPKey = MessagePack.KeyAttribute;
using MPObj = MessagePack.MessagePackObjectAttribute;
using N_JsonProperty = Newtonsoft.Json.JsonPropertyAttribute;
using S_JsonProperty = System.Text.Json.Serialization.JsonPropertyNameAttribute;

namespace System.Application.Models
{
    [MPObj]
    public class SyncAuthResponse
    {
        [MPKey(0)]
        [N_JsonProperty("0")]
        [S_JsonProperty("0")]
        public Guid Id { get; set; }

        [MPKey(1)]
        [N_JsonProperty("1")]
        [S_JsonProperty("1")]
        public DateTimeOffset LastSyncTime { get; set; }

        [MPKey(2)]
        [N_JsonProperty("2")]
        [S_JsonProperty("2")]
        public string Name { get; set; } = string.Empty;

        [MPKey(3)]
        [N_JsonProperty("3")]
        [S_JsonProperty("3")]
        public int Order { get; set; }
    }

    [MPObj]
    public class SyncAuthTokenResponse
    {
        [MPKey(0)]
        [N_JsonProperty("0")]
        [S_JsonProperty("0")]
        public Guid Id { get; set; }

        [MPKey(1)]
        [N_JsonProperty("1")]
        [S_JsonProperty("1")]
        public byte? Token { get; set; }
    }
}
