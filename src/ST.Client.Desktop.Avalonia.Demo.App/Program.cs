namespace System.Application.UI
{
    static partial class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Startup.IsMainProcess = true;
            Startup.Init(DILevel.MainProcess);
            BuildAvaloniaAppAndStartWithClassicDesktopLifetime(args);
        }
    }
}