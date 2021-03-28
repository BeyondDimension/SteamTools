using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using System.Application.UI.Views;
using System.Properties;
using System.Reflection;
using AvaloniaApplication = Avalonia.Application;

[assembly: AssemblyTitle(ThisAssembly.AssemblyTrademark + ".Setup v" + ThisAssembly.Version)]
namespace System.Application.UI
{
    public class App : AvaloniaApplication
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow();
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}