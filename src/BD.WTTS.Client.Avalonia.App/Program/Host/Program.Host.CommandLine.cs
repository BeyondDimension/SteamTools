// ReSharper disable once CheckNamespace
namespace BD.WTTS;

static partial class Program
{
    sealed partial class Host : CommandLineHost
    {
        public Func<AppServicesLevel, ValueTask>? ConfigureServicesDelegate { get; set; }

        public override async ValueTask ConfigureServicesAsync(AppServicesLevel level, bool isTrace = false)
        {
            if (ConfigureServicesDelegate != null)
            {
                await ConfigureServicesDelegate.Invoke(level);
            }
            else
            {
                await Program.ConfigureServicesAsync(this, level, isTrace: isTrace);
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