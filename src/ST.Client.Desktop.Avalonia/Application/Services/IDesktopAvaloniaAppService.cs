using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using System.Linq;
using System.Threading.Tasks;

namespace System.Application.Services
{
    public interface IDesktopAvaloniaAppService
    {
        public static IDesktopAvaloniaAppService Instance => DI.Get<IDesktopAvaloniaAppService>();

        Window MainWindow { get; }

        Avalonia.Application CurrentApp { get; }

        protected Window GetActiveWindow()
        {
            if (CurrentApp.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var activeWindow = desktop.Windows.FirstOrDefault(x => x.IsActive);
                if (activeWindow != null)
                {
                    return activeWindow;
                }
            }
            return MainWindow;
        }

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
    }
}