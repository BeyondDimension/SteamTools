using MessagePack;
using Octokit;
using System.Application.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace System
{
    static class Program
    {
        const string OpenSourceLibraryListEmoji = "📄";

        static async Task Main(string[] args)
        {
            var projPath = GetProjectPath();
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
                            var item = new OpenSourceLibrary
                            {
                                Name = line.Substring("[", "]"),
                                Url = line.Substring("(", ")"),
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
                                        item.LicenseText = await client.GetStringAsync(contents.DownloadUrl);
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

            var bytes = MessagePackSerializer.Serialize(list);
            File.WriteAllBytes(file, bytes);

            Print(list);

            Console.WriteLine("OK");
            Console.ReadLine();
        }

        const string README = "README.md";

        static string GetProjectPath(string? path = null)
        {
            path ??= AppContext.BaseDirectory;
            if (!Directory.GetFiles(path, README).Any())
            {
                var parent = Directory.GetParent(path);
                if (parent == null) return string.Empty;
                return GetProjectPath(parent.FullName);
            }
            return path;
        }

        static void Print(List<OpenSourceLibrary> items)
        {
            Console.WriteLine(OpenSourceLibrary.ToString(items));
            Console.WriteLine($"Count: {items.Count}");
        }
    }
}