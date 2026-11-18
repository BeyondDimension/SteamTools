using Avalonia.Threading;
using BD.WTTS.Client.Resources;

namespace BD.WTTS.UI.Views.Windows;

sealed class TrayPanelViewModel : ReactiveObject, IDisposable
{
    const string NoSample = "—";

    readonly SerialDisposable acceleratorSubscription = new();
    readonly SerialDisposable accountSubscription = new();
    readonly IDisposable steamSubscription;
    TrayMenuItem? accelerator;
    TrayMenuItem? accountGroup;
    TrayToggleState acceleratorState;
    TrayAccelerationSession? accelerationSession;
    DispatcherTimer? sessionTimer;
    bool readingTraffic;
    string accelerationDuration = NoSample;
    long elapsedSeconds = -1;
    TrayTrafficSample? traffic;
    string uploadTraffic = NoSample;
    string downloadTraffic = NoSample;
    TrayAccountsStatus accountStatus;
    IReadOnlyList<TrayAccountItem> accounts = [];
    string steamActionText = Strings.StartSteam;
    SecondarySection section;

    enum SecondarySection { None, Projects, Accounts }

    public TrayPanelViewModel(ICommand steamAction, IObservable<bool> steamRunning)
    {
        SteamAction = steamAction;
        steamSubscription = steamRunning.ObserveOn(RxSchedulers.MainThreadScheduler).Subscribe(running =>
            this.RaiseAndSetIfChanged(ref steamActionText, running ? Strings.CloseSteam : Strings.StartSteam, nameof(SteamActionText)));
    }

    public ICommand SteamAction { get; }
    public string SteamActionText => steamActionText;

    public ICommand? AccelerationCommand => accelerator?.Command;
    public object? AccelerationCommandParameter => accelerator?.CommandParameter;
    public bool IsAccelerationOn => IsOn(acceleratorState);
    public bool CanToggleAcceleration => AccelerationCommand != null && CanToggle(acceleratorState);
    public string AccelerationStatus => StatusText(acceleratorState);
    public bool HasAccelerationSession => accelerationSession != null;
    public IReadOnlyList<string> AccelerationProjects => accelerationSession?.Projects ?? [];
    public bool HasAccelerationProjects => AccelerationProjects.Count > 0;
    public string AccelerationDuration => accelerationDuration;
    public string UploadTraffic => uploadTraffic;
    public string DownloadTraffic => downloadTraffic;

    public bool AreProjectsExpanded
    {
        get => section == SecondarySection.Projects;
        set
        {
            if (value && HasAccelerationSession)
                SetSection(SecondarySection.Projects);
            else if (AreProjectsExpanded)
                SetSection(SecondarySection.None);
        }
    }

    public bool AreAccountsExpanded
    {
        get => section == SecondarySection.Accounts;
        set
        {
            if (value)
                SetSection(SecondarySection.Accounts);
            else if (AreAccountsExpanded)
                SetSection(SecondarySection.None);
        }
    }

    public ICommand? ManageAccountsCommand => accountGroup?.Command;
    public object? ManageAccountsCommandParameter => accountGroup?.CommandParameter;
    public bool HasAccountGroup => accountGroup != null;
    public IReadOnlyList<TrayAccountItem> Accounts => accounts;
    public bool HasNoAccounts => accounts.Count == 0;
    public ICommand? RefreshAccountsCommand => accountGroup?.RefreshAccountsCommand;
    public bool CanRefreshAccounts => accountStatus == TrayAccountsStatus.Failed && RefreshAccountsCommand != null;
    public string AccountStatusText => AccountsStatusText(accountStatus);

    public void SetAccelerator(TrayMenuItem? item, TrayToggleState initialState)
    {
        if (ReferenceEquals(accelerator, item) && item != null)
            return;
        acceleratorSubscription.Disposable = null;
        accelerator = item;
        this.RaisePropertyChanged(nameof(AccelerationCommand));
        this.RaisePropertyChanged(nameof(AccelerationCommandParameter));
        SetAcceleratorState(initialState);
        if (item?.ToggleState is { } states)
            acceleratorSubscription.Disposable = states.ObserveOn(RxSchedulers.MainThreadScheduler).Subscribe(SetAcceleratorState);
    }

    void SetAcceleratorState(TrayToggleState value)
    {
        acceleratorState = value;
        this.RaisePropertyChanged(nameof(IsAccelerationOn));
        this.RaisePropertyChanged(nameof(CanToggleAcceleration));
        this.RaisePropertyChanged(nameof(AccelerationStatus));
        UpdateAccelerationSession(IsOn(value) ? accelerator?.GetAccelerationSession?.Invoke() : null);
    }

    void UpdateAccelerationSession(TrayAccelerationSession? session)
    {
        if (!ReferenceEquals(accelerationSession, session))
        {
            accelerationSession = session;
            elapsedSeconds = -1;
            SetTraffic(null);
            if (session == null)
            {
                this.RaiseAndSetIfChanged(ref accelerationDuration, NoSample, nameof(AccelerationDuration));
                AreProjectsExpanded = false;
            }
            this.RaisePropertyChanged(nameof(HasAccelerationSession));
            this.RaisePropertyChanged(nameof(AccelerationProjects));
            this.RaisePropertyChanged(nameof(HasAccelerationProjects));
        }

        if (session == null || acceleratorState != TrayToggleState.On)
        {
            sessionTimer?.Stop();
            return;
        }

        if (sessionTimer == null)
        {
            sessionTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            sessionTimer.Tick += (_, _) => UpdateSessionMetrics();
        }
        sessionTimer.Start();
        UpdateSessionMetrics();
    }

    void UpdateSessionMetrics()
    {
        if (accelerationSession is not { } session || acceleratorState != TrayToggleState.On)
            return;

        var elapsed = Stopwatch.GetElapsedTime(session.StartedTimestamp);
        var seconds = elapsed.Ticks / TimeSpan.TicksPerSecond;
        if (elapsedSeconds != seconds)
        {
            elapsedSeconds = seconds;
            this.RaiseAndSetIfChanged(ref accelerationDuration, $"{seconds / 3600:00}:{elapsed.Minutes:00}:{elapsed.Seconds:00}", nameof(AccelerationDuration));
        }

        if (!readingTraffic)
            _ = ReadTrafficAsync(session);
    }

    async Task ReadTrafficAsync(TrayAccelerationSession session)
    {
        readingTraffic = true;
        TrayTrafficSample? sample = null;
        try
        {
            // The existing IPC call is synchronous; do not block the popup dispatcher.
            sample = await Task.Run(session.ReadTraffic);
        }
        catch (Exception ex)
        {
            Log.Error(nameof(TrayPanelViewModel), ex, "Could not read tray accelerator traffic.");
        }
        finally
        {
            readingTraffic = false;
        }

        if (ReferenceEquals(accelerationSession, session) && acceleratorState == TrayToggleState.On)
            SetTraffic(sample);
    }

    void SetTraffic(TrayTrafficSample? sample)
    {
        if (traffic == sample)
            return;
        var previous = traffic;
        traffic = sample;
        if (previous?.UploadedBytes != sample?.UploadedBytes || previous?.UploadBytesPerSecond != sample?.UploadBytesPerSecond)
            this.RaiseAndSetIfChanged(ref uploadTraffic, sample is { } up ? FormatTraffic(up.UploadBytesPerSecond, up.UploadedBytes) : NoSample, nameof(UploadTraffic));
        if (previous?.DownloadedBytes != sample?.DownloadedBytes || previous?.DownloadBytesPerSecond != sample?.DownloadBytesPerSecond)
            this.RaiseAndSetIfChanged(ref downloadTraffic, sample is { } down ? FormatTraffic(down.DownloadBytesPerSecond, down.DownloadedBytes) : NoSample, nameof(DownloadTraffic));
    }

    static string FormatTraffic(double bytesPerSecond, long totalBytes)
    {
        var (rate, rateUnit) = IOPath.GetDisplayFileSize(bytesPerSecond);
        var (total, totalUnit) = IOPath.GetDisplayFileSize(totalBytes);
        return $"{rate:0.#} {rateUnit}/s · {total:0.#} {totalUnit}";
    }

    void SetSection(SecondarySection value)
    {
        if (section == value)
            return;
        var previous = section;
        section = value;
        if (previous == SecondarySection.Projects || AreProjectsExpanded)
            this.RaisePropertyChanged(nameof(AreProjectsExpanded));
        if (previous == SecondarySection.Accounts || AreAccountsExpanded)
        {
            this.RaisePropertyChanged(nameof(AreAccountsExpanded));
            UpdateAccountSubscription();
        }
    }

    public void SetAccounts(TrayMenuItem? item)
    {
        if (ReferenceEquals(accountGroup, item))
            return;
        accountGroup = item;
        this.RaisePropertyChanged(nameof(HasAccountGroup));
        this.RaisePropertyChanged(nameof(ManageAccountsCommand));
        this.RaisePropertyChanged(nameof(ManageAccountsCommandParameter));
        this.RaisePropertyChanged(nameof(RefreshAccountsCommand));
        if (!HasAccountGroup && AreAccountsExpanded)
            SetSection(SecondarySection.None);
        else if (AreAccountsExpanded)
            UpdateAccountSubscription();
    }

    void UpdateAccountSubscription()
    {
        accountSubscription.Disposable = null;
        var source = AreAccountsExpanded ? accountGroup?.Accounts : null;
        SetAccountRows(new(source != null ? TrayAccountsStatus.Loading : TrayAccountsStatus.Failed, []));
        if (source != null)
            accountSubscription.Disposable = source.ObserveOn(RxSchedulers.MainThreadScheduler).Subscribe(SetAccountRows);
    }

    void SetAccountRows(TrayAccountsSnapshot value)
    {
        accountStatus = value.Status;
        accounts = value.Rows;
        this.RaisePropertyChanged(nameof(Accounts));
        this.RaisePropertyChanged(nameof(HasNoAccounts));
        this.RaisePropertyChanged(nameof(CanRefreshAccounts));
        this.RaisePropertyChanged(nameof(AccountStatusText));
    }

    internal static bool IsOn(TrayToggleState state) => state is TrayToggleState.On or TrayToggleState.Stopping;

    internal static bool CanToggle(TrayToggleState state) => state is TrayToggleState.On or TrayToggleState.Off;

    internal static string StatusText(TrayToggleState state) => state switch
    {
        TrayToggleState.Loading => Strings.Tray_AcceleratorLoading,
        TrayToggleState.Preparing => Strings.Tray_AcceleratorPreparing,
        TrayToggleState.Off => Strings.Off,
        TrayToggleState.Starting => Strings.Tray_AcceleratorStarting,
        TrayToggleState.On => Strings.On,
        TrayToggleState.Stopping => Strings.Tray_AcceleratorStopping,
        _ => Strings.Tray_AcceleratorUnavailable,
    };

    internal static string AccountsStatusText(TrayAccountsStatus status) => status switch
    {
        TrayAccountsStatus.Loading => Strings.Loading,
        TrayAccountsStatus.Loaded => Strings.Tray_NoSavedAccounts,
        _ => Strings.Tray_AccountReadFailed,
    };

    public void Dispose()
    {
        sessionTimer?.Stop();
        accelerationSession = null;
        acceleratorSubscription.Dispose();
        accountSubscription.Dispose();
        steamSubscription.Dispose();
    }
}
