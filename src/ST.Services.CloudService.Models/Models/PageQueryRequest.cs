using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MPKey = MessagePack.KeyAttribute;
using MPObj = MessagePack.MessagePackObjectAttribute;
using N_JsonProperty = Newtonsoft.Json.JsonPropertyAttribute;
using S_JsonProperty = System.Text.Json.Serialization.JsonPropertyNameAttribute;

namespace System.Application.Models
{
    /// <summary>
    /// 分页查询请求
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [MPObj]
    public class PageQueryRequest<T>
    {
        [MPKey(0)]
        [N_JsonProperty("0")]
        [S_JsonProperty("0")]
        public int Current { get; set; }

        [MPKey(1)]
        [N_JsonProperty("1")]
        [S_JsonProperty("1")]
        public int PageSize { get; set; } = 15;

        [MPKey(2)]
        [N_JsonProperty("2")]
        [S_JsonProperty("2")]
        public T? Params { get; set; }
    }
}
