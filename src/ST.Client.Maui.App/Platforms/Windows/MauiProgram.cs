using System.Runtime.InteropServices;
using WinRT;
using Microsoft.UI.Dispatching;
using WinUIApplication = Microsoft.UI.Xaml.Application;
using WinUIApp = System.Application.UI.WinUI.App;

namespace System.Application.UI
{
    public static partial class MauiProgram
    {
        static event Action? Exit;

        [DllImport("Microsoft.ui.xaml.dll")]
        static extern void XamlCheckProcessRequirements();

        [STAThread]
        static void Main(string[] args)
        {
            // 不能以管理员权限启动 程序“.exe”已退出，返回值为 3221226505 (0xc0000409)。
            try
            {
                XamlCheckProcessRequirements();
            }
            catch (DllNotFoundException ex)
            {
                // 需要安装 Windows App Runtime
                // <WindowsPackageType>None</WindowsPackageType>
                throw new ApplicationException("Requires Windows App Runtime to be installed.", ex);
            }
            ComWrappersSupport.InitializeComWrappers();
            WinUIApplication.Start(p =>
            {
                var context = new DispatcherQueueSynchronizationContext(DispatcherQueue.GetForCurrentThread());
                SynchronizationContext.SetSynchronizationContext(context);

                var app = new WinUIApp();
            });

            Exit?.Invoke();
        }
    }
}
