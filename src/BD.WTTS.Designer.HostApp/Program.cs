using Avalonia;

namespace BD.WTTS;

static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        FileSystem2.InitFileSystem();
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        //Avalonia.DesignerSupport.Remote.RemoteDesignerEntryPoint.Main(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UseWin32()
            .UseSkia()
            .LogToTrace(Avalonia.Logging.LogEventLevel.Debug, "Designer");
}