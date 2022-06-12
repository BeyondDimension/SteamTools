using System.Application.UI;
using System.Application.Services;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
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

    protected abstract void ConfigureServices(DILevel level);

#if StartWatchTrace
    protected abstract void StartWatchTraceRecord(string? mark = null, bool dispose = false);
#endif

    public IApplication.SingletonInstance? AppInstance { get; protected set; }

    protected abstract void StartApplication(string[] args);

    public abstract IApplication? Application { get; }

    protected abstract void SetIsMainProcess(bool value);

    protected abstract void SetIsCLTProcess(bool value);

    public int Run(string[] args)
    {
        if (args.Length == 0) args = new[] { "-h" };

        // https://docs.microsoft.com/zh-cn/archive/msdn-magazine/2019/march/net-parse-the-command-line-with-system-commandline
        var rootCommand = new RootCommand("命令行工具(Command Line Tools/CLT)");

        void MainHandler() => MainHandler_(null);
        void MainHandler_(Action? onInitStartuped)
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
                var isInitAppInstanceReset = false;
            initAppInstance: AppInstance = new();
                if (!AppInstance.IsFirst)
                {
                    //Console.WriteLine("ApplicationInstance.SendMessage(string.Empty);");
                    if (IApplication.SingletonInstance.SendMessage(string.Empty))
                    {
                        return;
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
                            return;
                        }
                    }
                }
                AppInstance.MessageReceived += value =>
                {
                    if (string.IsNullOrEmpty(value))
                    {
                        var app = Application;
                        if (app != null)
                        {
                            MainThread2.BeginInvokeOnMainThread(app.RestoreMainWindow);
                        }
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
        void MainHandlerByCLT() => MainHandlerByCLT_(null);
        void MainHandlerByCLT_(Action? onInitStartuped)
        {
            SetIsMainProcess(true);
            SetIsCLTProcess(false);
            MainHandler_(onInitStartuped);
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

                steamService.StartSteam();
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

        var r = rootCommand.InvokeAsync(args).GetAwaiter().GetResult();
        return r;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // TODO: 释放托管状态(托管对象)
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
}
