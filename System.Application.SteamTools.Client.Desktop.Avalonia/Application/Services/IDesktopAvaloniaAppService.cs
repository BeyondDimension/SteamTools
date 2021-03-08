using Avalonia.Controls;
using System.Threading.Tasks;

namespace System.Application.Services
{
    public interface IDesktopAvaloniaAppService
    {
        public static IDesktopAvaloniaAppService Instance => DI.Get<IDesktopAvaloniaAppService>();

        /// <summary>
        /// 打开子窗口
        /// </summary>
        /// <param name="window"></param>
        /// <returns></returns>
        Task ShowDialogWindow(Window window);

        void ShowWindow(Window window);
    }
}