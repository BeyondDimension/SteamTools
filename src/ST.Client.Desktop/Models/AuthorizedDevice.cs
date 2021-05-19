using ReactiveUI;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;
using static System.Application.SteamApiUrls;

namespace System.Application.Models
{
    public class AuthorizedDevice : ReactiveObject
    {
        public string? SteamId3 { get; set; }

        public long Timeused { get; set; }

        public string? Description { get; set; }

        public string? Tokenid { get; set; }
    }
}