using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using BD.WTTS.Services.GameTools;
using ReactiveUI.Reactive;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Input;

namespace BD.WTTS.UI.ViewModels;

public sealed class CpuSchedulingPageViewModel : ViewModelBase
{
    readonly DispatcherTimer refreshTimer = new() { Interval = TimeSpan.FromSeconds(2) };
    readonly Dictionary<(int, long), CpuProcessRow> rows = new();
    readonly Dictionary<string, Bitmap?> icons = new(StringComparer.OrdinalIgnoreCase);
    readonly CpuTopology topology;
    Func<bool>? isVisible;
    Task? processRefresh;
    bool processPickerOpen;
    bool busy;
    bool windowedOnly;
    bool realtimeConfirmed;
    string? search;
    CpuProcessRow? process;
    CpuChoice? priority;
    CpuChoice? ecoMode;
    CpuChoice? memoryPriority;
    CpuProcessState editorBaseline;

    public CpuSchedulingPageViewModel()
    {
        try
        {
            var buffer = OperatingSystem.IsWindows() && !Design.IsDesignMode
                ? IPlatformService.Instance.GetCpuSchedulingTopology()
                : [];
            topology = CpuTopology.Parse(buffer, IntPtr.Size);
        }
        catch (Exception ex)
        {
            Log.Error(nameof(CpuSchedulingPageViewModel), ex, "Could not read CPU topology.");
            topology = CpuTopology.Parse([], IntPtr.Size);
        }

        CoreGrid = CreateCoreGrid();
        CoreGroups = CreateCoreGroups();
        Presets = new CpuChoice[]
        {
            new((int)CpuPreset.All, Strings.GameTools_Cpu_AllCores),
            new((int)CpuPreset.Performance, Strings.GameTools_Cpu_PCores),
            new((int)CpuPreset.Efficiency, Strings.GameTools_Cpu_ECores),
            new((int)CpuPreset.OneThreadPerCore, Strings.GameTools_Cpu_OneThread),
            new((int)CpuPreset.Ccd0, Strings.GameTools_Cpu_Ccd0),
            new((int)CpuPreset.Ccd1, Strings.GameTools_Cpu_Ccd1),
            new((int)CpuPreset.LeaveFirstCoreFree, Strings.GameTools_Cpu_LeaveFirst),
        }.Where(choice => topology.GetPreset((CpuPreset)choice.Value).Count != 0).ToArray();
#if WINDOWS
        var pickWindow = ReactiveCommand.Create(PickWindow);
        CompositeDisposable.Add(pickWindow);
        PickWindowCommand = pickWindow;
#endif
        refreshTimer.Tick += async (_, _) => await RefreshAsync();
    }

    public ObservableCollection<CpuProcessRow> Processes { get; } = new();
    public ObservableCollection<CpuRuleRow> Rules { get; } = new();
    IReadOnlyList<CpuCoreRow> CoreGrid { get; }
    public IReadOnlyList<CpuCoreGroup> CoreGroups { get; }
    public IReadOnlyList<CpuChoice> Presets { get; }
    public ICommand? PickWindowCommand { get; }
    public bool IsWindowPickingSupported => PickWindowCommand is not null;
    public bool HasTopology => topology.LogicalProcessors.Count > 0;
    public bool HasMultipleGroups => topology.Groups.Count > 1;
    public bool HasNoTopology => !HasTopology;
    public bool HasNoProcesses => Processes.Count == 0;
    public bool HasNoRules => Rules.Count == 0;
    public bool HasProcess => SelectedProcess is not null;
    public bool HasNoProcess => !HasProcess;
    public bool CanEdit => !Busy && SelectedProcess is { IsProtected: false } row && row.State.CreationTime != 0;
    public bool CanEditAffinity => CanEdit && HasTopology && !HasMultipleGroups;
    public bool CanEditEco => CanEdit && SelectedProcess?.State.EcoMode is not null;
    public bool CanEditMemory => CanEdit && SelectedProcess?.State.MemoryPriority is not null;
    public bool NeedsRealtimeConfirmation => SelectedPriority?.Value == (int)ProcessPriorityClass.RealTime &&
        editorBaseline.Priority != ProcessPriorityClass.RealTime;
    public bool RealtimeConfirmed
    {
        get => realtimeConfirmed;
        set => this.RaiseAndSetIfChanged(ref realtimeConfirmed, value);
    }
    public bool CanMatchPath => SelectedProcess?.ImagePath is not null;
    public bool DisableAllRules { get; private set; }
    public bool RulesAreEditable => !Busy;

    public bool Busy
    {
        get => busy;
        private set
        {
            this.RaiseAndSetIfChanged(ref busy, value);
            NotifySelection();
            this.RaisePropertyChanged(nameof(RulesAreEditable));
        }
    }

    public string? Search
    {
        get => search;
        set
        {
            this.RaiseAndSetIfChanged(ref search, value);
            UpdateProcessList();
        }
    }

    public bool WindowedOnly
    {
        get => windowedOnly;
        set
        {
            this.RaiseAndSetIfChanged(ref windowedOnly, value);
            UpdateProcessList();
        }
    }

    public CpuProcessRow? SelectedProcess
    {
        get => process;
        set
        {
            if (ReferenceEquals(process, value))
                return;
            this.RaiseAndSetIfChanged(ref process, value);
            ReloadEditor();
            NotifySelection();
        }
    }

    public CpuChoice? SelectedPriority
    {
        get => priority;
        set
        {
            if (ReferenceEquals(priority, value))
                return;
            RealtimeConfirmed = false;
            this.RaiseAndSetIfChanged(ref priority, value);
            this.RaisePropertyChanged(nameof(NeedsRealtimeConfirmation));
        }
    }

    public CpuChoice? SelectedEcoMode
    {
        get => ecoMode;
        set => this.RaiseAndSetIfChanged(ref ecoMode, value);
    }

    public CpuChoice? SelectedMemoryPriority
    {
        get => memoryPriority;
        set => this.RaiseAndSetIfChanged(ref memoryPriority, value);
    }

    public IReadOnlyList<CpuChoice> Priorities { get; } =
    [
        new((int)ProcessPriorityClass.Idle, Strings.GameTools_Cpu_Idle),
        new((int)ProcessPriorityClass.BelowNormal, Strings.GameTools_Cpu_BelowNormal),
        new((int)ProcessPriorityClass.Normal, Strings.GameTools_Cpu_Normal),
        new((int)ProcessPriorityClass.AboveNormal, Strings.GameTools_Cpu_AboveNormal),
        new((int)ProcessPriorityClass.High, Strings.GameTools_Cpu_High),
        new((int)ProcessPriorityClass.RealTime, Strings.GameTools_Cpu_Realtime),
    ];

    public IReadOnlyList<CpuChoice> EcoModes { get; } =
    [
        new((int)CpuEcoMode.SystemManaged, Strings.GameTools_Cpu_SystemManaged),
        new((int)CpuEcoMode.Off, Strings.GameTools_Cpu_Off),
        new((int)CpuEcoMode.On, Strings.GameTools_Cpu_On),
    ];

    public IReadOnlyList<CpuChoice> MemoryPriorities { get; } =
    [
        new(1, Strings.GameTools_Cpu_LowestMemory),
        new(2, Strings.GameTools_Cpu_LowMemory),
        new(3, Strings.GameTools_Cpu_MediumMemory),
        new(4, Strings.GameTools_Cpu_BelowNormalMemory),
        new(5, Strings.GameTools_Cpu_Normal),
    ];

    internal static string GetPriorityLabel(ProcessPriorityClass? value) => value switch
    {
        ProcessPriorityClass.Idle => Strings.GameTools_Cpu_Idle,
        ProcessPriorityClass.BelowNormal => Strings.GameTools_Cpu_BelowNormal,
        ProcessPriorityClass.Normal => Strings.GameTools_Cpu_Normal,
        ProcessPriorityClass.AboveNormal => Strings.GameTools_Cpu_AboveNormal,
        ProcessPriorityClass.High => Strings.GameTools_Cpu_High,
        ProcessPriorityClass.RealTime => Strings.GameTools_Cpu_Realtime,
        _ => Strings.GameTools_Cpu_Unknown,
    };

    internal static string GetEcoLabel(CpuEcoMode value) => value switch
    {
        CpuEcoMode.Off => Strings.GameTools_Cpu_Off,
        CpuEcoMode.On => Strings.GameTools_Cpu_On,
        _ => Strings.GameTools_Cpu_SystemManaged,
    };

    IReadOnlyList<CpuCoreRow> CreateCoreGrid()
    {
        var grid = new List<CpuCoreRow>();
        foreach (var core in topology.Cores)
        {
            var first = core.Threads[0];
            var labels = new List<string> { string.Format(Strings.GameTools_Cpu_Core, grid.Count) };
            for (int i = 0; i < topology.L3Groups.Count; i++)
            {
                if (Array.IndexOf(topology.L3Groups[i], first) >= 0)
                    labels.Add(string.Format(Strings.GameTools_Cpu_CacheGroup, i));
            }
            foreach (var node in topology.NumaNodes)
            {
                if (Array.IndexOf(node.Value, first) >= 0)
                    labels.Add(string.Format(Strings.GameTools_Cpu_NumaNode, node.Key));
            }
            if (HasMultipleGroups)
                labels.Add(string.Format(Strings.GameTools_Cpu_CpuGroup, first.Group));

            grid.Add(new(string.Join(" · ", labels), core.Threads.Select(cpu =>
                new CpuTile(string.Format(Strings.GameTools_Cpu_CpuTile, cpu.Index), cpu)).ToArray()));
        }
        return grid;
    }

    IReadOnlyList<CpuCoreGroup> CreateCoreGroups()
    {
        if (!HasTopology)
            return [];
        if (!topology.IsHybrid)
            return [new(Strings.GameTools_Cpu_AllCores, CoreGrid)];

        var performance = topology.GetPreset(CpuPreset.Performance).ToHashSet();
        return
        [
            new(Strings.GameTools_Cpu_PerformanceCores, CoreGrid.Where(core => performance.Contains(core.Tiles[0].Cpu)).ToArray()),
            new(Strings.GameTools_Cpu_EfficiencyCores, CoreGrid.Where(core => !performance.Contains(core.Tiles[0].Cpu)).ToArray()),
        ];
    }

    internal void Start(Func<bool> visible)
    {
        if (!OperatingSystem.IsWindows())
            return;
        isVisible = visible;
        ReloadRules();
        refreshTimer.Start();
        _ = RefreshAsync();
    }

    internal void Stop()
    {
        isVisible = null;
        refreshTimer.Stop();
        SelectedProcess = null;
        processPickerOpen = false;
        Processes.Clear();
        rows.Clear();
        if (processRefresh is not { IsCompleted: false })
            ClearIcons();
    }

    Task RefreshAsync()
    {
        if (isVisible?.Invoke() != true)
            return Task.CompletedTask;
        return processRefresh is { IsCompleted: false } active ? active : processRefresh = RefreshProcessesAsync();
    }

    async Task RefreshProcessesAsync()
    {
        try
        {
            var snapshot = await Task.Run(CaptureSnapshot);
            if (isVisible?.Invoke() != true)
                return;

            var remaining = new HashSet<(int, long)>(rows.Keys);
            foreach (var sample in snapshot)
            {
                var key = (sample.Pid, sample.State.CreationTime);
                remaining.Remove(key);
                if (rows.TryGetValue(key, out var row))
                    row.Update(sample);
                else
                    rows.Add(key, new(sample, HasTopology ? topology.LogicalProcessors.Count : Environment.ProcessorCount));
            }
            foreach (var key in remaining)
                rows.Remove(key);
            if (SelectedProcess is { } selected && !IsCurrentProcess(selected))
                SelectedProcess = null;
            UpdateProcessList();
            NotifySelection();
        }
        catch (Exception ex)
        {
            Log.Error(nameof(CpuSchedulingPageViewModel), ex, "Could not refresh the CPU scheduling page.");
            if (rows.Count == 0 && isVisible?.Invoke() == true)
                Toast.Show(ToastIcon.Error, Strings.GameTools_Cpu_RefreshFailed);
        }
        finally
        {
            if (isVisible is null)
                ClearIcons();
        }
    }

    void ClearIcons()
    {
        foreach (var bitmap in icons.Values.Distinct())
            bitmap?.Dispose();
        icons.Clear();
    }

    CpuProcessSample[] CaptureSnapshot()
    {
        var platform = IPlatformService.Instance;
        var allProcesses = Process.GetProcesses();
        var timestamp = Stopwatch.GetTimestamp();
        var snapshot = new List<CpuProcessSample>(allProcesses.Length);
        try
        {
            foreach (var running in allProcesses)
            {
                try
                {
                    int pid = running.Id;
                    string name = running.ProcessName;
                    var state = platform.GetCpuSchedulingProcessState(pid);
                    if (!state.IsProtected && platform.IsCpuSchedulingProtectedProcess(pid, name))
                        state = state with { IsProtected = true };
                    bool hasWindow = false;
                    long workingSet = -1;
                    try
                    {
                        hasWindow = running.MainWindowHandle != IntPtr.Zero;
                        workingSet = running.WorkingSet64;
                    }
                    catch (Exception ex) when (ex is InvalidOperationException or System.ComponentModel.Win32Exception)
                    {
                    }
                    snapshot.Add(new(pid, name, hasWindow, workingSet, timestamp, state, GetIcon(state.ImagePath)));
                }
                catch (Exception ex) when (ex is InvalidOperationException or System.ComponentModel.Win32Exception)
                {
                    // The process can exit after enumeration.
                }
            }
        }
        finally
        {
            foreach (var running in allProcesses)
                running.Dispose();
        }
        return snapshot.ToArray();
    }

    Bitmap? GetIcon(string? path)
    {
#if WINDOWS
        var key = path ?? string.Empty;
        if (icons.TryGetValue(key, out var bitmap))
            return bitmap;
        try
        {
            using var ico = path is not null && !path.StartsWith(@"\\", StringComparison.Ordinal)
                ? System.Drawing.Icon.ExtractAssociatedIcon(path)
                : null;
            if (ico is not null)
                bitmap = ConvertIcon(ico);
        }
        catch (Exception ex) when (ex is ArgumentException or IOException or UnauthorizedAccessException or System.Runtime.InteropServices.ExternalException)
        {
        }
        if (bitmap is null && path is not null)
            bitmap = GetIcon(null);
        if (bitmap is null && path is null)
            bitmap = ConvertIcon(System.Drawing.SystemIcons.Application);
        icons.Add(key, bitmap);
        return bitmap;
#else
        return null;
#endif
    }

#if WINDOWS
    static Bitmap ConvertIcon(System.Drawing.Icon icon)
    {
        using var image = icon.ToBitmap();
        using var stream = new MemoryStream();
        image.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
        stream.Position = 0;
        return new Bitmap(stream);
    }
#endif

    internal void SetProcessPickerOpen(bool open)
    {
        processPickerOpen = open;
        if (open)
        {
            UpdateProcessList();
            _ = RefreshAsync();
        }
        else
        {
            Processes.Clear();
        }
    }

    void UpdateProcessList()
    {
        if (!processPickerOpen)
            return;

        var filter = Search?.Trim();
        var desired = rows.Values.Where(row => (!WindowedOnly || row.HasWindow) &&
            (string.IsNullOrEmpty(filter) || row.Name.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                row.Pid.ToString(CultureInfo.InvariantCulture).Contains(filter, StringComparison.Ordinal) ||
                row.ImagePath?.Contains(filter, StringComparison.OrdinalIgnoreCase) == true))
            .OrderBy(row => row.Name, StringComparer.OrdinalIgnoreCase).ThenBy(row => row.Pid).ToArray();
        var set = desired.ToHashSet();
        for (int i = Processes.Count - 1; i >= 0; i--)
        {
            if (!set.Contains(Processes[i]))
                Processes.RemoveAt(i);
        }
        for (int i = 0; i < desired.Length; i++)
        {
            if (i < Processes.Count && ReferenceEquals(Processes[i], desired[i]))
                continue;
            int existing = Processes.IndexOf(desired[i]);
            if (existing >= 0)
                Processes.Move(existing, i);
            else
                Processes.Insert(i, desired[i]);
        }
        this.RaisePropertyChanged(nameof(HasNoProcesses));
    }

    bool IsCurrentProcess(CpuProcessRow row) =>
        rows.TryGetValue((row.Pid, row.State.CreationTime), out var current) && ReferenceEquals(current, row);

    internal void SelectProcess(CpuProcessRow row)
    {
        if (IsCurrentProcess(row))
            SelectedProcess = row;
        else
            Toast.Show(ToastIcon.Warning, Strings.GameTools_Cpu_ProcessChanged);
    }

#if WINDOWS
    void PickWindow()
    {
        if (Busy)
            return;
        try
        {
            IPlatformService.Instance.GetMoveMouseDownWindow(window =>
            {
                using var pickedProcess = window.Process;
                if (pickedProcess is not null)
                {
                    int pid = pickedProcess.Id;
                    Dispatcher.UIThread.Post(async () => await SelectProcessAsync(pid));
                }
            });
        }
        catch (Exception ex)
        {
            ex.LogAndShowT(nameof(CpuSchedulingPageViewModel));
        }
    }
#endif

    internal async Task SelectProcessAsync(int pid)
    {
        await RefreshAsync();
        if (isVisible?.Invoke() != true)
            return;
        if (rows.Values.FirstOrDefault(row => row.Pid == pid) is { } selected)
            SelectProcess(selected);
        else
            Toast.Show(ToastIcon.Warning, Strings.GameTools_Cpu_ProcessChanged);
    }

    void NotifySelection()
    {
        this.RaisePropertyChanged(nameof(HasProcess));
        this.RaisePropertyChanged(nameof(HasNoProcess));
        this.RaisePropertyChanged(nameof(CanEdit));
        this.RaisePropertyChanged(nameof(CanEditAffinity));
        this.RaisePropertyChanged(nameof(CanEditEco));
        this.RaisePropertyChanged(nameof(CanEditMemory));
        this.RaisePropertyChanged(nameof(CanMatchPath));
    }

    internal void ReloadEditor()
    {
        RealtimeConfirmed = false;
        editorBaseline = SelectedProcess?.State ?? default;
        SelectedPriority = Priorities.FirstOrDefault(choice => choice.Value == (int?)editorBaseline.Priority);
        SelectedEcoMode = EcoModes.FirstOrDefault(choice => choice.Value == (int?)editorBaseline.EcoMode);
        SelectedMemoryPriority = MemoryPriorities.FirstOrDefault(choice => choice.Value == editorBaseline.MemoryPriority);
        var mask = editorBaseline.AffinityMask.GetValueOrDefault();
        foreach (var tile in CoreGrid.SelectMany(core => core.Tiles))
            tile.IsSelected = (mask & (1UL << tile.Cpu.Index)) != 0;
        this.RaisePropertyChanged(nameof(NeedsRealtimeConfirmation));
    }

    internal void UsePreset(CpuChoice choice)
    {
        if (!CanEditAffinity)
            return;
        var selected = topology.GetPreset((CpuPreset)choice.Value).ToHashSet();
        foreach (var tile in CoreGrid.SelectMany(core => core.Tiles))
            tile.IsSelected = selected.Contains(tile.Cpu);
    }

    ulong SelectedAffinity() => CoreGrid.SelectMany(core => core.Tiles).Where(tile => tile.IsSelected)
        .Aggregate(0UL, (mask, tile) => mask | (1UL << tile.Cpu.Index));

    internal async Task ApplyAsync(bool restoreDefaults)
    {
        if (!CanEdit || SelectedProcess is not { } selected)
            return;
        if (!restoreDefaults && NeedsRealtimeConfirmation && !RealtimeConfirmed)
        {
            Toast.Show(ToastIcon.Warning, Strings.GameTools_Cpu_RealtimeWarning);
            return;
        }

        int requestedPriority = SelectedPriority?.Value != (int?)editorBaseline.Priority ? SelectedPriority?.Value ?? 0 : 0;
        int requestedEco = CanEditEco && SelectedEcoMode?.Value != (int?)editorBaseline.EcoMode ? SelectedEcoMode?.Value ?? -1 : -1;
        int requestedMemory = CanEditMemory && SelectedMemoryPriority?.Value != editorBaseline.MemoryPriority ? SelectedMemoryPriority?.Value ?? 0 : 0;
        ulong mask = CanEditAffinity ? SelectedAffinity() : 0;
        bool changeAffinity = CanEditAffinity && mask != editorBaseline.AffinityMask.GetValueOrDefault();
        if (!restoreDefaults && changeAffinity && mask == 0)
        {
            Toast.Show(ToastIcon.Warning, Strings.GameTools_Cpu_EmptyAffinity);
            return;
        }
        if (!restoreDefaults && requestedPriority == 0 && requestedEco < 0 && requestedMemory == 0 && !changeAffinity)
            return;

        Busy = true;
        try
        {
            int result = await Task.Run(() => IPlatformService.Instance.ApplyCpuSchedulingAsync(selected.Pid, selected.State.CreationTime,
                requestedPriority, changeAffinity ? mask : 0, requestedEco, requestedMemory, restoreDefaults));
            selected.SetWriteStatus(result);
            if (result == CpuSchedulingResult.Success)
            {
                var state = await Task.Run(() => IPlatformService.Instance.GetCpuSchedulingProcessState(selected.Pid));
                if (selected.State.CreationTime == state.CreationTime)
                    selected.UpdateState(state);
                if (ReferenceEquals(SelectedProcess, selected))
                    ReloadEditor();
                Toast.Show(ToastIcon.Success, restoreDefaults ? Strings.GameTools_Cpu_Restored : Strings.GameTools_Cpu_Updated);
            }
            else
            {
                string message = result switch
                {
                    CpuSchedulingResult.AccessDenied => Strings.GameTools_Cpu_AccessDeniedDetails,
                    CpuSchedulingResult.ProtectedProcess => Strings.GameTools_Cpu_Protected,
                    CpuSchedulingResult.ProcessChanged or 1168 => Strings.GameTools_Cpu_ProcessChanged,
                    CpuSchedulingResult.AffinityUnavailable => Strings.GameTools_Cpu_MultiGroup,
                    CpuSchedulingResult.RollbackFailed => Strings.GameTools_Cpu_RollbackFailed,
                    _ => string.Format(Strings.GameTools_Cpu_ApplyFailed, result),
                };
                Toast.Show(ToastIcon.Error, message);
            }
        }
        finally
        {
            Busy = false;
        }
    }

    internal async Task SaveRuleAsync(bool fullPath, bool? gameTemplate)
    {
#if WINDOWS
        if (!CanEdit || SelectedProcess is not { } selected)
            return;
        var path = fullPath ? selected.ImagePath : null;
        var name = Path.GetFileName(selected.ImagePath) ?? selected.Name;
        if (!name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
            name += ".exe";

        var desiredPriority = (ProcessPriorityClass?)SelectedPriority?.Value;
        var desiredEco = (CpuEcoMode?)SelectedEcoMode?.Value;
        ulong mask = CanEditAffinity ? SelectedAffinity() : 0;
        if (gameTemplate.HasValue)
        {
            desiredPriority = gameTemplate.Value ? ProcessPriorityClass.AboveNormal : ProcessPriorityClass.BelowNormal;
            desiredEco = CanEditEco ? (gameTemplate.Value ? CpuEcoMode.Off : CpuEcoMode.On) : null;
            var preset = gameTemplate.Value
                ? (topology.IsHybrid ? CpuPreset.Performance : topology.L3Groups.Count > 1 ? CpuPreset.Ccd0 : CpuPreset.All)
                : (topology.IsHybrid ? CpuPreset.Efficiency : CpuPreset.All);
            mask = CanEditAffinity ? topology.GetPreset(preset).Aggregate(0UL, (m, cpu) => m | (1UL << cpu.Index)) : 0;
        }
        if (desiredPriority == ProcessPriorityClass.RealTime)
        {
            Toast.Show(ToastIcon.Warning, Strings.GameTools_Cpu_RealtimeRule);
            return;
        }
        if (desiredPriority is null && desiredEco is null && mask == 0)
        {
            Toast.Show(ToastIcon.Warning, Strings.GameTools_Cpu_NoSettings);
            return;
        }
        var rule = new CpuSchedulingRule(name, path, desiredPriority, mask == 0 ? null : mask, desiredEco);
        if (await ChangeRulesAsync(() => CpuSchedulingRules.Instance.SaveRuleAsync(rule)))
            Toast.Show(ToastIcon.Success, Strings.GameTools_Cpu_RuleSaved);
#else
        await Task.CompletedTask;
#endif
    }

    internal async Task SetRulesDisabledAsync(bool disabled)
    {
#if WINDOWS
        await ChangeRulesAsync(() => CpuSchedulingRules.Instance.SetDisabledAsync(disabled));
#else
        await Task.CompletedTask;
#endif
    }

    internal async Task ChangeRuleAsync(CpuRuleRow row, bool remove)
    {
#if WINDOWS
        await ChangeRulesAsync(() => remove ? CpuSchedulingRules.Instance.RemoveRuleAsync(row.Rule)
            : CpuSchedulingRules.Instance.SaveRuleAsync(row.Rule with { Enabled = !row.Rule.Enabled }));
#else
        await Task.CompletedTask;
#endif
    }

    void ReloadRules()
    {
#if WINDOWS
        var settings = CpuSchedulingRules.Instance.Settings;
        DisableAllRules = settings.Disabled;
        Rules.Clear();
        foreach (var rule in (settings.Rules ?? []).OrderBy(rule => rule.ExeName, StringComparer.OrdinalIgnoreCase))
            Rules.Add(new(rule));
        this.RaisePropertyChanged(nameof(DisableAllRules));
        this.RaisePropertyChanged(nameof(HasNoRules));
#endif
    }

#if WINDOWS
    async Task<bool> ChangeRulesAsync(Func<Task> change)
    {
        if (Busy)
            return false;
        Busy = true;
        try
        {
            await change();
            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException)
        {
            Log.Error(nameof(CpuSchedulingPageViewModel), ex, "Could not save CPU scheduling rules.");
            Toast.Show(ToastIcon.Error, Strings.GameTools_Cpu_RuleSaveError);
            return false;
        }
        finally
        {
            ReloadRules();
            Busy = false;
        }
    }
#endif
}
