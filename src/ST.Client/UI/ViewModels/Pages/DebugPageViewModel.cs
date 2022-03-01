#if DEBUG
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using ReactiveUI;
using System.Application.Models;
using System.Application.Repositories;
using System.Application.Services;
using System.Application.Services.Implementation;
using System.Application.UI.Resx;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Newtonsoft.Json.JsonConvert;

// ReSharper disable once CheckNamespace
namespace System.Application.UI.ViewModels
{
    public partial class DebugPageViewModel
    {
        string _DebugString = string.Empty;
        public string DebugString
        {
            get => _DebugString;
            protected set => this.RaiseAndSetIfChanged(ref _DebugString, value);
        }

        public DebugPageViewModel()
        {
            IconKey = nameof(GameRelatedPageViewModel);
        }

        //async void TestHttp()
        //{
        //    var task1 = IHttpService.Instance.GetAsync<string>("https://developers.google.com/android");
        //    var task2 = IHttpService.Instance.GetAsync<string>("https://developer.android.google.cn/");
        //    var str = await Task.WhenAny(task1, task2);
        //    DebugString = str.Result ?? string.Empty;
        //}

        //#if DEBUG
        //        void TestAppUpdate()
        //        {
        //            var s = (ApplicationUpdateServiceBaseImpl)IApplicationUpdateService.Instance;
        //            s.NewVersionInfo = new AppVersionDTO
        //            {
        //                Version = "2.6.1",
        //                Description = "desc",
        //                Platform = Platform.Windows,
        //                SupportedAbis = ArchitectureFlags.X64,
        //                Downloads = new AppVersionDTO.Download[]
        //                {
        //                    new AppVersionDTO.Download
        //                    {
        //                        DownloadType = AppDownloadType.Compressed_7z,
        //                        SHA256 = "f8525e772904a5696e651bae5fbf726861b013ecd6c1a859804e371cd0581e0b",
        //                        Length = 255,
        //                        FileIdOrUrl = "",
        //                        DownloadChannelType= UpdateChannelType.Gitee,
        //                    },
        //                },
        //            };
        //            s.StartUpdate();
        //        }
        //#endif

        public void DebugButton_Click()
        {
            //IHostsFileService.Instance.OccupyHosts();

            TestHttp3Quic();
            return;

            //#if DEBUG
            //            //TestAppUpdate();
            //            //return;
            //#endif

            //TestHttp();
            //return;

            //TestTextBoxWindow(0);

            //DebugButton_Click1();

            //DebugButton_Click_Test();
        }

        //void DebugButton_Click_Test()
        //{
        //    INotificationService.Instance.Notify("aaa", NotificationType.Announcement);

        //    Parallel.For(0, 10, (_, _) =>
        //    {
        //        DebugButton_Click1();
        //        //Task.Run(DebugButton_Click1);
        //    });
        //}

        public async void TestHttp3Quic()
        {
            // https://docs.microsoft.com/zh-cn/dotnet/core/extensions/httpclient-http3
            // https://devblogs.microsoft.com/dotnet/http-3-support-in-dotnet-6
            using HttpClient httpClient = new()
            {
#if NET6_0_OR_GREATER
                DefaultRequestVersion = HttpVersion.Version30,
                DefaultVersionPolicy = HttpVersionPolicy.RequestVersionExact,
#endif
            };

            using HttpRequestMessage request = new(HttpMethod.Get, "https://cloudflare-quic.com/");
            using var response = await httpClient.UseDefaultSendAsync(request);

            var htmlString = await response.Content.ReadAsStringAsync();

            StringBuilder @string = new();
            @string.AppendFormatLine("request.Version: {0}", request.Version);
            @string.AppendFormatLine("response.Version: {0}", response.Version);
            @string.AppendLine(htmlString);
            DebugString = @string.ToString();
            var r = await MessageBox.ShowAsync(DebugString);
            Toast.Show(r.ToString());
        }

        //static async void TestTextBoxWindow(int state)
        //{
        //    string? value = null;
        //    switch (state)
        //    {
        //        case 0:
        //            TextBoxWindowViewModel vm = new()
        //            {
        //                Title = AppResources.MacSudoPasswordTips,
        //                InputType = TextBoxWindowViewModel.TextBoxInputType.Password,
        //                Description = "TestDescription",
        //            };
        //            value = await TextBoxWindowViewModel.ShowDialogAsync(vm);
        //            break;
        //        case 1:
        //            value = await TextBoxWindowViewModel.ShowDialogByPresetAsync(TextBoxWindowViewModel.PresetType.LocalAuth_PasswordRequired);
        //            break;
        //    }
        //    if (!string.IsNullOrEmpty(value))
        //    {
        //        Toast.Show(value);
        //    }
        //}

        static IEnumerable<X509Certificate2> GetCertificates(X509Store store)
        {
#if NETSTANDARD
            return store.Certificates.Cast<X509Certificate2>();
#else
            return store.Certificates;
#endif
        }

        public async void DebugButton_Click1()
        {
            Toast.Show(DateTime.Now.ToString());

            StringBuilder @string = new();

            @string.AppendFormatLine("ThreadId: {0}", Environment.CurrentManagedThreadId);
            @string.AppendFormatLine("CJKTest: {0}", "中文繁體русский языкカタカナ한글");
            @string.AppendFormatLine("CLRVersion: {0}", Environment.Version);
            @string.AppendFormatLine("Culture: {0}", CultureInfo.CurrentCulture);
            @string.AppendFormatLine("UICulture: {0}", CultureInfo.CurrentUICulture);
            @string.AppendFormatLine("DefaultThreadCulture: {0}", CultureInfo.DefaultThreadCurrentCulture);
            @string.AppendFormatLine("DefaultThreadUICulture: {0}", CultureInfo.DefaultThreadCurrentUICulture);

            @string.AppendFormatLine("BaseDirectory: {0}", IOPath.BaseDirectory);
            @string.AppendFormatLine("AppDataDirectory: {0}", IOPath.AppDataDirectory);
            @string.AppendFormatLine("CacheDirectory: {0}", IOPath.CacheDirectory);

            @string.AppendFormatLine("UserName: {0}", Environment.UserName);
            @string.AppendFormatLine("MachineName: {0}", Environment.MachineName);
            if (OperatingSystem2.IsLinux)
            {
                @string.AppendFormatLine("$HOME: {0}", Environment.GetEnvironmentVariable("HOME"));
            }

            var folders = Enum2.GetAll<Environment.SpecialFolder>()
                .Select(x => (int)x)
                .Distinct()
                .Select(x => (Environment.SpecialFolder)x);
            foreach (var folder in folders)
            {
                @string.AppendFormatLine("{1}: {0}", Environment.GetFolderPath(folder), folder);
            }

            var platformS = DI.Get<IPlatformService>();
            @string.AppendFormatLine("SteamDirPath: {0}", platformS.GetSteamDirPath());
            @string.AppendFormatLine("SteamProgramPath: {0}", platformS.GetSteamProgramPath());
            @string.AppendFormatLine("LastSteamLoginUserName: {0}", platformS.GetLastSteamLoginUserName());

            (byte[] key, byte[] iv) = platformS.MachineSecretKey;
            @string.AppendFormatLine("MachineSecretKey.key: {0}", key.Base64UrlEncode());
            @string.AppendFormatLine("MachineSecretKey.iv: {0}", iv.Base64UrlEncode());

            @string.AppendFormatLine("X509Store My: {0}", string.Join(",", GetCertificates(new X509Store(StoreName.My, StoreLocation.CurrentUser)).Select(x => x.FriendlyName).ToArray()));
            @string.AppendFormatLine("X509Store Root: {0}", string.Join(",", GetCertificates(new X509Store(StoreName.Root, StoreLocation.CurrentUser)).Select(x => x.FriendlyName).ToArray()));
            @string.AppendFormatLine("X509Store CertificateAuthority: {0}", string.Join(",", GetCertificates(new X509Store(StoreName.CertificateAuthority, StoreLocation.CurrentUser)).Select(x => x.FriendlyName).ToArray()));
            var stopwatch = Stopwatch.StartNew();

            try
            {
                await TestSecurityStorage();
                @string.AppendLine("TestSecurityStorage: OK");
            }
            catch (Exception e)
            {
                @string.AppendLine("TestSecurityStorage: Error");
                @string.AppendLine(e.ToString());
            }
            finally
            {
                stopwatch.Stop();
                @string.AppendFormatLine("ElapsedMilliseconds: {0}ms", stopwatch.ElapsedMilliseconds);
            }

            var repository = DI.Get<IGameAccountPlatformAuthenticatorRepository>();

            stopwatch.Restart();

            var secondaryPassword = "12345678";
            var value = new GAPAuthenticatorValueDTO.SteamAuthenticator
            {
                DeviceId = "dsafdsaf",
                Serial = "qwewqrwqtr",
                SteamData = "cxzvcxzvcxzv",
                SessionData = "bbbb",
            };
            var item = new GAPAuthenticatorDTO
            {
                Name = "name",
                ServerId = Guid.NewGuid(),
                Value = value,
            };

            try
            {
                await repository.InsertOrUpdateAsync(item, true, secondaryPassword);
                @string.AppendLine("GAPA_Insert: OK");
            }
            catch (Exception e)
            {
                @string.AppendLine("GAPA_Insert: Error");
                @string.AppendLine(e.ToString());
            }
            finally
            {
                stopwatch.Stop();
                @string.AppendFormatLine("ElapsedMilliseconds: {0}ms", stopwatch.ElapsedMilliseconds);
            }

            stopwatch.Restart();

            IGAPAuthenticatorDTO? item2 = null;

            try
            {
                var all = await repository.GetAllAsync(secondaryPassword);
                item2 = all.FirstOrDefault(x => x.Id == item.Id);
                @string.AppendLine("GAPA_GetAll: OK");
            }
            catch (Exception e)
            {
                @string.AppendLine("GAPA_GetAll: Error");
                @string.AppendLine(e.ToString());
            }
            finally
            {
                stopwatch.Stop();
                @string.AppendFormatLine("ElapsedMilliseconds: {0}ms", stopwatch.ElapsedMilliseconds);
            }

            if (item2 == null)
            {
                @string.AppendLine("GAPA_ITEM: NULL");
            }
            else
            {
                if (item2.Name != item.Name)
                {
                    @string.AppendLine("GAPA_ITEM_!=: Name");
                }
                if (item2.ServerId != item.ServerId)
                {
                    @string.AppendLine("GAPA_ITEM_!=: ServerId");
                }
                if (item2.Value is GAPAuthenticatorValueDTO.SteamAuthenticator value2)
                {
                    if (value2.DeviceId != value.DeviceId)
                    {
                        @string.AppendLine("GAPA_ITEM_!=: Value.DeviceId");
                    }
                    if (value2.Serial != value.Serial)
                    {
                        @string.AppendLine("GAPA_ITEM_!=: Value.Serial");
                    }
                    if (value2.SteamData != value.SteamData)
                    {
                        @string.AppendLine("GAPA_ITEM_!=: Value.SteamData");
                    }
                    if (value2.SessionData != value.SessionData)
                    {
                        @string.AppendLine("GAPA_ITEM_!=: Value.SessionData");
                    }
                }
                else
                {
                    @string.AppendLine("GAPA_ITEM_!=: Value");
                }
            }

            try
            {
                await repository.DeleteAsync(item.ServerId.Value);
                @string.AppendLine("GAPA_Delete: OK");
            }
            catch (Exception e)
            {
                @string.AppendLine("GAPA_Delete: Error");
                @string.AppendLine(e.ToString());
            }

            var options = DI.Get<IOptions<AppSettings>>();
            string embeddedAes;
            try
            {
                embeddedAes = options.Value.Aes.ToString();
            }
            catch (Exception e)
            {
                embeddedAes = e.ToString();
            }
            @string.AppendFormatLine("EmbeddedAes: {0}", embeddedAes);

#if DEBUG
            DI.Get_Nullable<ITestAppCenter>()?.Test(@string);
#endif

            DebugString += @string.ToString() + Environment.NewLine;
        }

        public void ShowDialogButton_Click()
        {
            //#if DEBUG
            //            if (OperatingSystem2.IsWindows)
            //            {
            //                //IPCTest();
            //                FileShareTest();
            //            }
            //#endif
            INotificationService.Instance.Notify("测试Test🎆🎇→→", NotificationType.Announcement);
            ShowDialogButton_Click1();
        }

#if DEBUG
        public void IPCTest()
        {
            if (IApplication.ProgramPath.EndsWith(FileEx.EXE))
            {
                var consoleProgramPath = IApplication.ProgramPath.Substring(0, IApplication.ProgramPath.Length - FileEx.EXE.Length) + ".Console" + FileEx.EXE;
                if (File.Exists(consoleProgramPath))
                {
                    var pipeClient = new Process();
                    //pipeClient.StartInfo.FileName = "runas.exe";
                    pipeClient.StartInfo.FileName = consoleProgramPath;
                    using (var pipeServer = new AnonymousPipeServerStream(PipeDirection.Out, HandleInheritability.Inheritable))
                    {
                        DebugString = string.Format("[SERVER] Current TransmissionMode: {0}.", pipeServer.TransmissionMode);

                        // Pass the client process a handle to the server.

                        var connStr = pipeServer.GetClientHandleAsString();
                        connStr = Serializable.SMPB64U(connStr);
                        //pipeClient.StartInfo.Arguments = $"/trustlevel:0x20000 \"\"{consoleProgramPath}\" ipc -key \"{connStr}\"\"";
                        pipeClient.StartInfo.Arguments = $"ipc -key \"{connStr}\"";
                        pipeClient.StartInfo.UseShellExecute = false;
                        pipeClient.Start();

                        pipeServer.DisposeLocalCopyOfClientHandle();

                        try
                        {
                            // Read user input and send that to the client process.
                            using var sw = new StreamWriter(pipeServer);
                            sw.AutoFlush = true;
                            // Send a 'sync message' and wait for client to receive it.
                            sw.WriteLine("SYNC");
                            pipeServer.WaitForPipeDrain();
                            // Send the console input to the client process.
                            //Console.Write("[SERVER] Enter text: ");
                            //sw.WriteLine(Console.ReadLine());
                            sw.WriteLine("[SERVER] Enter text: ");
                        }
                        // Catch the IOException that is raised if the pipe is broken
                        // or disconnected.
                        catch (IOException e)
                        {
                            DebugString += Environment.NewLine + string.Format("[SERVER] Error: {0}", e.Message);
                        }
                    }
                    pipeClient.WaitForExit();
                    pipeClient.Close();
                    DebugString += Environment.NewLine + "[SERVER] Client quit. Server terminating.";
                }
            }
        }

        public void FileShareTest()
        {
            if (IApplication.ProgramPath.EndsWith(FileEx.EXE))
            {
                var consoleProgramPath = IApplication.ProgramPath.Substring(0, IApplication.ProgramPath.Length - FileEx.EXE.Length) + ".Console" + FileEx.EXE;
                if (File.Exists(consoleProgramPath))
                {
                    var pipeClient = new Process();
                    pipeClient.StartInfo.FileName = "runas.exe";

                    var tempFileDirectoryName = IOPath.CacheDirectory;
                    var tempFileName = Path.GetFileName(Path.GetTempFileName());
                    var tempFilePath = Path.Combine(tempFileDirectoryName, tempFileName);
                    IOPath.FileIfExistsItDelete(tempFilePath);

                    using var watcher = new FileSystemWatcher(tempFileDirectoryName, tempFileName)
                    {
                        NotifyFilter = NotifyFilters.Attributes
                            | NotifyFilters.CreationTime
                            | NotifyFilters.DirectoryName
                            | NotifyFilters.FileName
                            | NotifyFilters.LastAccess
                            | NotifyFilters.LastWrite
                            | NotifyFilters.Security
                            | NotifyFilters.Size,
                    };

                    var connStr = tempFilePath;
                    connStr = Serializable.SMPB64U(connStr);
                    pipeClient.StartInfo.Arguments = $"/trustlevel:0x20000 \"\"{consoleProgramPath}\" ipc2 -key \"{connStr}\"\"";
                    pipeClient.StartInfo.UseShellExecute = false;
                    pipeClient.Start();

                    pipeClient.WaitForExit();
                    pipeClient.Close();

                    watcher.WaitForChanged(WatcherChangeTypes.Created, 950);
                    if (File.Exists(tempFilePath))
                    {
                        var value = File.ReadAllText(tempFilePath);
                        File.Delete(tempFilePath);
                        DebugString = value;
                    }
                }
            }
        }
#endif

        public async void ShowDialogButton_Click1()
        {
#if DEBUG
            TextBoxWindowViewModel vm = new()
            {
                Title = "Title",
                InputType = TextBoxWindowViewModel.TextBoxInputType.ReadOnlyText,
                Description = @"Steam++ v1.1.2   
                                        2021-01-29
                                        更新内容
                                        1、新增账号切换的状态栏右下角登录新账号功能
                                        2、新增实时刷新获取Steam新登录的账号数据功能
                                        3、新增FAQ常见问题疑难解答文本，可以在关于中找到它
                                        4、优化配置文件备份机制，如果配置文件出错会提示还原上次读取成功的配置
                                        5、优化错误日志记录，现在它更详细了
                                        6、修复谷歌验证码代理方式为全局跳转recatpcha
                                        7、修复配置文件加载时提示根元素错误
                                        8、修复某些情况下开机自启失效问题",
            };
            await TextBoxWindowViewModel.ShowDialogAsync(vm);
            //await LoginOrRegisterWindowViewModel.FastLoginOrRegisterAsync();
#endif

            //DI.Get<IDesktopPlatformService>().OpenDesktopIconsSettings();

            //ToastService.Current.Notify("中文测试繁體測試🎉🧨🎇🎆🎄🖼🖼🖼🖼");
            //DebugString += ToastService.Current.Message + Environment.NewLine;
            //DebugString += ToastService.Current.IsVisible + Environment.NewLine;

            //await IWindowManager .Instance.Show(typeof(object), CustomWindow.NewVersion);

            //    var r = await MessageBox.ShowAsync(@"Steam++ v1.1.2   2021-01-29
            //更新内容
            //1、新增账号切换的状态栏右下角登录新账号功能
            //2、新增实时刷新获取Steam新登录的账号数据功能
            //3、新增FAQ常见问题疑难解答文本，可以在关于中找到它
            //4、优化配置文件备份机制，如果配置文件出错会提示还原上次读取成功的配置
            //5、优化错误日志记录，现在它更详细了
            //6、修复谷歌验证码代理方式为全局跳转recatpcha
            //7、修复配置文件加载时提示根元素错误
            //8、修复某些情况下开机自启失效问题", "Steam++", MessageBox.Button.OKCancel);

            //    DebugString += r + Environment.NewLine;

            //.ContinueWith(s => DebugString += s.Result + Environment.NewLine);
        }

        static async Task TestSecurityStorage()
        {
            await ISecureStorage.Instance.SetAsync("↑↑", Encoding.UTF8.GetBytes("↓↓"));

            var left_top_ = await ISecureStorage.Instance.GetAsync<byte[]>("↑↑");

            var left_top = Encoding.UTF8.GetString(left_top_.ThrowIsNull("↑-key"));

            if (left_top != "↓↓")
            {
                throw new Exception("↓↓");
            }

            await ISecureStorage.Instance.SetAsync<string>("←←", "→→");

            var left_left = await ISecureStorage.Instance.GetAsync<string>("←←");

            if (left_left != "→→")
            {
                throw new Exception("→→");
            }

            await ISecureStorage.Instance.SetAsync("aa", "bb");

            var left_aa = await ISecureStorage.Instance.GetAsync("aa");

            if (left_aa != "bb")
            {
                throw new Exception("bb");
            }

            var dict = new Dictionary<string, string> {
                { "🎈✨", "🎆🎇" },
                { "✨🎊", "🎃🎑" },
            };

            await ISecureStorage.Instance.SetAsync("dict", dict);

            var left_dict = await ISecureStorage.Instance.GetAsync<Dictionary<string, string>>("dict");

            if (left_dict == null)
            {
                throw new Exception("dict is null.");
            }

            if (left_dict.Count != dict.Count)
            {
                throw new Exception("dict Count !=.");
            }
        }

        public async void TestServerApiButton_Click()
        {
            var infoStr = await ICloudServiceClient.Instance.Info();
            DebugString += infoStr + Environment.NewLine;
        }

        /// <summary>
        /// 用于单元测试输出文本的JSON序列化与反序列化
        /// </summary>
        internal static class Serializable2
        {
            static readonly Lazy<JsonSerializerSettings> mDebugViewTextSettings = new(GetDebugViewTextSettings);

            static JsonSerializerSettings GetDebugViewTextSettings() => new()
            {
                ContractResolver = new IgnoreJsonPropertyContractResolver(),
                Converters = new List<JsonConverter>
                {
                    new StringEnumConverter(),
                },
            };

            /// <summary>
            /// 将忽略 <see cref="JsonPropertyAttribute"/> 属性
            /// </summary>
            sealed class IgnoreJsonPropertyContractResolver : DefaultContractResolver
            {
                protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
                {
                    var result = base.CreateProperties(type, memberSerialization);
                    foreach (var item in result)
                    {
                        item.PropertyName = item.UnderlyingName;
                    }
                    return result;
                }
            }

            /// <summary>
            /// 序列化 JSON 模型，使用原键名，仅调试使用
            /// </summary>
            /// <param name="value"></param>
            /// <returns></returns>
            public static string S(object? value)
                => SerializeObject(value, Formatting.Indented, mDebugViewTextSettings.Value);

            /// <summary>
            /// 反序列化 JSON 模型，使用原键名，仅调试使用
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="value"></param>
            /// <returns></returns>
            [return: MaybeNull]
            public static T D<T>(string value)
                => DeserializeObject<T>(value, mDebugViewTextSettings.Value);
        }

        public async void TestFontsButton_Click()
        {
            //IViewModelManager.Instance.ShowTaskBarWindow(0, 0);
            //InstalledFontCollection ifc = new();
            StringBuilder s = new();
            //foreach (var item in ifc.Families)
            //{
            //    s.AppendLine(item.GetName(R.Culture.LCID));
            //}
            var testdomain = "steampp.net";
            var DnsAnalysis = IDnsAnalysisService.Instance;

            s.AppendLine($"<Support IPV6>: {await DnsAnalysis.GetIsIpv6Support()}");
            s.AppendLine($"<IPV6> ipv6.rmbgame.net: {(await DnsAnalysis.AnalysisDomainIp("ipv6.rmbgame.net"))?.First()}");
            s.AppendLine($"<SystemDNS>{testdomain}: {(await DnsAnalysis.AnalysisDomainIp(testdomain))?.First()}");
            s.AppendLine($"<AliDNS>{testdomain}: {(await DnsAnalysis.AnalysisDomainIpByAliDns(testdomain))?.First()}");
            s.AppendLine($"<DNSPod>{testdomain}: {(await DnsAnalysis.AnalysisDomainIpByDnspod(testdomain))?.First()}");
            s.AppendLine($"<114DNS>{testdomain}: {(await DnsAnalysis.AnalysisDomainIpBy114Dns(testdomain))?.First()}");
            s.AppendLine($"<GoogleDNS>{testdomain}: {(await DnsAnalysis.AnalysisDomainIpByGoogleDns(testdomain))?.First()}");
            s.AppendLine($"<CloudflareDNS>{testdomain}: {(await DnsAnalysis.AnalysisDomainIpByCloudflare(testdomain))?.First()}");

            DebugString += s.ToString();
        }

#if DEBUG
        public interface ITestAppCenter
        {
            void Test(StringBuilder @string);
        }
#endif
    }
}
#endif