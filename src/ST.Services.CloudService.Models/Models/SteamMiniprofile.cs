using System.IO.FileFormats;
using MPKey = MessagePack.KeyAttribute;
using MPObject = MessagePack.MessagePackObjectAttribute;

namespace System.Application.Models
{
    [MPObject]
    public class SteamMiniProfile
    {
        [MPKey(0)]
        public Nameplate_[] Nameplate { get; set; } = Array.Empty<Nameplate_>();

        [MPKey(1)]
        public PlayerSection_ PlayerSection { get; set; } = new();

        [MPKey(2)]
        public Detailssection_ Detailssection { get; set; } = new();

        [MPObject]
        public class Nameplate_
        {
            [MPKey(0)]
            public VideoFormat Format { get; set; }

            [MPKey(1)]
            public string Src { get; set; } = string.Empty;
        }

        [MPObject]
        public class PlayerSection_
        {
            [MPKey(0)]
            public string AvatarFrame { get; set; } = string.Empty;

            [MPKey(1)]
            public string Avatar { get; set; } = string.Empty;

            [MPKey(2)]
            public string PersonaOnline { get; set; } = string.Empty;

            [MPKey(3)]
            public string FriendStatusOnline { get; set; } = string.Empty;
        }

        [MPObject]
        public class Detailssection_
        {
            [MPKey(0)]
            public string Badge { get; set; } = string.Empty;

            [MPKey(1)]
            public string BadgeName { get; set; } = string.Empty;

            [MPKey(2)]
            public string BadgeXp { get; set; } = string.Empty;

            [MPKey(3)]
            public ushort PlayerLevel { get; set; }
        }
    }
}