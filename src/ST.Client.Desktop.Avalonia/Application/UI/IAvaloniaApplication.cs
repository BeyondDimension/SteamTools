using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using System.Threading.Tasks;
using System.Linq;
using System.Application.UI.ViewModels;
using System.Application.Services;
using AvaloniaApplication = Avalonia.Application;
using System.Application.Mvvm;

namespace System.Application.UI
{
    public interface IAvaloniaApplication : IApplication
    {
        static new IAvaloniaApplication Instance => DI.Get<IAvaloniaApplication>();

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
            if (owner != null)
            {
                try
                {
                    await window.ShowDialog(owner);
                    return;
                }
                catch (InvalidOperationException)
                {
                }
            }
            window.Show();
        }

        void ShowWindow(Window window)
        {
            var owner = GetActiveWindow();
            if (owner != null)
            {
                try
                {
                    window.Show(owner);
                    return;
                }
                catch (InvalidOperationException)
                {
                }

            }
            window.Show();
        }

        void ShowWindowNoParent(Window window)
        {
            window.Show();
        }

        /// <summary>
        /// 获取当前渲染子系统名称
        /// </summary>
        string RenderingSubsystemName => string.Empty;
    }
}