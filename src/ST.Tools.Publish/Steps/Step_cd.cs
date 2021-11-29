using System.Application.Models;
using System.Application.Models.Internals;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static System.Application.Utils;
using static System.ProjectPathUtil;

namespace System.Application.Steps
{
    /// <summary>
    /// 持续交付
    /// </summary>
    internal static class Step_cd
    {
        static readonly string[] all_val = new[] { "win-x64", "osx-x64", "linux-x64", "linux-arm64", };

        public static void Add(RootCommand command)
        {
            // newv 1. (云端)创建新版本号RSA密钥
            // getv 1. (云端)获取之前创建新版本号RSA密钥
            // 3. (本地)读取剪切板公钥值写入txt的pfx文件中
            var ver = new Command("ver", "初始化版本")
            {
                Handler = CommandHandler.Create(VerHandlerAsync)
            };
            ver.AddOption(new Option<string>("-token", "jwt"));
            ver.AddOption(new Option<bool>("-use_last_skey", "是否使用上一个版本的密钥"));
            ver.AddOption(new Option<bool>("-dev", DevDesc));
            command.AddCommand(ver);

            // 手动发布 pubxml

            // full 中间步骤多合一
            var full = new Command("full", "自动化打包与上传哈希")
            {
                Handler = CommandHandler.Create(FullHandlerAsync)
            };
            full.AddOption(new Option<string>("-token", "jwt"));
            full.AddOption(new Option<string[]>("-val", InputPubDirNameDesc));
            full.AddOption(new Option<bool>("-dev", DevDesc));
            command.AddCommand(full);
        }

        static async Task VerHandlerAsync(string token, bool use_last_skey, bool dev)
        {
            var request = new CreateVersionRequest
            {
                Version = Utils.Version,
                Desc = ReadVersionDesc(),
                UseLastSKey = use_last_skey,
            };
            using var client = GetHttpClient(token);
            using var rsp = await client.PostAsJsonAsync(api_version_create, request);
            IApiResponse<AppIdWithPublicKey>? apiResponse = await rsp.Content.ReadFromJsonAsync<ApiResponseImpl<AppIdWithPublicKey>>();
            apiResponse = apiResponse.ThrowIsNull(nameof(apiResponse));
            if (!apiResponse.IsSuccess) throw new Exception(apiResponse.Message);
            var value = apiResponse.Content;
            if (!Step3.Handler(value, dev)) throw new Exception("Step3.Handler fail.");
        }

        static async Task FullHandlerAsync(string token, string[] val, bool dev)
        {
            if (!val.Any()) val = all_val;

            Dictionary<DeploymentMode, string[]> publishDict = new()
            {
                { DeploymentMode.SCD, val },
            };
            var fde_val = val.Where(x => x.StartsWith("win-")).ToArray(); // 仅 Windows 发行依赖部署(FDE) 包
            if (fde_val.Any()) publishDict.Add(DeploymentMode.FDE, fde_val);

            // X. (本地)将发布 Host 入口点重定向到 Bin 目录中
            var hpTasks = publishDict.Keys.Select(x => Task.Run(() => StepAppHostPatcher.Handler(x, endWriteOK: false))).ToArray();
            await Task.WhenAll(hpTasks);

            List<PublishDirInfo> publishDirs = new();
            // 5. (本地)验证发布文件夹与计算文件哈希
            foreach (var item in publishDict)
            {
                var dirNames = ScanDirectory(item.Key, item.Value, dev);
                if (dirNames.Any_Nullable()) publishDirs.AddRange(dirNames!);
            }
            var linux_publishDirs = publishDirs.Where(x => x.Name.StartsWith("linux-"));

            // 8. (本地)读取上一步操作后的 Publish.json 生成压缩包并计算哈希值写入 Publish.json
            var parallelTasks = new List<Task>();

            // 创建压缩包
            var gs = publishDirs.Select(x => Task.Run(() => GenerateCompressedPackage(x)));
            parallelTasks.AddRange(gs);

            // Create a CentOS/RedHat Linux installer
            Step_rpm.Init();
            var rpms = linux_publishDirs.Select(x => Task.Run(() => Step_rpm.HandlerItem(x)));
            parallelTasks.AddRange(rpms);

            // Create a Ubuntu/Debian Linux installer
            var debs = linux_publishDirs.Select(x => Task.Run(() => Step_deb.HandlerItem(x)));
            parallelTasks.AddRange(debs);

            await Task.WhenAll(parallelTasks);

            // 12. (本地)读取 **Publish.json** 中的 SHA256 值写入 release-template.md
            StepRel.Handler2(endWriteOK: false);

            // wdb 11. (云端)读取上一步上传的数据写入数据库中
            var request = new UpdateVersionRequest
            {
                Version = Utils.Version,
                DirNames = publishDirs.Where(x => x.Name.StartsWith("win-x64")).ToArray(),
            };
            using var client = GetHttpClient(token);
            using var rsp = await client.PutAsJsonAsync(api_version_create, request);
            IApiResponse? apiResponse = await rsp.Content.ReadFromJsonAsync<ApiResponseImpl>();
            apiResponse = apiResponse.ThrowIsNull(nameof(apiResponse));
            if (!apiResponse.IsSuccess) throw new Exception(apiResponse.Message);

            Console.WriteLine("OK");
        }

        /// <summary>
        /// 扫描发布文件夹，生成文件清单
        /// </summary>
        /// <param name="d"></param>
        /// <param name="val"></param>
        /// <param name="dev"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        static PublishDirInfo[]? ScanDirectory(DeploymentMode d, string[] val, bool dev)
        {
            var pubPath = d switch
            {
                DeploymentMode.SCD => DirPublish_,
                DeploymentMode.FDE => DirPublish_FDE_,
                _ => throw new ArgumentOutOfRangeException(nameof(d), d, null),
            };

            var dirBasePath = projPath + string.Format(pubPath, dev ? "Debug" : "Release");
            var dirNames = val.Select(x => new PublishDirInfo(x, Path.Combine(dirBasePath, x), d)).Where(x => Directory.Exists(x.Path)).ToArray();

            foreach (var item in dirNames)
            {
                if (!Directory.Exists(item.Path))
                {
                    Console.WriteLine($"错误：找不到发布文件夹({item.Name})，{item.Path}");
                    continue;
                }

                ScanPath(item.Path, item.Files, ignoreRootDirNames: ignoreDirNames);

                Console.WriteLine($"{item.Name}, count: {item.Files.Count}");
            }

            return dirNames;
        }

        /// <summary>
        /// 根据文件清单生成压缩包
        /// </summary>
        /// <param name="item"></param>
        static void GenerateCompressedPackage(PublishDirInfo item)
        {
            var type = GetCompressedTypeByRID(item.Name);
            var fileEx = GetFileExByCompressedType(type);

            var packPath = GetPackPath(item, fileEx);
            Console.WriteLine($"正在生成压缩包：{packPath}");
            IOPath.FileIfExistsItDelete(packPath);

            GetCreatePackByCompressedType(type)(packPath, item.Files);

            using var fileStream = File.OpenRead(packPath);
            var sha256 = Hashs.String.SHA256(fileStream);

            if (item.BuildDownloads.ContainsKey(type))
            {
                item.BuildDownloads[type] = new PublishFileInfo { SHA256 = sha256, Length = fileStream.Length };
            }
            else
            {
                item.BuildDownloads.Add(type, new PublishFileInfo { SHA256 = sha256, Length = fileStream.Length });
            }
        }

        /// <summary>
        /// 根据 RID 获取压缩包类型
        /// </summary>
        /// <param name="rid"></param>
        /// <returns></returns>
        static AppDownloadType GetCompressedTypeByRID(string rid)
        {
            //if (rid.StartsWith("linux-")) return AppDownloadType.Compressed_Zstd;
            return AppDownloadType.Compressed_7z;
        }

        /// <summary>
        /// 根据压缩包类型获取文件扩展名
        /// </summary>
        /// <param name="compressedType"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        static string GetFileExByCompressedType(AppDownloadType compressedType) => compressedType switch
        {
            AppDownloadType.Compressed_GZip => FileEx.TAR_GZ,
            AppDownloadType.Compressed_Br => FileEx.TAR_BR_LONG,
            AppDownloadType.Compressed_7z => FileEx._7Z,
            AppDownloadType.Compressed_Zstd => FileEx.TAR_ZST,
            AppDownloadType.Compressed_XZ => FileEx.TAR_XZ,
            _ => throw new ArgumentOutOfRangeException(nameof(compressedType), compressedType, null),
        };

        /// <summary>
        /// 根据压缩包类型获取创建压缩包委托
        /// </summary>
        /// <param name="compressedType"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        static Action<string, IEnumerable<PublishFileInfo>> GetCreatePackByCompressedType(AppDownloadType compressedType) => compressedType switch
        {
            AppDownloadType.Compressed_GZip => CreatePack,
            AppDownloadType.Compressed_Br => CreateBrotliPack,
            AppDownloadType.Compressed_7z => CreateSevenZipPack,
            AppDownloadType.Compressed_Zstd => CreateZstdPack,
            AppDownloadType.Compressed_XZ => CreateXZPack,
            _ => throw new ArgumentOutOfRangeException(nameof(compressedType), compressedType, null),
        };

        /// <summary>
        /// 读取更新日志
        /// </summary>
        /// <returns></returns>
        static string ReadVersionDesc()
        {
            throw new NotImplementedException("TODO");
            return string.Empty;
        }

        static HttpClient GetHttpClient(string token)
        {
            var client = new HttpClient
            {
                BaseAddress = new Uri(api_base_url)
            };
            client.DefaultRequestHeaders.Authorization = new(token);
            return client;
        }

        const string api_base_url = "https://cycyadmin.steampp.net";
        const string api_version_create = "/api/version";
    }
}