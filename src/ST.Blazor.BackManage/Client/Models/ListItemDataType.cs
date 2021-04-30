using System;
using System.Text.Json.Serialization;
using System.Application.Utils;

namespace System.Application.Models
{
    public class Member
    {
        public string Avatar { get; set; }
        public string Name { get; set; }
        public string Id { get; set; }
    }

    public class ListItemDataType
    {
        public string Id { get; set; }
        public string Owner { get; set; }
        public string Title { get; set; }
        public string Avatar { get; set; }
        public string Cover { get; set; }
        public string Status { get; set; }
        public int Percent { get; set; }
        public string Logo { get; set; }
        public string Href { get; set; }
        public string Body { get; set; }
        public string SubDescription { get; set; }
        public string Description { get; set; }
        public int ActiveUser { get; set; }
        public int NewUser { get; set; }
        public int Star { get; set; }
        public int Like { get; set; }
        public int Message { get; set; }
        public string Content { get; set; }
        public Member[] Members { get; set; }

        [JsonConverter(typeof(LongToDateTimeConverter))]
        public DateTime UpdatedAt { get; set; }

        [JsonConverter(typeof(LongToDateTimeConverter))]
        public DateTime CreatedAt { get; set; }
    }
}