using MessagePack;
using Octokit;
using System.Application.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using static System.ProjectPathUtil;

namespace System
{
    /// <summary>
    /// 开源协议清单列表自动生成工具
    /// </summary>
    static class Program
    {
        const string OpenSourceLibraryListEmoji = "📄";

        static async Task Main(string[] args)
        {
            var readmePath = Path.Combine(projPath, README);

            var file = Path.Combine(projPath, "resources", "OpenSourceLibraryList.mpo");
            if (File.Exists(file))
            {
                var data = MessagePackSerializer.Deserialize<List<OpenSourceLibrary>>(File.ReadAllBytes(file));
                Print(data);
                Console.ReadLine();
                return;
            }

            using var readmeRead = File.OpenText(readmePath);
            var isOpenSourceLibraryList = false;
            string? line;
            var list = new List<OpenSourceLibrary>();
            var client = new HttpClient();
            var github = new GitHubClient(new ProductHeaderValue("MyAmazingApp"));

            var token = Path.Combine(projPath, "github-token.pfx");
            if (File.Exists(token))
            {
                token = File.ReadAllText(token);
                var tokenAuth = new Credentials(token);
                github.Credentials = tokenAuth;
                Console.WriteLine($"Add Token: {token.FirstOrDefault()}");
                Console.WriteLine();
            }

            do
            {
                line = readmeRead.ReadLine();
                if (line == null) break;
                if (line.StartsWith("##"))
                {
                    if (isOpenSourceLibraryList)
                    {
                        break;
                    }
                    if (line.Contains(OpenSourceLibraryListEmoji) && (line.Contains("开源") || (line.Contains("Open") && line.Contains("Source"))))
                    {
                        isOpenSourceLibraryList = true;
                    }
                }
                if (isOpenSourceLibraryList)
                {
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        if (line.StartsWith("* "))
                        {
                            var name = line.Substring("[", "]");
                            if (string.IsNullOrWhiteSpace(line)) continue;
                            var index = line.IndexOf("]");
                            if (index < 0) continue;
                            var url = line[index..].Substring("(", ")");
                            if (string.IsNullOrWhiteSpace(url)) continue;
                            var item = new OpenSourceLibrary
                            {
                                Name = name,
                                Url = url,
                            };
                            const string githubUrl = "https://github.com/";
                            if (item.Url.StartsWith(githubUrl, StringComparison.OrdinalIgnoreCase))
                            {
                                var arr = item.Url[githubUrl.Length..].Split("/", StringSplitOptions.RemoveEmptyEntries);
                                if (arr.Length == 2)
                                {
                                    var owner = arr[0];
                                    var repo = arr[1];
                                    try
                                    {
                                        Console.WriteLine(item.Url);
                                        Console.WriteLine();
                                        var contents = await github.Repository.GetLicenseContents(owner, repo);
                                        item.License = contents.License.SpdxId;
                                        item.LicenseUrl = contents.DownloadUrl;
                                        item.LicenseText = await client.GetStringAsync(contents.DownloadUrl);
                                        if (!string.IsNullOrWhiteSpace(item.LicenseText))
                                        {
                                            var licenselines = item.LicenseText.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                                            foreach (var licenseline in licenselines)
                                            {
                                                if (licenseline.Contains("Copyright (c)"))
                                                {
                                                    const string start_del_1 = "// ";
                                                    var licenseline_ = licenseline;
                                                    if (licenseline_.StartsWith(start_del_1))
                                                    {
                                                        licenseline_ = licenseline_[start_del_1.Length..];
                                                    }
                                                    licenseline_ = licenseline_.Trim();
                                                    if (string.IsNullOrWhiteSpace(item.Copyright))
                                                    {
                                                        item.Copyright = licenseline_;
                                                    }
                                                    else
                                                    {
                                                        item.Copyright += Environment.NewLine + licenseline_;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    catch (NotFoundException)
                                    {
                                    }
                                }
                            }
                            list.Add(item);
                        }
                    }
                }
            }
            while (true);

            //var licenses = list.Where(x => !string.IsNullOrWhiteSpace(x.LicenseText)).Select(x => x.LicenseText).Distinct();
            //var licenses_ = new Dictionary<string, string?>();
            //foreach (var license in licenses)
            //{
            //    if (license == null || license == "other") continue;
            //    Console.WriteLine($"license: {license}");
            //    Console.WriteLine();
            //    try
            //    {
            //        var value = await github.Miscellaneous.GetLicense(license);
            //        licenses_.Add(license, value.Body);
            //    }
            //    catch (NotFoundException)
            //    {
            //        licenses_.Add(license, null);
            //    }
            //}

            //foreach (var item in list)
            //{
            //    if (string.IsNullOrWhiteSpace(item.LicenseText))
            //    {
            //        item.LicenseText = null;
            //    }
            //    else if (licenses_.ContainsKey(item.LicenseText))
            //    {
            //        item.LicenseText = licenses_[item.LicenseText];
            //    }
            //    else
            //    {
            //        item.LicenseText = null;
            //    }
            //}

            var lz4Options = MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray);
            var bytes = MessagePackSerializer.Serialize(list, lz4Options);
            File.WriteAllBytes(file, bytes);

            Print(list);

            Console.WriteLine("OK");
            Console.ReadLine();
        }

        const string README = "README.md";

        static void Print(List<OpenSourceLibrary> items)
        {
            Console.WriteLine(OpenSourceLibrary.ToString(items));
            Console.WriteLine($"Count: {items.Count}");
        }
    }
}