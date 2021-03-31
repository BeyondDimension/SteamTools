using System.Collections.Generic;
using System.IO.FileFormats;
using System.Linq;
using MPIgnore = MessagePack.IgnoreMemberAttribute;
using MPKey = MessagePack.KeyAttribute;
using MPObject = MessagePack.MessagePackObjectAttribute;

namespace System.Application.Models
{
    [MPObject]
    public class SteamMiniProfile
    {
        [MPKey(0)]
        public List<Nameplate_> Nameplate { get; set; } = new();

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

            /// <summary>
            /// 是否有头像边框
            /// </summary>
            [MPIgnore]
            public bool HasAvatarFrame => !string.IsNullOrWhiteSpace(AvatarFrame);

            [MPKey(1)]
            public string Avatar { get; set; } = string.Empty;

            /// <summary>
            /// 是否为中等大小头像，64px
            /// </summary>
            [MPIgnore]
            public bool IsAvatarMedium
            {
                get
                {
                    var value = Avatar.Split('.', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
                    if (value != null && value.EndsWith("_medium", StringComparison.OrdinalIgnoreCase)) return true;
                    return false;
                }
            }

            [MPKey(2)]
            public string Persona { get; set; } = string.Empty;

            [MPKey(3)]
            public string FriendStatus { get; set; } = string.Empty;
        }

        [MPObject]
        public class Detailssection_
        {
            [MPKey(0)]
            public string Badge { get; set; } = string.Empty;

            /// <summary>
            /// 是否有徽章
            /// </summary>
            [MPIgnore]
            public bool HasBadge => !string.IsNullOrWhiteSpace(Badge) &&
                !string.IsNullOrWhiteSpace(BadgeName) &&
                !string.IsNullOrWhiteSpace(BadgeXp);

            [MPKey(1)]
            public string BadgeName { get; set; } = string.Empty;

            [MPKey(2)]
            public string BadgeXp { get; set; } = string.Empty;

            [MPKey(3)]
            public ushort PlayerLevel { get; set; }
        }
    }
}