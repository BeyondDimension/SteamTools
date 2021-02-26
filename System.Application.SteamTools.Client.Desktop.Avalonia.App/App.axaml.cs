using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using System.Application.UI.ViewModels;
using System.Application.UI.Views;
using System.IO;
using System.Windows;
using AvaloniaApplication = Avalonia.Application;

namespace System.Application.UI
{
    public class App : AvaloniaApplication
    {
        public static App Instance => Current is App app ? app : throw new Exception("Impossible");

        [Obsolete("use IOPath.AppDataDirectory", true)]
        public DirectoryInfo LocalAppData => new DirectoryInfo(IOPath.AppDataDirectory);

        public static DirectoryInfo RootDirectory => new DirectoryInfo(AppContext.BaseDirectory);

        [Obsolete("use AppHelper.ProgramName", true)]
        public string ProgramName => AppHelper.ProgramName;

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            // 在UI预览中，ApplicationLifetime 为 null
            ViewModelBase.IsInDesignMode = ApplicationLifetime == null;
            Startup.Init();

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                if (!AppHelper.IsOfficialChannelPackage)
                {
                    MessageBox.Show("The program currently running is not from the official channel.", "Warning",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }

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