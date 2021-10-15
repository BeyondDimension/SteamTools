using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using System.Threading.Tasks;
using System.Linq;
using System.Application.UI.ViewModels;
using System.Application.Services;
using AvaloniaApplication = Avalonia.Application;

namespace System.Application.UI
{
    public interface IAvaloniaApplication : IService<IAvaloniaApplication>, IApplication
    {
        static new IAvaloniaApplication Instance => IService<IAvaloniaApplication>.Instance;

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

        bool IsVisibleWindow(WindowViewModel vm)
        {
            if (Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var window = desktop.Windows.FirstOrDefault(x => x.DataContext == vm);
                return window?.IsVisible == true;
            }
            return false;
        }

        void HideWindow(WindowViewModel vm)
        {
            if (Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var window = desktop.Windows.FirstOrDefault(x => x.DataContext == vm);
                window?.Hide();
            }
        }

        /// <summary>
        /// 获取当前渲染子系统名称
        /// </summary>
        string RenderingSubsystemName => string.Empty;
    }
}