using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Application.Models
{
    public class ASFGlobalConfig
    {
        [JsonProperty(Required = Required.DisallowNull)]
        public bool IPC { get; set; }

        [JsonProperty]
        public string? IPCPassword { get; set; }

        [JsonProperty(Required = Required.DisallowNull)]
        public ulong SteamOwnerID { get; set; }
    }
}
