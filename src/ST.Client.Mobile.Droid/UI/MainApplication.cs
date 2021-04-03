using Android.App;
using Android.Runtime;
using System.Properties;
using Xamarin.Essentials;
using AndroidApplication = Android.App.Application;
using XEFileProvider = Xamarin.Essentials.FileProvider;
using XEPlatform = Xamarin.Essentials.Platform;
using XEVersionTracking = Xamarin.Essentials.VersionTracking;

namespace System.Application.UI
{
    [Register(JavaPackageConstants.UI + nameof(MainApplication))]
    [Application(Debuggable = ThisAssembly.Debuggable)]
    public sealed class MainApplication : AndroidApplication
    {
        public MainApplication(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        /// <summary>
        /// 是否为主进程
        /// </summary>
        /// <returns></returns>
        public bool IsMainProcess()
        {
            // 注意：进程名可以自定义，默认是包名，如果自定义了主进程名，这里就有误，所以不要自定义主进程名！
            var name = this.GetCurrentProcessName();
            return name == PackageName;
        }

        public override void OnCreate()
        {
            base.OnCreate();

            VisualStudioAppCenterSDK.Init();

            XEFileProvider.TemporaryLocation = FileProviderLocation.Internal;
            XEPlatform.Init(this); // 初始化 Xamarin.Essentials.Platform.Init

            Startup.IsMainProcess = IsMainProcess();

            DILevel level;
            if (Startup.IsMainProcess) level = DILevel.MainProcess;
            level = DILevel.Min;
            Startup.Init(level);

            if (Startup.IsMainProcess)
            {
                XEVersionTracking.Track();
            }

#if DEBUG
            Log.Debug("Application", "OnCreate Complete.");
#endif
        }
    }
}