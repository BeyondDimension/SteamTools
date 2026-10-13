#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
using dotnetCampus.Ipc.CompilerServices.GeneratedProxies;
using System.CommandLine;
using Command = System.CommandLine.Command;
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
        var debug_args = new Option<string>("-args")
        {
            DefaultValueFactory = _ => "",
            Description = "测试参数",
        };
        var debug = new Command("debug", "调试")
        {
            debug_args,
        };
        debug.SetAction(parseResult =>
        {
            var args = parseResult.GetValue(debug_args);

            //Console.WriteLine("-clt debug -args " + args);
            // OutputType WinExe 导致控制台输入不会显示，只能附加一个新的控制台窗口显示内容，不合适
            // 如果能取消 管理员权限要求，改为运行时管理员权限，
            // 则可尝试通过 Windows Terminal 或直接 Host 进行命令行模式
            IsMainProcess = true;
            IsConsoleLineToolProcess = false;

            RunUIApplication();
        });
        rootCommand.Subcommands.Add(debug);
#endif

        var main = new Command(command_main)
        {
        };
        main.SetAction(parseResult =>
        {
            RunUIApplication();
        });
        rootCommand.Subcommands.Add(main);

        // -clt devtools
        // -clt devtools -disable_gpu
        // -clt devtools -use_wgl
        var devtools_disable_gpu = new Option<bool>("-disable_gpu")
        {
            DefaultValueFactory = _ => false,
            Description = "禁用 GPU 硬件加速",
        };
        var devtools_use_wgl = new Option<bool>("-use_wgl")
        {
            DefaultValueFactory = _ => false,
            Description = "使用 Native OpenGL（仅 Windows）",
        };
        var devtools_use_localhost = new Option<bool>("-use_localhost")
        {
            DefaultValueFactory = _ => false,
            Description = "使用本机服务端",
        };
        var devtools_steamrun = new Option<bool>("-steamrun")
        {
            DefaultValueFactory = _ => false,
            Description = "Steam 内启动",
        };
        var devtools = new Command("devtools")
        {
            devtools_disable_gpu, devtools_use_wgl, devtools_use_localhost, devtools_steamrun,
        };
        devtools.SetAction(parseResult =>
        {
            var disable_gpu = parseResult.GetValue(devtools_disable_gpu);
            var use_wgl = parseResult.GetValue(devtools_use_wgl);
            var use_localhost = parseResult.GetValue(devtools_use_localhost);
            var steamrun = parseResult.GetValue(devtools_steamrun);

#if DEBUG
            AppSettings.UseLocalhostApiBaseUrl = use_localhost;
#endif
            IsMainProcess = true;
            IsConsoleLineToolProcess = false;
            IsSteamRun = steamrun;

            overrideLoggerMinLevel = LogLevel.Debug;
            IApplication.EnableDevtools = true;
            IApplication.DisableGPU = disable_gpu;
            IApplication.UseWgl = use_wgl;

            RunUIApplication();
        });
        rootCommand.Subcommands.Add(devtools);

        // -clt c -silence
        // -clt c -steamrun
        var common_silence = new Option<bool>("-silence")
        {
            DefaultValueFactory = _ => false,
            Description = "静默启动（不弹窗口）",
        };
        var common_steamrun = new Option<bool>("-steamrun")
        {
            DefaultValueFactory = _ => false,
            Description = "Steam 内启动",
        };
        var common = new Command("c", "common")
        {
            common_silence, common_steamrun,
        };
        common.SetAction(parseResult =>
        {
            var silence = parseResult.GetValue(common_silence);
            var steamrun = parseResult.GetValue(common_steamrun);

            IsMinimize = silence;
            IsSteamRun = steamrun;

            IsMainProcess = true;
            IsConsoleLineToolProcess = false;

            RunUIApplication();
        });
        rootCommand.Subcommands.Add(common);

        // -clt steam -account
        var steamuser_account = new Option<string>("-account")
        {
            Description = "指定对应 Steam 用户名",
        };
        var steamuser = new Command("steam", "Steam 相关操作")
        {
            steamuser_account,
        };
        steamuser.SetAction(async parseResult =>
        {
            var account = parseResult.GetValue(steamuser_account);

            if (!string.IsNullOrEmpty(account))
            {
#if WINDOWS
                if (!WindowsPlatformServiceImpl.IsPrivilegedProcess)
                {
                    // 必须使用管理员权限进行操作
                    await RunSelfAsAdministrator($"-clt steam -account {account}");
                    return;
                }
#endif

                RunUIApplication(AppServicesLevel.Steam);

                await WaitConfiguredServices;

                var steamService = ISteamService.Instance;

                var users = steamService.GetRememberUserList();

                var currentuser = users.Where(s => s.AccountName == account).FirstOrDefault();

                await steamService.TryKillSteamProcess();

                if (currentuser != null)
                {
                    currentuser.MostRecent = true;
                    steamService.UpdateLocalUserData(users);
                    await steamService.SetSteamCurrentUserAsync(account);
                }

                steamService.StartSteamWithParameter();
            }
        });
        rootCommand.Subcommands.Add(steamuser);

        // -clt app -id 282800 -achievement
        var app_id = new Option<int>("-id")
        {
            Description = "指定一个 Steam 游戏 Id",
        };
        var app_achievement = new Option<bool>("-achievement")
        {
            Description = "打开成就解锁窗口",
        };
        var app_cloudmanager = new Option<bool>("-cloudmanager")
        {
            Description = "打开云存档管理窗口",
        };
        var app_silence = new Option<bool>("-silence")
        {
            DefaultValueFactory = _ => false,
            Description = "挂运行服务，不加载窗口，内存占用更小",
        };
        var run_SteamApp = new Command("app", "运行 Steam 应用")
        {
            app_id, app_achievement, app_cloudmanager, app_silence,
        };
        run_SteamApp.SetAction(async parseResult =>
        {
            var id = parseResult.GetValue(app_id);
            var achievement = parseResult.GetValue(app_achievement);
            var cloudmanager = parseResult.GetValue(app_cloudmanager);
            var silence = parseResult.GetValue(app_silence);

            int exitCode = default;
            if (id <= 0)
                return -1;

            if (cloudmanager || achievement)
            {
                RunUIApplication(AppServicesLevel.UI |
                    AppServicesLevel.Steam |
                    AppServicesLevel.HttpClientFactory, null,
                    loadModules: AssemblyInfo.GameList);
                await WaitConfiguredServices;

                if (TryGetPlugins(out var plugins) && plugins.Any_Nullable())
                {
                    await plugins.First().OnCommandRun(id.ToString(), achievement ? nameof(achievement) : nameof(cloudmanager));
                    StartUIApplication();
                }
                else
                {
                    // 找不到插件，可能该插件已被删除
                    INotificationService.Instance.Notify(Strings.GameList + " plugin does not exist.", NotificationType.Message);
                    return 404;
                }
            }
            else
            {
                RunUIApplication(AppServicesLevel.Steam);
                await WaitConfiguredServices;

                if (SteamConnectService.Current.Initialize(id))
                {
                    await new TaskCompletionSource().Task;
                }
            }

            return exitCode;
        });
        rootCommand.Subcommands.Add(run_SteamApp);

        // -clt show -config -cert
        var show_cert = new Option<bool>("-cert")
        {
            Description = "显示当前根证书信息",
        };
        var show = new Command("show", "显示信息")
        {
            show_cert,
        };
        //show.AddOption(new Option<bool>("-config", "显示 Config.mpo 值"));
        show.SetAction(async parseResult =>
        {
            var cert = parseResult.GetValue(show_cert);

#if WINDOWS
            if (!IsDesignMode)
            {
                if (!PInvoke.Kernel32.AttachConsole(-1))
                    PInvoke.Kernel32.AllocConsole();
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
                PInvoke.Kernel32.FreeConsole();
            }
#endif
        });
        rootCommand.Subcommands.Add(show);

        //#if MACOS
        //        var macOS = new Command("macoscert", "Mac 平台 Root 权限 操作指令");
        //        macOS.AddOption(new Option<string>("-i", "安装证书 参数为用户名 因为 Root 启动无法获取指定用户名"));
        //        macOS.AddOption(new Option<string>("-d", "删除证书"));
        //        macOS.Handler = CommandHandler.Create((string i, string d) =>
        //        {
        //            if (string.IsNullOrWhiteSpace(i) || string.IsNullOrWhiteSpace(i))
        //                return (int)CommandExitCode.HttpStatusBadRequest;
        //            if (!string.IsNullOrWhiteSpace(i))
        //            {

        //                return (int)CommandExitCode.HttpStatusBadRequest;
        //            }
        //            if (!string.IsNullOrWhiteSpace(d))
        //            {
        //                using var rootCert = X509CertificatePackable.CreateX509Certificate2(CertificateConstants.DefaultPfxFilePath, (string?)null, X509KeyStorageFlags.Exportable);
        //                return MacCatalystPlatformServiceImpl.RemoveCertificate(rootCert);
        //            }
        //            return (int)CommandExitCode.HttpStatusBadRequest;
        //        });
        //#endif

#if LINUX
        // -clt linux -i or -d AppDataDirectory
        //Linux 可以自定义用户文件夹
        var linux_ceri = new Option<string>("-ceri")
        {
            Description = "安装证书 参数为 AppDataDirectory",
        };
        var linux_cerd = new Option<string>("-cerd")
        {
            Description = "删除证书 参数为 AppDataDirectory",
        };
        var linux = new Command("linux", "Linux 平台 Root 权限 操作指令")
        {
            linux_ceri, linux_cerd,
        };
        //linux.AddOption(new Option<bool>("-bindProt", "执行允许绑定 443"));
        linux.SetAction(async parseResult =>
        {
            var ceri = parseResult.GetValue(linux_ceri);
            var cerd = parseResult.GetValue(linux_cerd);

            if (string.IsNullOrWhiteSpace(ceri) || string.IsNullOrWhiteSpace(ceri))
                return (int)CommandExitCode.HttpStatusBadRequest;
            if (!string.IsNullOrWhiteSpace(ceri))
            {
                LinuxPlatformServiceImpl.TrustRootCertificateCore(Path.Combine(ceri, CertificateConstants.CerFileName));
                return (int)CommandExitCode.HttpStatusCodeOk;
            }
            if (!string.IsNullOrWhiteSpace(cerd))
            {
                LinuxPlatformServiceImpl.RemoveCertificate(Path.Combine(cerd, CertificateConstants.CerFileName));
                return (int)CommandExitCode.HttpStatusCodeOk;
            }
            return (int)CommandExitCode.HttpStatusBadRequest;
        });
        rootCommand.Subcommands.Add(linux);

        // -clt nixos -gcert [-path <dir>]
        // NixOS 不可变系统适配：手动生成/导出根证书到指定目录（默认软件执行目录），
        // 供用户复制到 nixos-config 的 ssl 目录并由 security.pki.certificateFiles 声明式安装。
        var nixos_gcert = new Option<bool>("-gcert", "生成根证书到指定目录");
        var nixos_path = new Option<string>("-path", "输出目录（默认：软件执行目录）");
        var nixos = new Command("nixos", "NixOS 平台操作指令")
        {
            nixos_gcert, nixos_path,
        };
        nixos.SetAction(parseResult =>
        {
            var gcert = parseResult.GetValue(nixos_gcert);
            var path = parseResult.GetValue(nixos_path);
            if (!gcert)
                return (int)CommandExitCode.HttpStatusBadRequest;
            // 默认输出到软件执行目录（nix 临时安装目录），只读时回退到 AppData
            var targetDirectory = string.IsNullOrWhiteSpace(path) ? IOPath.BaseDirectory : path;
            var cerFilePath = LinuxPlatformServiceImpl.GenerateRootCertificateToDirectory(targetDirectory);
            if (cerFilePath == null)
                return (int)CommandExitCode.HttpStatusBadRequest;
            Console.WriteLine($"NixOS: 根证书已生成: {cerFilePath}");
            Console.WriteLine("请将 SteamTools.Certificate.cer 复制到 nixos-config 仓库的 ssl/ 目录，");
            Console.WriteLine("并在系统配置中通过 security.pki.certificateFiles 引入。");
            return (int)CommandExitCode.HttpStatusCodeOk;
        });
        rootCommand.Subcommands.Add(nixos);
#endif

        // -clt proxy -on
        var proxy_on = new Option<bool>("-on")
        {
            Description = "开启代理服务",
        };
        var proxy_off = new Option<bool>("-off")
        {
            Description = "关闭代理服务",
        };
        var proxy = new Command(key_proxy, "启用代理服务，静默启动（不弹窗口）")
        {
            proxy_on, proxy_off,
        };
        proxy.SetAction(async parseResult =>
        {
            var on = parseResult.GetValue(proxy_on);
            var off = parseResult.GetValue(proxy_off);

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
        rootCommand.Subcommands.Add(proxy);

        // -clt shutdown
        var shutdown = new Command(key_shutdown, "安全结束正在运行的程序")
        {
        };
        shutdown.SetAction(async parseResult =>
        {
            InitSingleInstancePipeline(() => key_shutdown);
        });
        rootCommand.Subcommands.Add(shutdown);

#if WINDOWS
        // -clt sudo
        var sudo_n = new Option<string>(IPlatformService.IPCRoot.args_PipeName)
        {
            Description = "IPC 管道名",
        };
        var sudo_p = new Option<string>(IPlatformService.IPCRoot.args_ProcessId)
        {
            Description = "主进程 Id",
        };
        var sudo = new Command(IPlatformService.IPCRoot.CommandName, "使用管理员权限启动 IPC 服务进程")
        {
            sudo_n, sudo_p,
        };
        sudo.SetAction(async parseResult =>
        {
            var n = parseResult.GetValue(sudo_n);
            var p = parseResult.GetValue(sudo_p);

            if (string.IsNullOrWhiteSpace(n) || p == default)
                return 400;
            if (!WindowsPlatformServiceImpl.IsPrivilegedProcess)
                return 401;
            if (ModuleName != IPlatformService.IPCRoot.moduleName)
                return 402;

            if (!int.TryParse(p, out var pid) || pid <= 0)
                return (int)CommandExitCode.NotFoundMainProcessId;

#if DEBUG
            Log.Info(nameof(IPCSubProcessServiceImpl), "客户端正在校验服务端进程，{pid}", pid);
#endif
            // 验证要连接的服务端进程是否为自己的程序
            var ckPid = IPCSubProcessServiceImpl.CheckBePid(pid);
            if (!ckPid)
                return (int)CommandExitCode.MainProcessIdIncorrect;


            RunUIApplication(AppServicesLevel.IPCRoot | AppServicesLevel.Hosts);
            await WaitConfiguredServices;

            try
            {
                var exitCode = await IPCSubProcessService.MainAsync(IPlatformService.IPCRoot.moduleName, null, ConfigureServices, static ipcProvider =>
                {
                    // 添加平台服务（供主进程的 IPC 远程访问）
                    var platformService = IPlatformService.Instance;
                    IHostsFileService hostsFileService = IHostsFileService.Constants.Instance;
#if DEBUG
                    Console.WriteLine(platformService.GetDebugString());
#endif
                    ipcProvider.CreateIpcJoint<IPCPlatformService>(platformService);
                    ipcProvider.CreateIpcJoint(hostsFileService);

                    var s = Startup.Instance;
                    if (s.TryGetPlugins(out var plugins))
                    {

                        foreach (var plugin in plugins)
                        {
                            try
                            {
                                plugin.ConfigureServices(ipcProvider!, s);
                            }
                            catch (Exception ex)
                            {
                                //GlobalExceptionHandler.Handler(ex, $"{plugin.UniqueEnglishName}.ConfigureRequiredServices");
                            }
                        }
                    }

                }, [n, p.ToString()]);

                return exitCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Console.ReadLine();
                return 500;
            }
        });
        rootCommand.Subcommands.Add(sudo);
#endif

        // -clt plugins -l {AppServicesLevel} -m {插件名} -n {PipeName} -p {ProcessId} -a {插件需要解析的参数}
        var plugins_l = new Option<uint>("-l")
        {
            Description = "AppServicesLevel",
        };
        var plugins_m = new Option<string>("-m")
        {
            Description = "需要加载的模块名称",
        };
        var plugins_a = new Option<string>("-a")
        {
            Description = "需要解析的参数，使用 HttpUtility.UrlDecode 解码",
        };
        var plugins_n = new Option<string>(IPlatformService.IPCRoot.args_PipeName)
        {
            Description = "IPC 管道名",
        };
        var plugins_p = new Option<string>(IPlatformService.IPCRoot.args_ProcessId)
        {
            Description = "主进程 Id",
        };
        var plugins = new Command("plugins", "插件使用的 IPC 服务进程")
        {
            plugins_l, plugins_m, plugins_a, plugins_n, plugins_p,
        };
        plugins.SetAction(async parseResult =>
        {
            var l = parseResult.GetValue(plugins_l);
            string m = parseResult.GetValue(plugins_m) ?? "";
            var a = parseResult.GetValue(plugins_a);
            var n = parseResult.GetValue(plugins_n);
            var p = parseResult.GetValue(plugins_p);

            if (string.IsNullOrWhiteSpace(n) ||
                string.IsNullOrWhiteSpace(p))
                return (int)CommandExitCode.HttpStatusBadRequest;

            var level = (AppServicesLevel)l;
            RunUIApplication(level, loadModules: m);
            await WaitConfiguredServices;

            if (!TryGetPlugins(out var plugins))
                return (int)CommandExitCode.GetPluginsFail;

            var plugin = plugins.FirstOrDefault(x => x.UniqueEnglishName == m);
            if (plugin == null)
                return (int)CommandExitCode.GetPluginFail;

            var exitCode = await plugin.RunSubProcessMainAsync(m, n, p, a);
            return exitCode;
        });
        rootCommand.Subcommands.Add(plugins);

        //        // -clt types
        //        var types = new Command("types", "显示所有类型")
        //        {
        //            Handler = CommandHandler.Create(() =>
        //            {
        //                int exitCode = default;
        //                using var assemblies_stream = new FileStream(Path.Combine(
        //                    IOPath.CacheDirectory, "assemblies.txt"),
        //                    FileMode.OpenOrCreate,
        //                    FileAccess.Write,
        //                    FileShare.ReadWrite | FileShare.Delete);
        //                using var types_stream = new FileStream(Path.Combine(
        //                    IOPath.CacheDirectory, "types.txt"),
        //                    FileMode.OpenOrCreate,
        //                    FileAccess.Write,
        //                    FileShare.ReadWrite | FileShare.Delete);
        //                try
        //                {
        //                    var assemblies_ = AppDomain.CurrentDomain.GetAssemblies();
        //                    HashSet<Assembly> assemblies = new(assemblies_);
        //                    AppDomain.CurrentDomain.AssemblyLoad += (_, args) =>
        //                    {
        //                        var assembly = args.LoadedAssembly;
        //                        if (assemblies.Add(assembly))
        //                        {
        //                            ShowAssembly(assembly);
        //                        }
        //                    };
        //                    foreach (var assembly in assemblies_)
        //                    {
        //                        ShowAssembly(assembly);
        //                    }

        //                    void ShowAssembly(Assembly assembly)
        //                    {
        //                        lock (types_stream)
        //                        {
        //                            var assemblyName = assembly.FullName;
        //                            if (!string.IsNullOrWhiteSpace(assemblyName))
        //                            {
        //                                assemblies_stream.Write(Encoding.UTF8.GetBytes(assemblyName));
        //                                assemblies_stream.Write("\r\n"u8);
        //                            }
        //                            var types = assembly.GetTypes();
        //                            foreach (var type in types)
        //                            {
        //                                var fullName = type.FullName;
        //                                if (!string.IsNullOrWhiteSpace(fullName))
        //                                {
        //                                    types_stream.Write(Encoding.UTF8.GetBytes(fullName));
        //                                    types_stream.Write("\r\n"u8);
        //                                }
        //                            }
        //                        }
        //                    }
        //                    return exitCode = (int)CommandExitCode.HttpStatusCodeOk;
        //                }
        //                catch (Exception ex)
        //                {
        //                    Console.WriteLine(ex);
        //                    return exitCode = (int)CommandExitCode.HttpStatusCodeInternalServerError;
        //                }
        //                finally
        //                {
        //                    types_stream.SetLength(types_stream.Position);
        //                    types_stream.Flush();
        //                    types_stream.Dispose();
        //                    assemblies_stream.SetLength(assemblies_stream.Position);
        //                    assemblies_stream.Flush();
        //                    assemblies_stream.Dispose();
        //                    Console.WriteLine($"ExitCode: {exitCode}");
        //#if DEBUG
        //                    Console.ReadLine();
        //#endif
        //                }
        //            }),
        //        };
        //        rootCommand.Subcommands.Add(types);
    }
#endif

#if WINDOWS
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    async ValueTask<Process?> RunSelfAsAdministrator(string arguments)
    {
        var processPath = Environment.ProcessPath;
        processPath.ThrowIsNull();
        var process = await WindowsPlatformServiceImpl.StartAsAdministrator(processPath, arguments);
        return process;
    }
#endif
}