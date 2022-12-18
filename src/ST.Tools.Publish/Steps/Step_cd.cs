using System.Application.Models;
using System.Application.Models.Internals;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.NamingConventionBinder;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static System.Application.Utils;
using static System.ProjectPathUtil;
using _ThisAssembly = System.Properties.ThisAssembly;

namespace System.Application.Steps
{
    /// <summary>
    /// 持续交付
    /// </summary>
    internal static class Step_cd
    {
        public static readonly string[] all_val = new[] { "win-x64", "win-x86", "osx-x64", "osx-arm64", "linux-x64", "linux-arm64", };

        public static void Add(RootCommand command)
        {
            // newv 1. (云端)创建新版本号RSA密钥
            // getv 1. (云端)获取之前创建新版本号RSA密钥
            // 3. (本地)读取剪切板公钥值写入txt的pfx文件中
            var ver = new Command("ver", "初始化版本")
            {
                Handler = CommandHandler.Create(VerHandlerAsync),
            };
            ver.AddOption(new Option<string>("-token", "jwt"));
            ver.AddOption(new Option<bool>("-use_last_skey", "是否使用上一个版本的密钥"));
            ver.AddOption(new Option<bool>("-dev", DevDesc));
            command.AddCommand(ver);

            // 手动发布 pubxml

            // full 中间步骤多合一
            var full = new Command("full", "自动化打包与上传哈希")
            {
                Handler = CommandHandler.Create(FullHandlerAsync),
            };
            full.AddOption(new Option<string>("-token", "jwt"));
            full.AddOption(new Option<string[]>("-val", InputPubDirNameDesc));
            full.AddOption(new Option<bool>("-dev", DevDesc));
            full.AddOption(new Option<string>("-win_sign_pfx_pwd", "Windows 数字签名证书密码"));
            command.AddCommand(full);

#if DEBUG
            // p nsis -path "path"
            var nsis = new Command("nsis", "调试 NSIS 打包")
            {
                Handler = CommandHandler.Create((string path) =>
                {
                    if (!string.IsNullOrWhiteSpace(path))
                    {
                        var buildDownloads = new Dictionary<AppDownloadType, PublishFileInfo>
                        {
                            { AppDownloadType.Compressed_7z, new PublishFileInfo { Path = path } }
                        };
                        var dirName = new PublishDirInfo
                        {
                            BuildDownloads = buildDownloads,
                        };

                        NSISBuild(true, new[] { dirName });
                    }
                    else
                    {
                        var publish_json_path = PublishJsonFilePath;
                        var publish_json_str = File.ReadAllText(publish_json_path);
                        var dirNames = Serializable.DJSON<PublishDirInfo[]>(publish_json_str)
                            ?.Where(x => x.Name.StartsWith("win-")).ToArray();

                        if (!dirNames.Any_Nullable())
                        {
                            Console.WriteLine($"错误：发布配置文件读取失败！{publish_json_path}");
                            return;
                        }

                        dirNames = dirNames.ThrowIsNull(nameof(dirNames));

                        NSISBuild(true, dirNames);
                    }
                })
            };
            nsis.AddOption(new Option<string>("-path", "7z Path"));
            command.AddCommand(nsis);

            // ./p osxapp -val osx-x64 osx-arm64 
            var osxapp = new Command("osxapp", "调试 OSX 打包")
            {
                Handler = CommandHandler.Create((string[] val, bool dev) =>
                {
                    OSXBuild(dev, val);
                }),
            };
            osxapp.AddOption(new Option<string[]>("-val", InputPubDirNameDesc));
            osxapp.AddOption(new Option<bool>("-dev", DevDesc));
            command.AddCommand(osxapp);
#endif
        }

        public static async Task VerHandlerAsync(string token, bool use_last_skey, bool dev)
        {
            var desc = ReadVersionDesc();
            var request = new CreateVersionRequest
            {
                Version = GetVersion(dev),
                Desc = desc,
                UseLastSKey = use_last_skey,
            };
            using var client = GetHttpClient(token, dev);
            using var req = GetRequestContent(request);
            using var rsp = await client.PostAsync(api_version_create, req);
            if (rsp.StatusCode == HttpStatusCode.InternalServerError)
            {
                var html = await rsp.Content.ReadAsStringAsync();
                Console.WriteLine(html);
            }
            rsp.EnsureSuccessStatusCode();
            var apiResponse = await GetResponseAsync<AppIdWithPublicKey>(rsp);
            apiResponse = apiResponse.ThrowIsNull(nameof(apiResponse));
            if (!apiResponse.IsSuccess) throw new Exception(apiResponse.Message);
            var value = apiResponse.Content;
            if (!Step3.Handler(value, dev)) throw new Exception("Step3.Handler fail.");
        }

        public static readonly bool EnableStepAppHostPatcher = false; // 因 https://github.com/dotnet/runtime/pull/63533 使用单文件不在需要 Bin 文件夹

        static async Task FullHandlerAsync(string token, string[] val, bool dev, string win_sign_pfx_pwd)
        {
            if (!val.Any()) val = all_val;

            var hasWindows = val.Any(x => x.StartsWith("win-"));
            var hasLinux = val.Any(x => x.StartsWith("linux-"));
            var hasOsx = val.Any(x => x.StartsWith("osx-"));

            Dictionary<DeploymentMode, string[]> publishDict = new()
            {
                { DeploymentMode.SCD, val },
            };

            if (hasWindows)
            {
                var fde_val = val.Where(x => x.StartsWith("win-")).ToArray(); // 仅 Windows 发行依赖部署(FDE) 包
                if (fde_val.Any()) publishDict.Add(DeploymentMode.FDE, fde_val);

                Console.WriteLine($"EnableStepAppHostPatcher: {EnableStepAppHostPatcher}");
                if (EnableStepAppHostPatcher)
                {
                    // X. (本地)将发布 Host 入口点重定向到 Bin 目录中
                    var tasks = publishDict.Keys.Select(x => Task.Run(() => StepAppHostPatcher.Handler(dev, x, endWriteOK: false))).ToArray();
                    await Task.WhenAll(tasks);
                }

                if (OperatingSystem2.IsWindows())
                {
                    // 程序集数字签名
                    if (!string.IsNullOrWhiteSpace(win_sign_pfx_pwd))
                    {
                        var tasks = publishDict.Keys.Select(x => Task.Run(() => DigitalSignUtil.Handler(win_sign_pfx_pwd, dev, x, endWriteOK: false))).ToArray();
                        await Task.WhenAll(tasks);
                    }
                }
            }

            if (hasOsx)
            {
                if (OperatingSystem2.IsMacOS())
                {
                    var osx_val = val.Where(x => x.StartsWith("osx-")).ToArray();
                    if (osx_val.Any()) OSXBuild(dev, osx_val);
                }
            }
            List<PublishDirInfo> publishDirs = new();
            // 5. (本地)验证发布文件夹与计算文件哈希
            foreach (var item in publishDict)
            {
                var dirNames = ScanDirectory(item.Key, item.Value, dev);
                if (dirNames.Any_Nullable()) publishDirs.AddRange(dirNames!);
            }

            // 8. (本地)读取上一步操作后的 Publish.json 生成压缩包并计算哈希值写入 Publish.json
            var parallelTasks = new List<Task>();

            Console.WriteLine("7z Step 正在创建压缩包...");

            // 创建压缩包
            PerformanceCounter? ramCounter = null;
            if (OperatingSystem2.IsWindows())
            {
                var processName = Process.GetCurrentProcess().ProcessName;
                ramCounter = new PerformanceCounter("Process", "Working Set", processName);
            }
            void WriteRAMLine()
            {
                if (ramCounter != null)
                    Console.WriteLine($"Working Set: {IOPath.GetDisplayFileSizeString(ramCounter.NextValue())}");
            }
            foreach (var item in publishDirs)
            {
                WriteRAMLine();
                GenerateCompressedPackage(dev, item);
                WriteRAMLine();
            }

            if (hasWindows)
            {
                var win_publishDirs = publishDirs.Where(x => x.Platform == OSPlatform.Windows);

                Console.WriteLine("nsis Step 正在打包 EXE installer...");

                NSISBuild(dev, win_publishDirs, win_sign_pfx_pwd);
            }

            if (hasLinux)
            {
                var linux_publishDirs = publishDirs.Where(x => x.Platform == OSPlatform.Linux);

                Console.WriteLine("rpm Step 正在打包 CentOS/RedHat Linux installer...");

                // Create a CentOS/RedHat Linux installer
                Step_rpm.Init();
                var rpms = linux_publishDirs.Select(x => Task.Run(() => Step_rpm.HandlerItem(dev, x)));
                parallelTasks.AddRange(rpms);

                await Task.WhenAll(parallelTasks);
                parallelTasks.Clear();

                Console.WriteLine("deb Step 正在打包 Ubuntu/Debian Linux installer...");

                // Create a Ubuntu/Debian Linux installer
                var debs = linux_publishDirs.Select(x => Task.Run(() => Step_deb.HandlerItem(dev, x)));
                parallelTasks.AddRange(debs);

                await Task.WhenAll(parallelTasks);
                parallelTasks.Clear();
            }
            #region rel 12. (本地)读取 **Publish.json** 中的 SHA256 值写入 release-template.md

            //Console.WriteLine("rel Step 正在写入 SHA256...");

            //// 12. (本地)读取 **Publish.json** 中的 SHA256 值写入 release-template.md
            //StepRel.Handler2(endWriteOK: false);

            #endregion

            var winX64 = publishDirs.Where(x => x.Platform == OSPlatform.Windows && x.Architecture == Architecture.X64).ToArray();
            if (winX64.Any())
            {
                Console.WriteLine("wdb Step 正在上传新版本数据中...");

                // wdb 11. (云端)读取上一步上传的数据写入数据库中
                var request = new UpdateVersionRequest
                {
                    Version = GetVersion(dev),
                    DirNames = winX64,
                };
                using var client = GetHttpClient(token, dev);
                using var req = GetRequestContent(request);
                using var rsp = await client.PutAsync(api_version_create, req);
                if (rsp.StatusCode == HttpStatusCode.InternalServerError)
                {
                    var html = await rsp.Content.ReadAsStringAsync();
                    throw new HttpRequestException(html);
                }
                rsp.EnsureSuccessStatusCode();
                var apiResponse = await GetResponseAsync(rsp);
                apiResponse = apiResponse.ThrowIsNull(nameof(apiResponse));
                if (!apiResponse.IsSuccess) throw new Exception(apiResponse.Message);
            }

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

            var dirBasePath = projPath + string.Format(pubPath, GetConfiguration(dev, isLower: false));
            var dirNames = val.Select(x => new PublishDirInfo(x, Path.Combine(dirBasePath, x), d)).Where(x => Directory.Exists(x.Path)).ToArray();

            foreach (var item in dirNames)
            {
                if (!Directory.Exists(item.Path))
                {
                    Console.WriteLine($"错误：找不到发布文件夹({item.Name})，{item.Path}");
                    continue;
                }

                OnScanPathBefore(item);

                ScanPath(item.Path, item.Files, ignoreRootDirNames: ignoreDirNames);

                Console.WriteLine($"{item.Name}, count: {item.Files.Count}");
            }

            return dirNames;
        }

        static void GenerateCompressedPackage(bool dev, PublishDirInfo item)
        {
            var type = GetCompressedTypeByRID(item.Name);
            GenerateCompressedPackage(dev, item, type);
        }

        /// <summary>
        /// 根据文件清单生成压缩包
        /// </summary>
        /// <param name="dev"></param>
        /// <param name="item"></param>
        /// <param name="type"></param>
        public static void GenerateCompressedPackage(bool dev, PublishDirInfo item, AppDownloadType type)
        {
            try
            {
                var isLinux = item.Platform == OSPlatform.Linux;
                if (isLinux)
                {
                    type = AppDownloadType.Compressed_Zstd;
                }

                var fileEx = GetFileExByCompressedType(type);

                var packPath = GetPackPath(dev, item, fileEx);
                Console.WriteLine($"正在生成压缩包：{packPath}");
                IOPath.FileIfExistsItDelete(packPath);

                GetCreatePackByCompressedType(type)(packPath, item.Files);

                using var fileStream = File.OpenRead(packPath);
                var sha256 = Hashs.String.SHA256(fileStream);

                var fileInfoM = new PublishFileInfo
                {
                    SHA256 = sha256,
                    Length = fileStream.Length,
                    Path = packPath,
                };

                if (item.BuildDownloads.ContainsKey(type))
                {
                    item.BuildDownloads[type] = fileInfoM;
                }
                else
                {
                    item.BuildDownloads.Add(type, fileInfoM);
                }
                Console.WriteLine($"压缩包已生成：{packPath}");
            }
            finally
            {
                GC.Collect();
            }
        }

        /// <summary>
        /// 根据 RID 获取压缩包类型
        /// </summary>
        /// <param name="rid"></param>
        /// <returns></returns>
        static AppDownloadType GetCompressedTypeByRID(string rid)
        {
            if (rid.StartsWith("linux-")) return AppDownloadType.Compressed_Zstd;
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
            const char splitChar = ';';
            var release_template_md_path = projPath + release_template_md;
            using var sr = File.OpenText(release_template_md_path);
            StringBuilder builder = new();
            var isFirstTitle = false;
            var num = 1;
            while (true)
            {
                var line = sr.ReadLine();
                if (line == null) break;
                if (string.IsNullOrWhiteSpace(line)) continue;
                if (line.StartsWith('#'))
                {
                    var title = line.Split(' ', StringSplitOptions.RemoveEmptyEntries).Skip(1).FirstOrDefault();
                    if (!string.IsNullOrWhiteSpace(title))
                    {
                        if (title == "已知问题" || title.Contains("下载指南"))
                        {
                            break;
                        }
                    }
                    if (!isFirstTitle)
                    {
                        builder.Append(splitChar);
                        isFirstTitle = true;
                    }
                    builder.Append(title);
                    num = 1;
                    builder.Append(splitChar);
                }
                else if (line.Contains('.'))
                {
                    builder.Append(num++);
                    builder.Append(". ");
                    var lineArray = line.Split('.');
                    var numStr = lineArray.FirstOrDefault();
                    if (!string.IsNullOrWhiteSpace(numStr) && numStr.Length <= 3 && !byte.TryParse(numStr, out var _)) continue;
                    lineArray = lineArray.Skip(1).ToArray();
                    for (int i = 0; i < lineArray.Length; i++)
                    {
                        var item = lineArray[i];
                        builder.Append(item);
                        if (i != lineArray.Length - 1) builder.Append('.');
                    }
                    builder.Append(splitChar);
                }
            }
            if (builder[^1] == splitChar)
            {
                builder.Remove(builder.Length - 1, 1);
            }
            return builder.ToString();
        }

        static HttpClient GetHttpClient(string token, bool dev)
        {
            var handler = new SocketsHttpHandler
            {
                SslOptions = new()
                {
                    RemoteCertificateValidationCallback = delegate { return true; },
                },
            };
            var client = new HttpClient(handler)
            {
                BaseAddress = new Uri(dev ? dev_api_base_url : api_base_url),
                DefaultRequestVersion = HttpVersion.Version20,
                DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrHigher,
            };
            client.DefaultRequestHeaders.Add(AppIdWithPublicKey.AuthorizationToken, token);
            return client;
        }

        static HttpContent GetRequestContent<T>(T request)
        {
            //var bytes = Serializable.SMP(request);
            //var content = new ByteArrayContent(bytes);
            //content.Headers.ContentType = new(MediaTypeNames.MessagePack);
            //return content;

            var json = Serializable.SJSON(request);
            var content = new StringContent(json, Encoding.UTF8, MediaTypeNames.JSON);
            return content;
        }

        static async Task<IApiResponse<T>> GetResponseAsync<T>(HttpResponseMessage responseMessage)
        {
            //using var stream = await responseMessage.Content.ReadAsStreamAsync();
            //return await ApiResponse.DeserializeAsync<T>(stream, default);

            var json = await responseMessage.Content.ReadAsStringAsync();
            return Serializable.DJSON<ApiRsp<T>>(json)!;
        }

        static async Task<IApiResponse> GetResponseAsync(HttpResponseMessage responseMessage)
        {
            //using var stream = await responseMessage.Content.ReadAsStreamAsync();
            //return await ApiResponse.DeserializeAsync<object>(stream, default);

            var json = await responseMessage.Content.ReadAsStringAsync();
            return Serializable.DJSON<ApiRsp>(json)!;
        }

        const string dev_api_base_url = "https://pan.mossimo.net:9911";
        const string api_base_url = "https://adminapi.steampp.net";
        const string api_version_create = "/api/version";

        static string GetFullVersion(bool dev)
        {
            Version version = new(GetVersion(dev));
            return $"{version.Major}.{GetInt32ByVersion(version.Minor)}.{GetInt32ByVersion(version.Build)}.{GetInt32ByVersion(version.Revision)}";

            static int GetInt32ByVersion(int value) => value < 0 ? 0 : value;
        }

        const string AigioPC = "ee6c36c1bbf6076e5f915b12cd3c7d034f0d6f45b71c934529ba9f9faba72735084399e6039375501c8fbabc245ac3a3";
        static readonly string MachineName = Hashs.String.SHA384(Environment.MachineName);

        static void NSISBuild(bool dev, IEnumerable<PublishDirInfo> publishDirs, string? win_sign_pfx_pwd = null)
        {
            string rootDirPath;
            if (MachineName == AigioPC)
            {
                rootDirPath = Path.Combine(projPath, "..", "NSIS");
            }
            else
            {
                rootDirPath = Path.Combine(projPath, "NSIS-Build");
            }
            if (!Directory.Exists(rootDirPath))
            {
                Console.WriteLine($"找不到 NSIS-Build 目录，值：{rootDirPath}");
                return;
            }

            var nsiFilePath = Path.Combine(rootDirPath, "AppCode", "Steampp", "app", "SteamPP_setup.nsi");
            var nsiFileContent = File.ReadAllText(nsiFilePath);

            var version = GetFullVersion(dev);

            var appFileDirPath = Path.Combine(rootDirPath, "AppCode", "Steampp");
            //var batFilePath = Path.Combine(rootDirPath, "steam++.bat");
            var nsisExeFilePath = Path.Combine(rootDirPath, "NSIS", "makensis.exe");
            var exeFilePaths = new List<string>();
            foreach (var item in publishDirs)
            {
                var install7zFilePath = item.BuildDownloads[AppDownloadType.Compressed_7z].Path;
                var install7zFileName = Path.GetFileName(install7zFilePath);
                var outputFileName = Path.GetFileNameWithoutExtension(install7zFilePath) + FileEx.EXE;
                var outputFilePath = Path.Combine(new FileInfo(install7zFilePath).DirectoryName!, outputFileName);
                var exeName = "Steam++.exe";
                const string launcherExeName = "Steam++.Launcher.exe";

                if (item.DeploymentMode == DeploymentMode.FDE)
                {
                    if (File.Exists(Path.Combine(item.Path, launcherExeName)))
                    {
                        exeName = launcherExeName;
                    }
                }

                var nsiFileContent2 = nsiFileContent
                     .Replace("${{ Steam++_Company }}", _ThisAssembly.AssemblyCompany)
                     .Replace("${{ Steam++_Copyright }}", _ThisAssembly.AssemblyCopyright)
                     .Replace("${{ Steam++_ProductName }}", _ThisAssembly.AssemblyTrademark)
                     .Replace("${{ Steam++_ExeName }}", exeName)
                     .Replace("${{ Steam++_Version }}", version)
                     .Replace("${{ Steam++_OutPutFileName }}", outputFileName)
                     .Replace("${{ Steam++_AppFileDir }}", appFileDirPath)
                     .Replace("${{ Steam++_7zFilePath }}", install7zFilePath)
                     .Replace("${{ Steam++_7zFileName }}", install7zFileName)
                     .Replace("${{ Steam++_OutPutFilePath }}", outputFilePath)
                     ;
                File.WriteAllText(nsiFilePath, nsiFileContent2);

                var process = Process.Start(new ProcessStartInfo()
                {
                    FileName = nsisExeFilePath,
                    Arguments = $" /DINSTALL_WITH_NO_NSIS7Z=1 \"{nsiFilePath}\"",
                    UseShellExecute = false,
                });
                process!.WaitForExit();

                exeFilePaths.Add(outputFilePath);
            }

            if (!string.IsNullOrWhiteSpace(win_sign_pfx_pwd))
            {
                DigitalSignUtil.Signature(win_sign_pfx_pwd, exeFilePaths);
            }
        }

        [SupportedOSPlatform("MacOS")]
        static void OSXBuild(bool dev, string[] osx_val)
        {
            var shFilePath = Path.Combine(projPath, "packaging", "build-osx-app.sh");
            var icnsFilePath = Path.Combine(projPath, "resources", "AppIcon", "Logo.icns");
            if (!File.Exists(shFilePath))
            {
                Console.WriteLine($"找不到 sh 文件，值：{shFilePath}");
                return;
            }
            if (!File.Exists(icnsFilePath))
            {
                Console.WriteLine($"找不到 icns 文件，值：{icnsFilePath}");
                return;
            }

            var mCFBundleVersion = GetFullVersion(dev);
            var mCFBundleShortVersionString = mCFBundleVersion.TrimEnd(".0");

            var shFileContent = File.ReadAllText(shFilePath);
            foreach (var item in osx_val)
            {
                var destPath = projPath + string.Format(DirPublishOsx, item);
                destPath = destPath.Replace('\\', '/');
                if (!Directory.Exists(destPath))
                {
                    Console.WriteLine($"找不到 destPath 文件夹，值：{destPath}");
                    continue;
                }
                //var appName = $"{_ThisAssembly.AssemblyTrademark}{(item == "osx-x64" ? "" : " Arm64")}";
                const string appName = _ThisAssembly.AssemblyTrademark;
                var shFileContent2 = shFileContent
                        .Replace("${{ Steam++_AppName }}", appName)
                        .Replace("${{ Steam++_Version }}", mCFBundleVersion)
                        .Replace("${{ Steam++_ShortVersion }}", mCFBundleShortVersionString)
                        .Replace("${{ Steam++_IcnsFile }}", icnsFilePath)
                        .Replace("${{ Steam++_OutPutFilePath }}", projPath)
                        .Replace("${{ Steam++_APPDIR }}", destPath)
                        .Replace("${{ Steam++_RunName }}", "Steam++")
                        ;
                File.WriteAllText(shFilePath, shFileContent2);
                _ = Chmod(shFilePath, (int)EUnixPermission.Combined777);
                var process = Process.Start(new ProcessStartInfo()
                {
                    FileName = "bash",
                    Arguments = $"-c {shFilePath}",
                    UseShellExecute = false,
                });

                process!.WaitForExit();
            }
        }

        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [DllImport("libc", EntryPoint = "chmod", SetLastError = true)]
        [SupportedOSPlatform("FreeBSD")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("MacOS")]
        internal static extern int Chmod(string path, int mode);

        [Flags]
        [SupportedOSPlatform("FreeBSD")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("MacOS")]
        internal enum EUnixPermission : ushort
        {
            OtherExecute = 0x1,
            OtherWrite = 0x2,
            OtherRead = 0x4,
            GroupExecute = 0x8,
            GroupWrite = 0x10,
            GroupRead = 0x20,
            UserExecute = 0x40,
            UserWrite = 0x80,
            UserRead = 0x100,
            Combined777 = UserRead | UserWrite | UserExecute | GroupRead | GroupWrite | GroupExecute | OtherRead | OtherWrite | OtherExecute
        }
    }
}