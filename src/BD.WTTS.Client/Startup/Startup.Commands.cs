#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using dotnetCampus.Ipc.CompilerServices.GeneratedProxies;
#endif

// ReSharper disable once CheckNamespace
namespace BD.WTTS;

partial class Startup // 自定义控制台命令参数
{
#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void ConfigureCommands(RootCommand rootCommand)
    {
#if DEBUG
        // -clt debug -args 730
        var debug = new Command("debug", "调试");
        debug.AddOption(new Option<string>("-args", () => "", "测试参数"));
        debug.Handler = CommandHandler.Create((string args) => // 参数名与类型要与 Option 中一致！
        {
            //Console.WriteLine("-clt debug -args " + args);
            // OutputType WinExe 导致控制台输入不会显示，只能附加一个新的控制台窗口显示内容，不合适
            // 如果能取消 管理员权限要求，改为运行时管理员权限，
            // 则可尝试通过 Windows Terminal 或直接 Host 进行命令行模式
            IsMainProcess = true;
            IsConsoleLineToolProcess = false;

            RunUIApplication();
        });
        rootCommand.AddCommand(debug);
#endif

        var main = new Command(command_main)
        {
            Handler = CommandHandler.Create(() =>
            {
                RunUIApplication();
            }),
        };
        rootCommand.AddCommand(main);

        // -clt devtools
        // -clt devtools -disable_gpu
        // -clt devtools -use_wgl
        var devtools = new Command("devtools");
        devtools.AddOption(new Option<bool>("-disable_gpu", () => false, "禁用 GPU 硬件加速"));
        devtools.AddOption(new Option<bool>("-use_wgl", () => false, "使用 Native OpenGL（仅 Windows）"));
        devtools.AddOption(new Option<bool>("-use_localhost", () => false, "使用本机服务端"));
        devtools.Handler = CommandHandler.Create((bool disable_gpu, bool use_wgl, bool use_localhost) =>
        {
#if DEBUG
            AppSettings.UseLocalhostApiBaseUrl = use_localhost;
#endif
            IsMainProcess = true;
            IsConsoleLineToolProcess = false;

            overrideLoggerMinLevel = LogLevel.Debug;
            IApplication.EnableDevtools = true;
            IApplication.DisableGPU = disable_gpu;
            IApplication.UseWgl = use_wgl;

            RunUIApplication();
        });
        rootCommand.AddCommand(devtools);

        // -clt c -silence
        var common = new Command("c", "common");
        common.AddOption(new Option<bool>("-silence", "静默启动（不弹窗口）"));
        common.Handler = CommandHandler.Create((bool silence) =>
        {
            IsMinimize = silence;

            IsMainProcess = true;
            IsConsoleLineToolProcess = false;

            RunUIApplication();
        });
        rootCommand.AddCommand(common);

        // -clt steam -account
        var steamuser = new Command("steam", "Steam 相关操作");
        steamuser.AddOption(new Option<string>("-account", "指定对应 Steam 用户名"));
        steamuser.Handler = CommandHandler.Create((string account) =>
        {
            if (!string.IsNullOrEmpty(account))
            {
                Task.Factory.StartNew(async () =>
                {
                    await WaitConfiguredServices;

                    var steamService = ISteamService.Instance;

                    var users = steamService.GetRememberUserList();

                    var currentuser = users.Where(s => s.AccountName == account).FirstOrDefault();

                    steamService.TryKillSteamProcess();

                    if (currentuser != null)
                    {
                        currentuser.MostRecent = true;
                        steamService.UpdateLocalUserData(users);
                        steamService.SetCurrentUser(account);
                    }

                    steamService.StartSteamWithParameter();
                });
                RunUIApplication(AppServicesLevel.Steam);
            }
        });
        rootCommand.AddCommand(steamuser);

        // -clt app -id 632360
        var run_SteamApp = new Command("app", "运行 Steam 应用");
        run_SteamApp.AddOption(new Option<int>("-id", "指定一个 Steam 游戏 Id"));
        run_SteamApp.AddOption(new Option<bool>("-achievement", "打开成就解锁窗口"));
        run_SteamApp.AddOption(new Option<bool>("-cloudmanager", "打开云存档管理窗口"));
        run_SteamApp.AddOption(new Option<bool>("-silence", "挂运行服务，不加载窗口，内存占用更小"));
        run_SteamApp.Handler = CommandHandler.Create((int id, bool achievement, bool cloudmanager) =>
        {
            if (id <= 0)
                return;
            Task.Factory.StartNew(async () =>
            {
                await WaitConfiguredServices;

                if (cloudmanager)
                {
                    //IViewModelManager.Instance.InitCloudManageMain(id);
                }
                else if (achievement)
                {
                    //IViewModelManager.Instance.InitUnlockAchievement(id);
                }
                else
                {
                    SteamConnectService.Current.Initialize(id);
                    TaskCompletionSource tcs = new();
                    await tcs.Task;
                }
            });

            AppServicesLevel level;
            if (cloudmanager)
            {
                level = AppServicesLevel.UI |
                    AppServicesLevel.Steam |
                    AppServicesLevel.HttpClientFactory;
            }
            else if (achievement)
            {
                level = AppServicesLevel.UI |
                    AppServicesLevel.Steam |
                    AppServicesLevel.HttpClientFactory;
            }
            else
            {
                level = AppServicesLevel.Steam;
            }
            RunUIApplication(level);
        });
        rootCommand.AddCommand(run_SteamApp);

        // -clt show -config -cert
        var show = new Command("show", "显示信息");
        //show.AddOption(new Option<bool>("-config", "显示 Config.mpo 值"));
        show.AddOption(new Option<bool>("-cert", "显示当前根证书信息"));
        show.Handler = CommandHandler.Create((/*bool config,*/ bool cert) =>
        {
#if WINDOWS
            if (!IsDesignMode)
            {
                if (!Interop.Kernel32.AttachConsole())
                    Interop.Kernel32.AllocConsole();
            }
#endif

            //if (config)
            //{
            //    Console.WriteLine("Config: ");
            //    try
            //    {
            //        SettingsHost.Load();
            //        var configValue = SettingsHostBase.Local.ToJsonString();
            //        Console.WriteLine(configValue);
            //    }
            //    catch (Exception e)
            //    {
            //        Console.WriteLine(e.ToString());
            //    }
            //    Console.WriteLine();
            //}

            if (cert)
            {
                Console.WriteLine("RootCertificate: ");
                try
                {
                    using X509Certificate2 rootCert = new(CertificateConstants.DefaultPfxFilePath, (string?)null, X509KeyStorageFlags.Exportable);
                    Console.WriteLine("Subject：");
                    Console.WriteLine(rootCert.Subject);
                    Console.WriteLine("SerialNumber：");
                    Console.WriteLine(rootCert.SerialNumber);
                    Console.WriteLine("PeriodValidity：");
                    Console.Write(rootCert.GetEffectiveDateString());
                    Console.Write(" ~ ");
                    Console.Write(rootCert.GetExpirationDateString());
                    Console.WriteLine();
                    Console.WriteLine("SHA256：");
                    Console.WriteLine(rootCert.GetCertHashStringCompat(HashAlgorithmName.SHA256));
                    Console.WriteLine("SHA1：");
                    Console.WriteLine(rootCert.GetCertHashStringCompat(HashAlgorithmName.SHA1));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
                Console.WriteLine();
            }

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();

#if WINDOWS
            if (!IsDesignMode)
            {
                Interop.Kernel32.FreeConsole();
            }
#endif
        });
        rootCommand.AddCommand(show);

        // -clt proxy -on
        var proxy = new Command(key_proxy, "启用代理服务，静默启动（不弹窗口）");
        proxy.AddOption(new Option<bool>("-on", "开启代理服务"));
        proxy.AddOption(new Option<bool>("-off", "关闭代理服务"));
        proxy.Handler = CommandHandler.Create((bool on, bool off) =>
        {
            // 开关为可选，都不传或都为 false 则执行 toggle
            // 检查当前是否已启动程序
            // 已启动，进行 IPC 通信传递开关
            // 未启动，执行类似 -silence 逻辑
            IsMinimize = true;
            IsProxyService = true;
            ProxyServiceStatus = on ? OnOffToggle.On : (off ? OnOffToggle.Off : OnOffToggle.Toggle);
            var sendMessage = () => $"{key_proxy} {ProxyServiceStatus}";
            RunUIApplication(sendMessage: sendMessage);
        });
        rootCommand.AddCommand(proxy);

        // -clt ayaneo -path
        var ayaneo = new Command("ayaneo", "生成 ayaneo 数据在指定位置");
        ayaneo.AddOption(new Option<string>("-path", "json 生成路径"));
        ayaneo.Handler = CommandHandler.Create(async (string path) =>
        {
            RunUIApplication(AppServicesLevel.Steam);
            await WaitConfiguredServices;

            var steamService = ISteamService.Instance;
            var users = steamService.GetRememberUserList();

            IReadOnlyDictionary<long, string?>? accountRemarks =
                Ioc.Get<IPartialGameAccountSettings>()?.AccountRemarks;

            var sUsers = users.Select(s =>
            {
                if (accountRemarks?.TryGetValue(s.SteamId64, out var remark) == true &&
                   !string.IsNullOrEmpty(remark))
                    s.Remark = remark;

                var title = s.SteamNickName ?? s.SteamId64.ToString(CultureInfo.InvariantCulture);
                if (!string.IsNullOrEmpty(s.Remark))
                    title = s.SteamNickName + "(" + s.Remark + ")";

                return new
                {
                    Name = title,
                    Account = s.AccountName,
                };
            });

            var content = new
            {
                steamppPath = Environment.ProcessPath,
                steamUsers = sUsers,
            };
            if (string.IsNullOrWhiteSpace(path))
            {
                path = Path.Combine(IOPath.BaseDirectory, "steampp.json");
            }
            File.WriteAllText(path, Serializable.SJSON(content, writeIndented: true));
        });
        rootCommand.AddCommand(ayaneo);

        // -clt shutdown
        var shutdown = new Command(key_shutdown, "安全结束正在运行的程序")
        {
            Handler = CommandHandler.Create(() =>
            {
                InitSingleInstancePipeline(() => key_shutdown);
            }),
        };
        rootCommand.AddCommand(shutdown);

#if WINDOWS
        // -clt sudo
        var sudo = new Command(IPlatformService.IPCRoot.CommandName, "使用管理员权限启动 IPC 服务进程")
        {
            Handler = CommandHandler.Create(async (string n, ushort p) =>
            {
                if (string.IsNullOrWhiteSpace(n) || p == default)
                    return 400;

                if (!IPlatformService._IsAdministrator.Value)
                    return 401;

                ModuleName = IPlatformService.IPCRoot.moduleName;

                RunUIApplication(AppServicesLevel.IPCRoot | AppServicesLevel.Hosts);
                await WaitConfiguredServices;

                try
                {
                    var exitCode = await IPCSubProcessService.MainAsync(IPlatformService.IPCRoot.moduleName, ConfigureServices, static ipcProvider =>
                    {
                        // 添加平台服务（供主进程的 IPC 远程访问）
                        var platformService = IPlatformService.Instance;
#if DEBUG
                        Console.WriteLine(platformService.GetDebugString());
#endif
                        ipcProvider.CreateIpcJoint<IPCPlatformService>(platformService);
                    }, new[] { n, p.ToString() });

                    return exitCode;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    Console.ReadLine();
                    return 500;
                }
            }),
        };
        sudo.AddOption(new Option<string>(IPlatformService.IPCRoot.args_PipeName, "IPC 管道名"));
        sudo.AddOption(new Option<ushort>(IPlatformService.IPCRoot.args_ProcessId, "主进程 Id"));
        rootCommand.AddCommand(sudo);
#endif

        // -clt plugins -l {AppServicesLevel} -m {插件名} -n {PipeName} -p {ProcessId} -a {插件需要解析的参数}
        var plugins = new Command("plugins", "插件使用的 IPC 服务进程")
        {
            Handler = CommandHandler.Create(async (uint l, string m, string n, string p, string a) =>
            {
                if (string.IsNullOrWhiteSpace(n) ||
                    string.IsNullOrWhiteSpace(p))
                    return (int)CommandExitCode.HttpStatusBadRequest;

                var level = (AppServicesLevel)l;
                RunUIApplication(level, loadModules: m);
                await WaitConfiguredServices;

                if (!TryGetPlugins(out var plugins))
                    return (int)CommandExitCode.GetPluginsFail;

                var plugin = plugins.FirstOrDefault(x => x.Name == m);
                if (plugin == null)
                    return (int)CommandExitCode.GetPluginFail;

                var exitCode = await plugin.RunSubProcessMainAsync(m, n, p, a);
                return exitCode;
            }),
        };
        plugins.AddOption(new Option<uint>("-l", "AppServicesLevel"));
        plugins.AddOption(new Option<string>("-m", "需要加载的模块名称"));
        plugins.AddOption(new Option<string>("-a", "需要解析的参数，使用 HttpUtility.UrlDecode 解码"));
        plugins.AddOption(new Option<string>(IPlatformService.IPCRoot.args_PipeName, "IPC 管道名"));
        plugins.AddOption(new Option<string>(IPlatformService.IPCRoot.args_ProcessId, "主进程 Id"));
        rootCommand.AddCommand(plugins);

        // -clt types
        var types = new Command("types", "显示所有类型")
        {
            Handler = CommandHandler.Create(() =>
            {
                int exitCode = default;
                using var assemblies_stream = new FileStream(Path.Combine(
                    IOPath.CacheDirectory, "assemblies.txt"),
                    FileMode.OpenOrCreate,
                    FileAccess.Write,
                    FileShare.ReadWrite | FileShare.Delete);
                using var types_stream = new FileStream(Path.Combine(
                    IOPath.CacheDirectory, "types.txt"),
                    FileMode.OpenOrCreate,
                    FileAccess.Write,
                    FileShare.ReadWrite | FileShare.Delete);
                try
                {
                    var assemblies_ = AppDomain.CurrentDomain.GetAssemblies();
                    HashSet<Assembly> assemblies = new(assemblies_);
                    AppDomain.CurrentDomain.AssemblyLoad += (_, args) =>
                    {
                        var assembly = args.LoadedAssembly;
                        if (assemblies.Add(assembly))
                        {
                            ShowAssembly(assembly);
                        }
                    };
                    foreach (var assembly in assemblies_)
                    {
                        ShowAssembly(assembly);
                    }

                    void ShowAssembly(Assembly assembly)
                    {
                        lock (types_stream)
                        {
                            var assemblyName = assembly.FullName;
                            if (!string.IsNullOrWhiteSpace(assemblyName))
                            {
                                assemblies_stream.Write(Encoding.UTF8.GetBytes(assemblyName));
                                assemblies_stream.Write("\r\n"u8);
                            }
                            var types = assembly.GetTypes();
                            foreach (var type in types)
                            {
                                var fullName = type.FullName;
                                if (!string.IsNullOrWhiteSpace(fullName))
                                {
                                    types_stream.Write(Encoding.UTF8.GetBytes(fullName));
                                    types_stream.Write("\r\n"u8);
                                }
                            }
                        }
                    }
                    return exitCode = (int)CommandExitCode.HttpStatusCodeOk;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    return exitCode = (int)CommandExitCode.HttpStatusCodeInternalServerError;
                }
                finally
                {
                    types_stream.SetLength(types_stream.Position);
                    types_stream.Flush();
                    types_stream.Dispose();
                    assemblies_stream.SetLength(assemblies_stream.Position);
                    assemblies_stream.Flush();
                    assemblies_stream.Dispose();
                    Console.WriteLine($"ExitCode: {exitCode}");
#if DEBUG
                    Console.ReadLine();
#endif
                }
            }),
        };
        rootCommand.AddCommand(types);
    }
#endif
}