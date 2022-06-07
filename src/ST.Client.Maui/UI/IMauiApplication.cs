using MauiApplication = Microsoft.Maui.Controls.Application;

namespace System.Application.UI
{
    public interface IMauiApplication : IApplication
    {
        static new IMauiApplication Instance => DI.Get<IMauiApplication>();

        Window? MainWindow { get; }

        MauiApplication Current { get; }

        Window GetActiveWindow();
    }
}
