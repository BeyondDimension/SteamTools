using System.IO;
using System.Threading.Tasks;
using N_JsonIgnore = Newtonsoft.Json.JsonIgnoreAttribute;
using N_JsonProperty = Newtonsoft.Json.JsonPropertyAttribute;
using S_JsonIgnore = System.Text.Json.Serialization.JsonIgnoreAttribute;
using S_JsonProperty = System.Text.Json.Serialization.JsonPropertyNameAttribute;

namespace System.Application.Models
{
    public class SteamMiniProfile
    {
        public class Badge
        {
            [N_JsonProperty("name")]
            [S_JsonProperty("name")]
            public string? Name { get; set; }

            /// <summary>
            /// 经验值
            /// </summary>
            [N_JsonProperty("xp")]
            [S_JsonProperty("xp")]
            public string? Xp { get; set; }

            /// <summary>
            /// 等级
            /// </summary>
            [N_JsonProperty("level")]
            [S_JsonProperty("level")]
            public int Level { get; set; }

            /// <summary>
            /// 说明
            /// </summary>
            [N_JsonProperty("description")]
            [S_JsonProperty("description")]
            public string? Description { get; set; }

            /// <summary>
            /// 徽章图标
            /// </summary>
            [N_JsonProperty("icon")]
            [S_JsonProperty("icon")]
            public string? Icon { get; set; }
        }

        public class Game
        {
            [N_JsonProperty("name")]
            [S_JsonProperty("name")]
            public string? Name { get; set; }

            /// <summary>
            /// 非Steam游戏
            /// </summary>
            [N_JsonProperty("is_non_steam")]
            [S_JsonProperty("is_non_steam")]
            public bool isNonSteam { get; set; }

            /// <summary>
            /// 游戏LOGO
            /// </summary>
            [N_JsonProperty("logo")]
            [S_JsonProperty("logo")]
            public string? Logo { get; set; }
        }

        public class ProfileBackground
        {
            [N_JsonProperty("video/webm")]
            [S_JsonProperty("video/webm")]
            public string? VideoWebm { get; set; }

            [N_JsonProperty("video/mp4")]
            [S_JsonProperty("video/mp4")]
            public string? VideoMp4 { get; set; }
        }

        /// <summary>
        /// Steam等级
        /// </summary>
        [N_JsonProperty("level")]
        [S_JsonProperty("level")]
        public int Level { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [N_JsonProperty("level_class")]
        [S_JsonProperty("level_class")]
        public string? LevelClass { get; set; }

        /// <summary>
        /// 静态头像链接
        /// </summary>
        [N_JsonProperty("avatar_url")]
        [S_JsonProperty("avatar_url")]
        public string? AvatarUrl { get; set; }

        /// <summary>
        /// Steam昵称
        /// </summary>
        [N_JsonProperty("persona_name")]
        [S_JsonProperty("persona_name")]
        public string? PersonaName { get; set; }

        /// <summary>
        /// 收藏的徽章
        /// </summary>
        [N_JsonProperty("favorite_badge")]
        [S_JsonProperty("favorite_badge")]
        public Badge? FavoriteBadge { get; set; }

        [N_JsonProperty("in_game")]
        [S_JsonProperty("in_game")]
        public Game? InGame { get; set; }

        [N_JsonProperty("profile_background")]
        [S_JsonProperty("profile_background")]
        public ProfileBackground? Background { get; set; }

        /// <summary>
        /// 头像框uri
        /// </summary>
        [N_JsonProperty("avatar_frame")]
        [S_JsonProperty("avatar_frame")]
        public string? AvatarFrame { get; set; }

        [N_JsonIgnore]
        [S_JsonIgnore]
        public Task<string?>? AvatarFrameStream { get; set; }

        /// <summary>
        /// 动态头像url
        /// </summary>
        [N_JsonProperty("animated_avatar")]
        [S_JsonProperty("animated_avatar")]
        public string? AnimatedAvatar { get; set; }

        [N_JsonIgnore]
        [S_JsonIgnore]
        public Task<string?>? AnimatedAvatarStream { get; set; }
    }
}
