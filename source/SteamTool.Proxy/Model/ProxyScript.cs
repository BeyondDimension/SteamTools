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
        public string[] NameSpace { get; set; }

        /// <summary>
        /// Version
        /// </summary>
        public string Version { get; set; }
        public string HomepageURL { get; set; }
        public string SupportURL { get; set; }
        public string DownloadURL { get; set; }
        public string UpdateURL { get; set; }

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
        public string[] Match { get; set; }

        /// <summary>
        /// include
        /// </summary>
        public string[] Include { get; set; }

        /// <summary>
        /// exclude
        /// </summary>
        public string[] Exclude { get; set; }
        public string[] Grant { get; set; }
        public string[] Require { get; set; }

        public bool Enable { get; set; }

        public string Content { get; set; }

        private const string DescRegex = @"(?<={0})[\s\S]*?(?=\n)";

        public static bool TryParse(string path, out ProxyScript proxyScript)
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
                        Content = RemoveComment(content.Replace("</script>", "<\\/script>")).Replace("\t", ""),
                        //Content = content.Replace("</script>", "<\\/script>").Replace(" ", "").Replace("\r", "").Replace("\n", "").Replace("\t", ""),
                        FileName = System.IO.Path.GetFileName(path),
                        Name = Regex.Match(userScript, string.Format(DescRegex, $"@{nameof(Name)}"), RegexOptions.IgnoreCase).GetValue(s => s.Success == true),
                        NameSpace = Regex.Matches(userScript, string.Format(DescRegex, $"@{nameof(NameSpace)}"), RegexOptions.IgnoreCase).GetValues(s => s.Success == true).ToArray(),
                        Version = Regex.Match(userScript, string.Format(DescRegex, $"@{nameof(Version)}"), RegexOptions.IgnoreCase).GetValue(s => s.Success == true),
                        Description = Regex.Match(userScript, string.Format(DescRegex, $"@{nameof(Description)}"), RegexOptions.IgnoreCase).GetValue(s => s.Success == true),
                        Author = Regex.Match(userScript, string.Format(DescRegex, $"@{nameof(Author)}"), RegexOptions.IgnoreCase).GetValue(s => s.Success == true),
                        HomepageURL = Regex.Match(userScript, string.Format(DescRegex, $"@{nameof(HomepageURL)}"), RegexOptions.IgnoreCase).GetValue(s => s.Success == true),
                        SupportURL = Regex.Match(userScript, string.Format(DescRegex, $"@{nameof(SupportURL)}"), RegexOptions.IgnoreCase).GetValue(s => s.Success == true),
                        DownloadURL = Regex.Match(userScript, string.Format(DescRegex, $"@{nameof(DownloadURL)}"), RegexOptions.IgnoreCase).GetValue(s => s.Success == true),
                        UpdateURL = Regex.Match(userScript, string.Format(DescRegex, $"@{nameof(UpdateURL)}"), RegexOptions.IgnoreCase).GetValue(s => s.Success == true),
                        Exclude = Regex.Matches(userScript, string.Format(DescRegex, $"@{nameof(Exclude)}"), RegexOptions.IgnoreCase).GetValues(s => s.Success == true).ToArray(),
                        Grant = Regex.Matches(userScript, string.Format(DescRegex, $"@{nameof(Grant)}"), RegexOptions.IgnoreCase).GetValues(s => s.Success == true).ToArray(),
                        Require = Regex.Matches(userScript, string.Format(DescRegex, $"@{nameof(Require)}"), RegexOptions.IgnoreCase).GetValues(s => s.Success == true).ToArray()
                    };
                    var matchs = Regex.Matches(userScript, string.Format(DescRegex, $"@{nameof(Match)}"), RegexOptions.IgnoreCase).GetValues(s => s.Success == true).ToArray();
                    var Includes = Regex.Matches(userScript, string.Format(DescRegex, $"@{nameof(Include)}"), RegexOptions.IgnoreCase).GetValues(s => s.Success == true).ToArray();
                    script.Match = matchs.Length == 0 ? Includes : matchs;
                    var enable = Regex.Match(userScript, string.Format(DescRegex, $"@{nameof(Enable)}"), RegexOptions.IgnoreCase).GetValue(s => s.Success == true);

                    script.Enable = bool.TryParse(enable, out var e) ? e : false;
                    proxyScript = script;
                    return true;
                }
            }
            proxyScript = null;
            return false;
        }

        private static string RemoveComment(string str)
        {
            var r = Regex.Replace(str, "^//.*", "", RegexOptions.Multiline);

            return Regex.Replace(r, @"^\s*\n", "", RegexOptions.Multiline);
        }
    }
}
