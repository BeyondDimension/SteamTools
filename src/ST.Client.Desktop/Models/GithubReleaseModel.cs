using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Application.Models
{
    [Obsolete("use https://github.com/octokit/octokit.net")]
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

        public class Assets
        {
            public string id { get; set; }
            public string url { get; set; }
            public string name { get; set; }
            public string content_type { get; set; }
            public long size { get; set; }
            public string download_count { get; set; }
            public string updated_at { get; set; }
            public string browser_download_url { get; set; }
        }
    }
}
