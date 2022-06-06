using System.Application.CommandLine;
using System.Application.Services;
#if MAUI
using Program = System.Application.UI.MauiProgram;
#endif

namespace System.Application.UI;

static partial class
#if MAUI
    MauiProgram
#else
    Program
#endif
{
    private sealed partial class ProgramHost
    {
        public static ProgramHost Instance { get; } = new();

        private ProgramHost()
        {

        }
    }

    partial class ProgramHost : IApplication.IDesktopProgramHost
    {
        public bool IsMinimize { get; set; }

        public bool IsCLTProcess { get; set; }

        public bool IsMainProcess { get; set; }

        public bool IsTrayProcess { get; set; }

        public void ConfigureServices(DILevel level) => Program.ConfigureServices(this, level);

        public void InitVisualStudioAppCenterSDK()
        {
#if !MAUI && (WINDOWS || XAMARIN_MAC || __MOBILE__ || __ANDROID__ || __IOS__ || MACCATALYST || IOS)
#pragma warning disable IDE0079 // 请删除不必要的忽略
#pragma warning disable CA1416 // 验证平台兼容性
            VisualStudioAppCenterSDK.Init();
#pragma warning restore CA1416 // 验证平台兼容性
#pragma warning restore IDE0079 // 请删除不必要的忽略
#endif
        }

        public void OnStartup() => Program.OnStartup(this);

        IApplication IApplication.IProgramHost.Application => App.Instance;

        void IApplication.IDesktopProgramHost.OnCreateAppExecuted(Action<IViewModelManager>? handlerViewModelManager, bool isTrace) => Program.OnCreateAppExecuted(this, handlerViewModelManager, isTrace);
    }

    partial class ProgramHost : CommandLineHost
    {
        protected override IApplication.IDesktopProgramHost Host => this;

        protected override void ConfigureServices(IApplication.IStartupArgs args, DILevel level)
            => Program.ConfigureServices(args, level);

#if StartWatchTrace
        protected override void StartWatchTraceRecord(string? mark = null, bool dispose = false)
            => StartWatchTrace.Record(mark, dispose);
#endif

        protected override void StartApplication(string[] args) =>
#if MAUI
            StartMauiApp();
#else
            StartAvaloniaApp(args);
#endif

        public override IApplication? Application => App.Instance;

        protected override void SetIsMainProcess(bool value) => IsMainProcess = value;

        protected override void SetIsCLTProcess(bool value) => IsCLTProcess = value;
    }
}