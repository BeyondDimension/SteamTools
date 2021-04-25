using Avalonia.Controls;
using System.Threading.Tasks;
using AvaloniaApplication = Avalonia.Application;

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
    }
}