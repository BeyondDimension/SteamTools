using SteamTool.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Linq;

namespace SteamTool.Proxy
{
    public class ProxyScript
    {
        /// <summary>
        /// 脚本文件路径
        /// </summary>
        public string FilePath { get; set; }
        /// <summary>
        /// 脚本文件名称
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// namespace
        /// </summary>
        public List<string> NameSpace { get; set; }

        /// <summary>
        /// Version
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// author
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        /// author
        /// </summary>
        public List<string> Match { get; set; }

        /// <summary>
        /// include
        /// </summary>
        public List<string> Include { get; set; }

        /// <summary>
        /// exclude
        /// </summary>
        public List<string> Exclude { get; set; }
        public List<string> Grant { get; set; }

        public bool IsEnable { get; set; }

        public string Content { get; set; }

        private const string DescRegex = @"(?<={0})[\s\S]*?(?=\n)";

        public static bool TryParse(string path,out ProxyScript proxyScript)
        {
            var content = File.ReadAllText(path);
            if (!string.IsNullOrEmpty(content))
            {
                var userScript = content.Substring("==UserScript==", "==/UserScript==");
                if (!string.IsNullOrEmpty(userScript))
                {
                    var script = new ProxyScript
                    {
                        FilePath = path,
                        Content = content.Replace("</script>", "<\\/script>"),
                        //Content = content.Replace("</script>", "<\\/script>").Replace(" ", "").Replace("\r", "").Replace("\n", "").Replace("\t", ""),
                        FileName = System.IO.Path.GetFileName(path),
                        Name = Regex.Match(userScript, string.Format(DescRegex, $"@{nameof(Name).ToLower()}")).GetValue(s => s.Success == true),
                        NameSpace = Regex.Matches(userScript, string.Format(DescRegex, $"@{nameof(NameSpace).ToLower()}")).GetValues(s => s.Success == true).ToList(),
                        Version = Regex.Match(userScript, string.Format(DescRegex, $"@{nameof(Version).ToLower()}")).GetValue(s => s.Success == true),
                        Description = Regex.Match(userScript, string.Format(DescRegex, $"@{nameof(Description).ToLower()}")).GetValue(s => s.Success == true),
                        Author = Regex.Match(userScript, string.Format(DescRegex, $"@{nameof(Author).ToLower()}")).GetValue(s => s.Success == true),
                        Exclude = Regex.Matches(userScript, string.Format(DescRegex, $"@{nameof(Exclude).ToLower()}")).GetValues(s => s.Success == true).ToList(),
                        Grant = Regex.Matches(userScript, string.Format(DescRegex, $"@{nameof(Grant).ToLower()}")).GetValues(s => s.Success == true).ToList()
                    };
                    var matchs = Regex.Matches(userScript, string.Format(DescRegex, $"@{nameof(Match).ToLower()}")).GetValues(s => s.Success == true).ToList();
                    var Includes = Regex.Matches(userScript, string.Format(DescRegex, $"@{nameof(Include).ToLower()}")).GetValues(s => s.Success == true).ToList();
                    script.Match = matchs.Count == 0 ? Includes : matchs;
                    proxyScript = script;
                    return true;
                }
            }
            proxyScript = null;
            return false;
        }
    }
}
