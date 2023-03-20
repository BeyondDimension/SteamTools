// ReSharper disable once CheckNamespace
namespace BD.WTTS;

static partial class Program
{
    sealed partial class Host : IApplication.IDesktopProgramHost
    {
        public bool IsMinimize { get; set; }

        public bool IsCLTProcess { get; set; }

        public bool IsMainProcess { get; set; }

        public bool IsTrayProcess { get; set; }

        public bool IsProxy { get; set; }

        public OnOffToggle ProxyStatus { get; set; }

        void IApplication.IProgramHost.ConfigureServices(AppServicesLevel level, bool isTrace)
            => ConfigureServicesAsync(level, isTrace);

        bool isInitVisualStudioAppCenterSDK = false;

        public void InitVisualStudioAppCenterSDK()
        {
            if (isInitVisualStudioAppCenterSDK) return;
            isInitVisualStudioAppCenterSDK = true;

            //#if !MAUI && (WINDOWS || XAMARIN_MAC || __MOBILE__ || __ANDROID__ || __IOS__ || MACCATALYST || IOS)
            //#pragma warning disable IDE0079 // 请删除不必要的忽略
            //#pragma warning disable CA1416 // 验证平台兼容性
            //            VisualStudioAppCenterSDK.Init();
            //#pragma warning restore CA1416 // 验证平台兼容性
            //#pragma warning restore IDE0079 // 请删除不必要的忽略
            //#endif
        }

        public void OnStartup() => Program.OnStartup(this);

        IApplication IApplication.IProgramHost.Application => App.Instance;

        void IApplication.IDesktopProgramHost.OnCreateAppExecuted(Action<IViewModelManager>? handlerViewModelManager, bool isTrace) => Program.OnCreateAppExecuted(this, handlerViewModelManager, isTrace);

        public DeploymentMode DeploymentMode => DeploymentMode.
#if FRAMEWORK_DEPENDENT || !PUBLISH
           FDE
#else
           SCD
#endif
           ;
    }
}