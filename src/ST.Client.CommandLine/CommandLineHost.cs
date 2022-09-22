using System.Globalization;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.Application.UI;
using System.Application.Services;
using System.Application.Settings;
using System.Application.UI.ViewModels;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace System.Application.CommandLine;

/// <summary>
/// 命令行工具(Command Line Tools/CLT)
/// </summary>
public abstract class CommandLineHost : IDisposable
{
    public const string command_main = "main";
    bool disposedValue;

    protected abstract IApplication.IDesktopProgramHost Host { get; }

    protected abstract void ConfigureServices(DILevel level, bool isTrace = false);

#if StartWatchTrace
    protected abstract void StartWatchTraceRecord(string? mark = null, bool dispose = false);
#endif

    public IApplication.SingletonInstance? AppInstance { get; protected set; }

    protected abstract void StartApplication(string[] args);

    public abstract IApplication? Application { get; }

    Task<IApplication>? _WaitForApplication;

    public Task<IApplication> WaitForApplicationAsync()
    {
        if (Application != null) return Task.FromResult(Application);
        lock (this)
        {
            if (_WaitForApplication == null)
                _WaitForApplication = Task.Run(async () =>
                {
                    while (Application == null)
                    {
                        await Task.Delay(500);
                    }
                    return Application;
                });
            return _WaitForApplication;
        }
    }

    protected abstract void SetIsMainProcess(bool value);

    protected abstract void SetIsCLTProcess(bool value);

    public int Run(string[] args)
    {
        if (args.Length == 0) args = new[] { "-h" };

        // https://docs.microsoft.com/zh-cn/archive/msdn-magazine/2019/march/net-parse-the-command-line-with-system-commandline
        var rootCommand = new RootCommand("命令行工具(Command Line Tools/CLT)");

        bool InitAppInstance(Func<string>? sendMessage = null)
        {
            var isInitAppInstanceReset = false;
        initAppInstance: AppInstance = new();
            if (!AppInstance.IsFirst)
            {
                //Console.WriteLine("ApplicationInstance.SendMessage(string.Empty);");
                if (IApplication.SingletonInstance.SendMessage(sendMessage?.Invoke() ?? ""))
                {
                    return false;
                }
                else
                {
                    if (!isInitAppInstanceReset &&
                        IApplication.SingletonInstance.TryKillCurrentAllProcess())
                    {
                        isInitAppInstanceReset = true;
                        AppInstance.Dispose();
                        goto initAppInstance;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        void MainHandler() => MainHandler_(null);
        void MainHandler_(Action? onInitStartuped, Func<string>? sendMessage = null)
        {
#if StartWatchTrace
            StartWatchTraceRecord("ProcessCheck");
#endif
            ConfigureServices(Host.IsMainProcess ? DILevel.MainProcess : DILevel.Min);
#if StartWatchTrace
            StartWatchTraceRecord("Startup.Init");
#endif
            onInitStartuped?.Invoke();
            if (Host.IsMainProcess)
            {
                if (!InitAppInstance(sendMessage))
                {
                    return;
                }
                AppInstance!.MessageReceived += async value =>
                {
                    if (!string.IsNullOrEmpty(value))
                    {
                        switch (value)
                        {
                            case key_shutdown:
                                (await WaitForApplicationAsync()).Shutdown();
                                return;
                            default:
                                var args = value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                                if (args.Length >= 1)
                                {
                                    switch (args[0])
                                    {
                                        case key_proxy:
                                            if (args.Length >= 2)
                                            {
                                                ProxyMessageReceived();
                                                return;

                                            }
                                            void ProxyMessageReceived()
                                            {
                                                var value = Enum.TryParse<EOnOff>(args[1], out var value_) ? value_ : default;
                                                try
                                                {
                                                    MainThread2.BeginInvokeOnMainThread(() =>
                                                    {
                                                        ProxyService.Current.ProxyStatus = value switch
                                                        {
                                                            EOnOff.On => true,
                                                            EOnOff.Off => false,
                                                            _ => !ProxyService.Current.ProxyStatus,
                                                        };
                                                    });
                                                }
                                                catch
                                                {

                                                }
                                            }
                                            break;
                                    }
                                }
                                break;
                        }
                    }
                    try
                    {
                        var app = await WaitForApplicationAsync();
                        MainThread2.BeginInvokeOnMainThread(app.RestoreMainWindow);
                    }
                    catch
                    {

                    }
                };
            }
            //#if StartWatchTrace
            //                    StartWatchTraceRecord("ApplicationInstance");
            //#endif
            //                    initCef();
            //#if StartWatchTrace
            //                    StartWatchTraceRecord("InitCefNetApp");
            //#endif
            if (Host.IsMainProcess)
            {
                StartApplication(args);
            }
#if StartWatchTrace
            StartWatchTrace.Record("InitAvaloniaApp");
#endif
        }
        void MainHandlerByCLT(Func<string>? sendMessage = null) => MainHandlerByCLT_(null, sendMessage);
        void MainHandlerByCLT_(Action? onInitStartuped, Func<string>? sendMessage = null)
        {
            SetIsMainProcess(true);
            SetIsCLTProcess(false);
            MainHandler_(onInitStartuped, sendMessage);
        }

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
            MainHandlerByCLT();
        });
        rootCommand.AddCommand(debug);
#endif

        var main = new Command(command_main)
        {
            Handler = CommandHandler.Create(MainHandler),
        };
        rootCommand.AddCommand(main);

        // -clt devtools
        // -clt devtools -disable_gpu
        // -clt devtools -use_wgl
        var devtools = new Command("devtools");
        devtools.AddOption(new Option<bool>("-disable_gpu", () => false, "禁用 GPU 硬件加速"));
        devtools.AddOption(new Option<bool>("-use_wgl", () => false, "使用 Native OpenGL(仅 Windows)"));
        devtools.Handler = CommandHandler.Create((bool disable_gpu, bool use_wgl) =>
        {
            IApplication.EnableDevtools = true;
            IApplication.DisableGPU = disable_gpu;
            IApplication.UseWgl = use_wgl;
            MainHandlerByCLT_(onInitStartuped: () =>
            {
                IApplication.LoggerMinLevel = LogLevel.Debug;
            });
        });
        rootCommand.AddCommand(devtools);

        // -clt c -silence
        var common = new Command("c", "common");
        common.AddOption(new Option<bool>("-silence", "静默启动（不弹窗口）"));
        common.Handler = CommandHandler.Create((bool silence) =>
        {
            Host.IsMinimize = silence;
            MainHandlerByCLT();
        });
        rootCommand.AddCommand(common);

        // -clt steam -account
        var steamuser = new Command("steam", "Steam 相关操作");
        steamuser.AddOption(new Option<string>("-account", "指定对应 Steam 用户名"));
        steamuser.Handler = CommandHandler.Create(async (string account) =>
        {
            if (!string.IsNullOrEmpty(account))
            {
                ConfigureServices(DILevel.Steam);

                var steamService = ISteamService.Instance;

                var users = steamService.GetRememberUserList();

                var currentuser = users.Where(s => s.AccountName == account).FirstOrDefault();

                await steamService.ShutdownSteamAsync();

                if (currentuser != null)
                {
                    currentuser.MostRecent = true;
                    steamService.UpdateLocalUserData(users);
                    steamService.SetCurrentUser(account);
                }

                steamService.StartSteamWithParameter();
            }
        });
        rootCommand.AddCommand(steamuser);

        // -clt app -id 632360
        var run_SteamApp = new Command("app", "运行 Steam 应用");
        run_SteamApp.AddOption(new Option<int>("-id", "指定一个 Steam 游戏 Id"));
        run_SteamApp.AddOption(new Option<bool>("-achievement", "打开成就解锁窗口"));
        run_SteamApp.AddOption(new Option<bool>("-cloudmanager", "打开云存档管理窗口"));
        run_SteamApp.AddOption(new Option<bool>("-silence", "挂运行服务，不加载窗口，内存占用更小"));
        run_SteamApp.Handler = CommandHandler.Create(async (int id, bool achievement, bool cloudmanager) =>
        {
            try
            {
                if (id <= 0) return;
                if (cloudmanager)
                {
                    ConfigureServices(DILevel.GUI | DILevel.Steam | DILevel.HttpClientFactory);
                    IViewModelManager.Instance.InitCloudManageMain(id);
                    StartApplication(args);
                }
                else if (achievement)
                {
                    ConfigureServices(DILevel.GUI | DILevel.Steam | DILevel.HttpClientFactory);
                    IViewModelManager.Instance.InitUnlockAchievement(id);
                    StartApplication(args);
                }
                else
                {
                    ConfigureServices(DILevel.Steam);
                    SteamConnectService.Current.Initialize(id);
                    TaskCompletionSource tcs = new();
                    await tcs.Task;
                }
            }
            catch (Exception ex)
            {
                Log.Error(nameof(run_SteamApp), ex, "Start");
            }
        });
        rootCommand.AddCommand(run_SteamApp);

        // -clt show -config -cert
        var show = new Command("show", "显示信息");
        //show.AddOption(new Option<bool>("-config", "显示 Config.mpo 值"));
        show.AddOption(new Option<bool>("-cert", "显示当前根证书信息"));
        show.Handler = CommandHandler.Create((/*bool config,*/ bool cert) =>
        {
            if (OperatingSystem.IsWindows()) if (!AttachConsole()) AllocConsole();

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
                    using X509Certificate2 rootCert = new(ICertificateManager.DefaultPfxFilePath, (string?)null, X509KeyStorageFlags.Exportable);
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

            if (OperatingSystem.IsWindows()) FreeConsole();
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
            Host.IsMinimize = true;
            Host.IsProxy = true;
            Host.ProxyStatus = on ? EOnOff.On : (off ? EOnOff.Off : EOnOff.Toggle);
            var msg = () => $"{key_proxy} {Host.ProxyStatus}";
            MainHandlerByCLT(msg);
        });
        rootCommand.AddCommand(proxy);

        // -clt ayaneo -path
        var ayaneo = new Command("ayaneo", "生成 ayaneo 数据在指定位置");
        ayaneo.AddOption(new Option<string>("-path", "json 生成路径"));
        ayaneo.Handler = CommandHandler.Create((string path) =>
        {
            ConfigureServices(DILevel.Steam);
            var steamService = ISteamService.Instance;
            var users = steamService.GetRememberUserList();

#pragma warning disable CA1416 // 验证平台兼容性
            IReadOnlyDictionary<long, string?>? accountRemarks = SteamAccountSettings.AccountRemarks.Value;
#pragma warning restore CA1416 // 验证平台兼容性
            var sUsers = users.Select(s =>
            {
                if (accountRemarks?.TryGetValue(s.SteamId64, out var remark) == true &&
                   !string.IsNullOrEmpty(remark))
                    s.Remark = remark;

                var title = s.SteamNickName ?? s.SteamId64.ToString(CultureInfo.InvariantCulture);
                if (!string.IsNullOrEmpty(s.Remark))
                    title = s.SteamNickName + "(" + s.Remark + ")";

#pragma warning disable CS8619 // 值中的引用类型的为 Null 性与目标类型不匹配。
                return new
                {
                    Name = title,
                    Account = s.AccountName,
                };
#pragma warning restore CS8619 // 值中的引用类型的为 Null 性与目标类型不匹配。
            });

            var content = new
            {
                steamppPath = Path.Combine(IOPath.BaseDirectory, System.Application.Constants.HARDCODED_APP_NAME + ".exe"),
                steamUsers = sUsers,
            };
            if (string.IsNullOrWhiteSpace(path))
            {
                path = Path.Combine(IOPath.BaseDirectory, "steampp.json");
            }
            File.WriteAllText(path, Serializable.SJSON(content));
        });
        rootCommand.AddCommand(ayaneo);

        // -clt shutdown
        var shutdown = new Command(key_shutdown, "安全结束正在运行的程序")
        {
            Handler = CommandHandler.Create(() =>
            {
                InitAppInstance(() => key_shutdown);
            }),
        };
        rootCommand.AddCommand(shutdown);

        var r = rootCommand.InvokeAsync(args).GetAwaiter().GetResult();
        return r;
    }

    const string key_proxy = "proxy";
    const string key_shutdown = "shutdown";

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // TODO: 释放托管状态(托管对象)
                _WaitForApplication = null;
                AppInstance?.Dispose();
                Application?.Dispose();
            }

            // TODO: 释放未托管的资源(未托管的对象)并重写终结器
            // TODO: 将大型字段设置为 null
            disposedValue = true;
        }
    }

    // // TODO: 仅当“Dispose(bool disposing)”拥有用于释放未托管资源的代码时才替代终结器
    // ~CommandLineHost()
    // {
    //     // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
    //     Dispose(disposing: false);
    // }

    public void Dispose()
    {
        // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    [SupportedOSPlatform("Windows")]
    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool AttachConsole(int dwProcessId = -1);

    [SupportedOSPlatform("Windows")]
    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool FreeConsole();

    [SupportedOSPlatform("Windows")]
    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool AllocConsole();
}
