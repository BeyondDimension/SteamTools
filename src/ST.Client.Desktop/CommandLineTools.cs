using System.Application.Services;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;

namespace System.Application
{
    /// <summary>
    /// 命令行工具(Command Line Tools/CLT)
    /// </summary>
    public static class CommandLineTools
    {
        public delegate void InitUIApp(bool initWindowS);

        public static int Main(string[] args,
            Action<DILevel> initStartup,
            InitUIApp initUIApp,
            Action initCef)
        {
            if (args.Length == 0) args = new[] { "-h" };

            // https://docs.microsoft.com/zh-cn/archive/msdn-magazine/2019/march/net-parse-the-command-line-with-system-commandline
            var rootCommand = new RootCommand("命令行工具(Command Line Tools/CLT)");

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
                initStartup(DILevel.Main);
                initCef();
                initUIApp(initWindowS: true);
            });
            rootCommand.AddCommand(debug);

#endif

            // -clt unlock-achievement -appid 730
            var unlock_achievement = new Command("unlock-achievement", "打开成就解锁窗口");
            unlock_achievement.AddOption(new Option<int>("-appid", "指定一个Steam游戏Id"));
            unlock_achievement.Handler = CommandHandler.Create((int appid) =>
            {
                if (appid <= 0) return;
                initStartup(DILevel.Min);
                IWindowService.Instance.InitUnlockAchievement(appid);
                initUIApp(initWindowS: false);
            });
            rootCommand.AddCommand(unlock_achievement);

            var r = rootCommand.InvokeAsync(args, new SystemConsole()).Result;
            return r;
        }

        public enum DILevel : byte
        {
            /// <summary>
            /// 主进程所需DI服务级别
            /// </summary>
            Main,

            /// <summary>
            /// 最小DI服务级别
            /// </summary>
            Min,
        }
    }
}