using N_JsonProperty = Newtonsoft.Json.JsonPropertyAttribute;
using S_JsonProperty = System.Text.Json.Serialization.JsonPropertyNameAttribute;

namespace System.Application.Models
{
    public class SteamMiniProfile
    {
        public class FavoriteBadge
        {
            public string? name { get; set; }
            public string? xp { get; set; }
            public int level { get; set; }
            public string? description { get; set; }
            public string? icon { get; set; }
        }

        public class InGame
        {
            public string? name { get; set; }
            public bool is_non_steam { get; set; }
            public string? logo { get; set; }
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

        public int level { get; set; }
        public string? level_class { get; set; }
        public string? avatar_url { get; set; }
        public string? persona_name { get; set; }
        public FavoriteBadge? favorite_badge { get; set; }
        public InGame? in_game { get; set; }
        public ProfileBackground? profile_background { get; set; }
        public string? avatar_frame { get; set; }
        public string? animated_avatar { get; set; }
    }
}
