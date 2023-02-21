namespace BD.WTTS;

static partial class Program
{
    sealed partial class Host : CommandLineHost
    {
        protected override IApplication.IDesktopProgramHost Host => this;

        public Action<AppServicesLevel>? ConfigureServicesDelegate { get; set; }

        protected override void ConfigureServices(AppServicesLevel level, bool isTrace = false)
        {
            if (ConfigureServicesDelegate != null)
            {
                ConfigureServicesDelegate.Invoke(level);
            }
            else
            {
                Program.ConfigureServices(this, level, isTrace: isTrace);
            }
        }

#if StartWatchTrace
        protected override void StartWatchTraceRecord(string? mark = null, bool dispose = false)
            => StartWatchTrace.Record(mark, dispose);
#endif

        protected override void StartApplication(string[] args) =>
#if MAUI
            StartMauiApp(args);
#else
            StartAvaloniaApp(args);
#endif

        public override IApplication? Application => App.Instance;

        protected override void SetIsMainProcess(bool value) => IsMainProcess = value;

        protected override void SetIsCLTProcess(bool value) => IsCLTProcess = value;
    }
}