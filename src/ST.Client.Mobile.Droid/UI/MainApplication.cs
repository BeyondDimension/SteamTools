using Android.App;
using Android.Runtime;
using System.Application.UI.Resx;
using System.Application.UI.ViewModels;
using System.Diagnostics;
using System.Windows;
using Xamarin.Essentials;
using _ThisAssembly = System.Properties.ThisAssembly;
using AndroidApplication = Android.App.Application;
using AppTheme = System.Application.Models.AppTheme;
using XEFileProvider = Xamarin.Essentials.FileProvider;
using XEPlatform = Xamarin.Essentials.Platform;
using XEVersionTracking = Xamarin.Essentials.VersionTracking;

namespace System.Application.UI
{
    [Register(JavaPackageConstants.UI + nameof(MainApplication))]
    [Application(Debuggable = _ThisAssembly.Debuggable)]
    public sealed class MainApplication : AndroidApplication
    {
        public MainApplication(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
            ViewModelBase.IsInDesignMode = false;
        }

        public override void OnCreate()
        {
            base.OnCreate();

#if DEBUG
            var stopwatch = Stopwatch.StartNew();
#endif

            VisualStudioAppCenterSDK.Init();

            XEFileProvider.TemporaryLocation = FileProviderLocation.Internal;
            XEPlatform.Init(this); // 初始化 Xamarin.Essentials.Platform.Init

            bool GetIsMainProcess()
            {
                // 注意：进程名可以自定义，默认是包名，如果自定义了主进程名，这里就有误，所以不要自定义主进程名！
                var name = this.GetCurrentProcessName();
                return name == PackageName;
            }
            IsMainProcess = GetIsMainProcess();

            var level = DILevel.Min;
            if (IsMainProcess) level = DILevel.MainProcess;
            Startup.Init(level);
            if (IsMainProcess)
            {
                XEVersionTracking.Track();
                ImageLoader.Init(this);
            }

#if DEBUG
            Log.Debug("Application", $"OnCreate Complete({stopwatch.ElapsedMilliseconds}ms).");
#endif
        }

        /// <summary>
        /// 当前是否是主进程
        /// </summary>
        public static bool IsMainProcess { get; private set; }

        public static string GetTheme()
        {
            if (DarkModeUtil.IsDarkMode(Context))
            {
                return AppTheme.Dark.ToString2();
            }
            return AppTheme.Light.ToString2();
        }

        public static async void ShowUnderConstructionTips() => await MessageBoxCompat.ShowAsync(AppResources.UnderConstruction, "", MessageBoxButtonCompat.OK);
    }
}