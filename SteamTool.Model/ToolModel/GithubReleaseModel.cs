using System;
using System.Collections.Generic;
using System.Text;

namespace SteamTool.Model.ToolModel
{
    public class GithubReleaseModel
    {
        public string id { get; set; }
        public string html_url { get; set; }
        public string tag_name { get; set; }
        public Version version => new Version(tag_name);
        public string target_commitish { get; set; }
        public string created_at { get; set; }
        public string published_at { get; set; }
        public List<Assets> assets { get; set; }
        public string body { get; set; }
    }

    public class Assets
    {
        public string id { get; set; }
        public string url { get; set; }
        public string name { get; set; }
        public string content_type { get; set; }
        public string size { get; set; }
        public string download_count { get; set; }
        public string updated_at { get; set; }
        public string browser_download_url { get; set; }
    }
}
