using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using System.Threading.Tasks;
using System.Linq;
using AvaloniaApplication = Avalonia.Application;
using System.Application.UI.ViewModels;

namespace System.Application.Services
{
    public interface IDesktopAvaloniaAppService
    {
        public static IDesktopAvaloniaAppService Instance => DI.Get<IDesktopAvaloniaAppService>();

        Window MainWindow { get; }

        AvaloniaApplication Current { get; }

        Window GetActiveWindow();

        /// <summary>
        /// 打开子窗口
        /// </summary>
        /// <param name="window"></param>
        /// <returns></returns>
        async Task ShowDialogWindow(Window window)
        {
            var owner = GetActiveWindow();
            await window.ShowDialog(owner);
        }

        void ShowWindow(Window window)
        {
            var owner = GetActiveWindow();
            window.Show(owner);
        }

        void ShowWindowNoParent(Window window)
        {
            var owner = GetActiveWindow();
            window.Show();
        }

        /// <summary>
        /// 根据WindowViewModel显示window
        /// </summary>
        /// <param name="window"></param>
        void ShowWindowNoParent(WindowViewModel vm)
        {
            if (Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var window = desktop.Windows.FirstOrDefault(x => x.DataContext == vm);
                window?.Show();
            }
        }

        /// <summary>
        /// 根据WindowViewModel关闭window
        /// </summary>
        /// <param name="window"></param>
        void CloseWindow(WindowViewModel vm)
        {
            if (Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var window = desktop.Windows.FirstOrDefault(x => x.DataContext == vm);
                window?.Close();
            }
        }

        void HideWindow(WindowViewModel vm)
        {
            if (Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var window = desktop.Windows.FirstOrDefault(x => x.DataContext == vm);
                window?.Hide();
            }
        }
    }
}