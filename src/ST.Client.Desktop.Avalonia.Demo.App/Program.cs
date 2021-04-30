namespace System.Application.UI
{
    static partial class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            IsMainProcess = true;
            Startup.Init(DILevel.MainProcess);
            BuildAvaloniaAppAndStartWithClassicDesktopLifetime(args);
        }

        /// <summary>
        /// 当前是否是主进程
        /// </summary>
        public static bool IsMainProcess { get; private set; }
    }
}
