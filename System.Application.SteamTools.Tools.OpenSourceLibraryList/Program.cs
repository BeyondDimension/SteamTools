using MessagePack;
using Octokit;
using System.Application.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

//https://github.com/runceel/Livet
//https://github.com/ninject/Ninject
//https://github.com/SteamDB-API/api
//https://github.com/SteamRE/Steam4NET
//https://github.com/neuecc/MessagePack-CSharp
//https://github.com/gfoidl/Base64
//https://github.com/App-vNext/Polly
//https://github.com/xamarin/essentials

namespace System
{
    static class Program
    {
        static async Task Main(string[] args)
        {
            var projPath = GetProjectPath();
            var readmePath = Path.Combine(projPath, README);

            var file = Path.Combine(projPath, "resources", "OpenSourceLibraryList.mpo");
            if (File.Exists(file)) return;

            using var readmeRead = File.OpenText(readmePath);
            var isOpenSourceLibraryList = false;
            string? line;
            var list = new List<OpenSourceLibrary>();
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
                if (line.Contains("##"))
                {
                    if (isOpenSourceLibraryList)
                    {
                        break;
                    }
                    if (line.Contains("📄"))
                    {
                        isOpenSourceLibraryList = true;
                    }
                }
                if (isOpenSourceLibraryList)
                {
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        var arr = line.Split(' ', StringSplitOptions.RemoveEmptyEntries).Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
                        if (arr.Length == 2 && arr[0] == "*")
                        {
                            var item = new OpenSourceLibrary
                            {
                                Name = arr[1].Substring("[", "]"),
                                Url = arr[1].Substring("(", ")"),
                            };
                            const string githubUrl = "https://github.com/";
                            if (item.Url.StartsWith(githubUrl, StringComparison.OrdinalIgnoreCase))
                            {
                                arr = item.Url[githubUrl.Length..].Split("/", StringSplitOptions.RemoveEmptyEntries);
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
                                        item.LicenseText = contents.License.Key;
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

            var licenses = list.Where(x => !string.IsNullOrWhiteSpace(x.LicenseText)).Select(x => x.LicenseText).Distinct();
            var licenses_ = new Dictionary<string, string?>();
            foreach (var license in licenses)
            {
                if (license == null || license == "other") continue;
                Console.WriteLine($"license: {license}");
                Console.WriteLine();
                try
                {
                    var value = await github.Miscellaneous.GetLicense(license);
                    licenses_.Add(license, value.Body);
                }
                catch (NotFoundException)
                {
                    licenses_.Add(license, null);
                }
            }

            foreach (var item in list)
            {
                if (string.IsNullOrWhiteSpace(item.LicenseText))
                {
                    item.LicenseText = null;
                }
                else if (licenses_.ContainsKey(item.LicenseText))
                {
                    item.LicenseText = licenses_[item.LicenseText];
                }
                else
                {
                    item.LicenseText = null;
                }
            }

            var bytes = MessagePackSerializer.Serialize(list);
            File.WriteAllBytes(file, bytes);

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
    }
}