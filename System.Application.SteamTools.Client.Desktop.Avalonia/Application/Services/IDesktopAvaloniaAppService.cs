using Avalonia.Controls;
using System.Threading.Tasks;

namespace System.Application.Services
{
    public interface IDesktopAvaloniaAppService
    {
        public static IDesktopAvaloniaAppService Instance => DI.Get<IDesktopAvaloniaAppService>();

        Window MainWindow { get; }

        /// <summary>
        /// 打开子窗口
        /// </summary>
        /// <param name="window"></param>
        /// <returns></returns>
        async Task ShowDialogWindow(Window window) => await window.ShowDialog(MainWindow);

        void ShowWindow(Window window) => window.Show(MainWindow);
    }
}