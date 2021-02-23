using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using System.Application.UI.ViewModels;
using System.Application.UI.Views;
using System.IO;
using AvaloniaApplication = Avalonia.Application;

namespace System.Application.UI
{
    public class App : AvaloniaApplication
    {
        public static App Instance => Current is App app ? app : throw new Exception("Impossible");

        [Obsolete("use IOPath.AppDataDirectory", true)]
        public DirectoryInfo LocalAppData => new DirectoryInfo(IOPath.AppDataDirectory);

        [Obsolete("use AppHelper.ProgramName", true)]
        public string ProgramName => AppHelper.ProgramName;

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            // ÔÚUIÔ¤ÀÀÖÐ£¬ApplicationLifetime Îª null
            ViewModelBase.IsInDesignMode = ApplicationLifetime == null;

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow
                {
                    DataContext = DataContext = DI.Get<MainWindowViewModel>(),
                };
                desktop.ShutdownMode = ShutdownMode.OnMainWindowClose;
            }

            base.OnFrameworkInitializationCompleted();
        }

        public static async void SetClipboardText(string s) => await Current.Clipboard.SetTextAsync(s);
    }
}