using System.Application.Services;
using System.Application.UI;
using System.CommandLine;
using System.CommandLine.Invocation;

namespace System.Application
{
    /// <summary>
    /// 命令行工具(Command Line Tools/CLT)
    /// </summary>
    public static class CommandLineTools
    {
        public static int Main(string[] args,
            Action<DILevel> initStartup,
            Action initUIApp,
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
                initStartup(DILevel.MainProcess);
                initCef();
                initUIApp();
            });
            rootCommand.AddCommand(debug);
#endif

            // -clt devtools
            var devtools = new Command("devtools");
            devtools.Handler = CommandHandler.Create(() =>
            {
                var appInstance = new ApplicationInstance();
                if (!appInstance.IsFirst) return;
                Startup.IsMainProcess = true;
                Program.IsCLTProcess = false;
                AppHelper.EnableDevtools = true;
                initStartup(DILevel.MainProcess);
                initCef();
                initUIApp();
            });
            rootCommand.AddCommand(devtools);

            // -clt c
            var common = new Command("c", "common");
            common.AddOption(new Option<bool>("-silence", "静默启动（不弹窗口）"));
            common.Handler = CommandHandler.Create((bool silence) =>
            {
                initStartup(DILevel.MainProcess);
                initCef();
                Program.IsMinimize = silence;
                initUIApp();
            });
            rootCommand.AddCommand(common);

            // -clt app -id 632360
            var unlock_achievement = new Command("app", "打开成就解锁窗口");
            unlock_achievement.AddOption(new Option<int>("-id", "指定一个Steam游戏Id"));
            unlock_achievement.AddOption(new Option<bool>("-silence", "静默启动（不弹窗口）"));
            unlock_achievement.Handler = CommandHandler.Create((int id, bool silence) =>
            {
                if (id <= 0) return;
                initStartup(DILevel.GUI | DILevel.Steam | DILevel.HttpClientFactory);
                IWindowService.Instance.InitUnlockAchievement(id);
                Program.IsMinimize = silence;
                initUIApp();
            });
            rootCommand.AddCommand(unlock_achievement);

            var r = rootCommand.InvokeAsync(args).Result;
            return r;
        }
    }
}