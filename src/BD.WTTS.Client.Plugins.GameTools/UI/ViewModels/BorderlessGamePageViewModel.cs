using ReactiveUI.Reactive;
using System.Windows.Input;

namespace BD.WTTS.UI.ViewModels;

public sealed partial class BorderlessGamePageViewModel : ViewModelBase
{
    #region SelectWindow 变更通知

    private NativeWindowModel? _SelectWindow;

    public NativeWindowModel? SelectWindow
    {
        get => _SelectWindow;
        set
        {
            if (_SelectWindow != value)
            {
                _SelectWindow = value;
                this.RaisePropertyChanged();
                this.RaisePropertyChanged(nameof(HasSelectedWindow));
#if WINDOWS
                SelectControls();
#endif
            }
        }
    }

    #endregion

    ObservableCollection<NativeWindowModel> _WindowList = new();

    public ObservableCollection<NativeWindowModel> WindowList
    {
        get => _WindowList;
        set
        {
            if (_WindowList != value)
            {
                _WindowList = value;
                this.RaisePropertyChanged();
            }
        }
    }

    public bool IsWindowPickingSupported { get; }

    public bool HasSelectedWindow => IsWindowPickingSupported && SelectWindow is not null;

    public ICommand? PickWindowCommand { get; }
    public ICommand? CopyGameInfoCommand { get; }
    public ICommand? BorderlessWindowCommand { get; }
    public ICommand? MaximizeWindowCommand { get; }
    public ICommand? NormalWindowCommand { get; }
    public ICommand? KillWindowCommand { get; }
    public ICommand? ShowWindowCommand { get; }
    public ICommand? HideWindowCommand { get; }
    public ICommand? ToWallpaperWindowCommand { get; }
    public ICommand? ResetWallpaperCommand { get; }
    public ICommand? PauseWallpaperCommand { get; }
    public ICommand? ReattachWallpaperCommand { get; }
    public ICommand? CenterWindowCommand { get; }
    public ICommand? MoveToMonitorCommand { get; }
    public ICommand? FitMonitorCommand { get; }
    public ICommand? SetBoundsCommand { get; }
    public ICommand? RefreshMonitorsCommand { get; }
    public ICommand? SaveRuleCommand { get; }
    public ICommand? ToggleRuleCommand { get; }
    public ICommand? RemoveRuleCommand { get; }

    public IReadOnlyList<WindowPriorityChoice> Priorities { get; } =
    [
        new(NativeWindowBackgroundPriority.Unchanged, Strings.GameTools_Win_PriorityUnchanged),
        new(NativeWindowBackgroundPriority.BelowNormal, Strings.GameTools_Win_PriorityBelowNormal),
        new(NativeWindowBackgroundPriority.Idle, Strings.GameTools_Win_PriorityIdle),
    ];

    IReadOnlyList<NativeWindowMonitor> monitors = [];
    public IReadOnlyList<NativeWindowMonitor> Monitors => monitors;
    public ObservableCollection<WindowRuleChoice> WindowRules { get; } = new();

    WindowRuleChoice? selectedRule;
    public WindowRuleChoice? SelectedRule
    {
        get => selectedRule;
        set
        {
            this.RaiseAndSetIfChanged(ref selectedRule, value);
            this.RaisePropertyChanged(nameof(HasSelectedRule));
        }
    }
    public bool HasSelectedRule => SelectedRule is not null;

    BorderlessWindowControls? controls;
    public BorderlessWindowControls? Controls => controls;

    public BorderlessGamePageViewModel()
    {
#if WINDOWS
        IsWindowPickingSupported = true;
        PickWindowCommand = Register(PickWindow);
        BorderlessWindowCommand = Register(BorderlessWindow);
        MaximizeWindowCommand = Register(MaximizeWindow);
        NormalWindowCommand = Register(NormalWindow);
        KillWindowCommand = Register(KillWindow);
        ShowWindowCommand = Register(ShowWindow);
        HideWindowCommand = Register(HideWindow);
        ToWallpaperWindowCommand = Register(ToWallpaperWindow);
        ResetWallpaperCommand = Register(ResetWallpaper);
        PauseWallpaperCommand = Register(() => Apply(windowApi.PauseWallpaperWindow));
        ReattachWallpaperCommand = Register(ToWallpaperWindow);
        CenterWindowCommand = Register(() => MoveToMonitor(NativeWindowMonitorAction.Center));
        MoveToMonitorCommand = Register(() => MoveToMonitor(NativeWindowMonitorAction.Move));
        FitMonitorCommand = Register(() => MoveToMonitor(NativeWindowMonitorAction.Fit));
        SetBoundsCommand = Register(SetBounds);
        RefreshMonitorsCommand = Register(RefreshMonitors);
        SaveRuleCommand = Register(SaveRule);
        ToggleRuleCommand = Register(ToggleRule);
        RemoveRuleCommand = Register(RemoveRule);
        RefreshMonitors();
        foreach (var rule in (GameWindowToolsSettings.Rules.Value ?? []).OrderBy(rule => rule.ProcessName, StringComparer.OrdinalIgnoreCase))
            WindowRules.Add(new(rule));
        var copy = ReactiveCommand.CreateFromTask(CopyGameInfoAsync);
        CompositeDisposable.Add(copy);
        CopyGameInfoCommand = copy;
#endif
    }

#if WINDOWS
    readonly IPlatformService windowApi = IPlatformService.Instance!;
    readonly Dictionary<NativeWindowModel, BorderlessWindowControls> windowControls = new();

    ICommand Register(Action action)
    {
        var command = ReactiveCommand.Create(action);
        CompositeDisposable.Add(command);
        return command;
    }

    async Task CopyGameInfoAsync()
    {
        if (SelectWindow is not { } window)
            return;

        var text = $"{Strings.GameRelated_Borderless_Title}: {window.Title}\n" +
                   $"{Strings.GameRelated_Borderless_Process}: {window.Name}\n" +
                   $"{Strings.GameRelated_Borderless_ClassName}: {window.ClassName}\n" +
                   $"{Strings.GameRelated_Borderless_Path}: {window.Path}";
        await Clipboard2.SetTextAsync(text);
        Toast.Show(ToastIcon.Success, Strings.CopyToClipboard);
    }

    void PickWindow()
    {
        windowApi.GetMoveMouseDownWindow((window) =>
        {
            var previous = WindowList.FirstOrDefault(item => item.Handle == window.Handle && item.Process?.Id == window.Process?.Id);
            if (previous is null)
                WindowList.Insert(0, window);
            else
                window.Process?.Dispose();
            SelectWindow = previous ?? window;
            ReadSelectedWindow();
        });
    }

    void BorderlessWindow()
    {
        if (SelectWindow == null)
        {
            return;
        }
        windowApi.BorderlessWindow(SelectWindow);
        ReadSelectedWindow();
    }

    void MaximizeWindow()
    {
        if (SelectWindow == null)
        {
            return;
        }
        windowApi.MaximizeWindow(SelectWindow);
    }

    void NormalWindow()
    {
        if (Apply(windowApi.RestoreWindow) && Controls is { } state)
            state.FitMonitor = null;
    }

    void KillWindow()
    {
        if (SelectWindow == null)
        {
            return;
        }
        SelectWindow.Kill();
    }

    void ShowWindow()
    {
        if (SelectWindow == null)
        {
            return;
        }
        windowApi.ShowWindow(SelectWindow);
    }

    void HideWindow()
    {
        if (SelectWindow == null)
        {
            return;
        }
        windowApi.HideWindow(SelectWindow);
    }

    void ToWallpaperWindow()
    {
        if (Controls is { Monitor: { } monitor } state)
            Apply(window => windowApi.SetWallpaperWindow(window, monitor, state.WallpaperMouse, state.WallpaperKeyboard));
    }

    void ResetWallpaper()
    {
        windowApi.ResetWallerpaper();
        ReadSelectedWindow();
    }

    void SelectControls()
    {
        if (SelectWindow is not { } window)
        {
            controls = null;
            this.RaisePropertyChanged(nameof(Controls));
            return;
        }
        if (!windowControls.TryGetValue(window, out var state))
        {
            state = new(Priorities, (previous, options) =>
            {
                var updated = previous.ClickThrough != options.ClickThrough
                    ? windowApi.SetWindowPenetrate(window, options.ClickThrough)
                    : windowApi.SetWindowToolOptions(window, options);
                if (updated)
                    return true;
                ShowOperationError();
                return false;
            }, () =>
            {
                if (windowControls.TryGetValue(window, out var updated) && (updated.WallpaperAttached || updated.WallpaperPaused))
                    windowApi.SetWallpaperInput(window, updated.WallpaperMouse, updated.WallpaperKeyboard);
            })
            {
                Monitor = Monitors.FirstOrDefault(),
                AutoApply = FindRule(window.Name)?.Rule.AutoApply == true,
                FitMonitor = FindRule(window.Name)?.Rule.FitMonitor,
            };
            windowControls.Add(window, state);
        }
        controls = state;
        ReadSelectedWindow();
        this.RaisePropertyChanged(nameof(Controls));
    }

    void ReadSelectedWindow()
    {
        if (SelectWindow is { } window && windowApi.GetWindowToolInfo(window) is { } info)
            controls?.Read(info);
    }

    bool Apply(Func<NativeWindowModel, bool> action)
    {
        if (SelectWindow is not { } window)
            return false;
        var success = action(window);
        if (!success)
            ShowOperationError();
        ReadSelectedWindow();
        return success;
    }

    static void ShowOperationError() => Toast.Show(ToastIcon.Error, Strings.GameTools_Win_OperationFailed);

    void RefreshMonitors()
    {
        var selection = windowControls.Values.Select(control => (Control: control, Device: control.Monitor?.DeviceName)).ToArray();
        monitors = windowApi.GetWindowMonitors();
        this.RaisePropertyChanged(nameof(Monitors));
        foreach (var (control, device) in selection)
            control.Monitor = Monitors.FirstOrDefault(monitor => monitor.DeviceName == device) ?? Monitors.FirstOrDefault();
        ReadSelectedWindow();
    }

    void MoveToMonitor(NativeWindowMonitorAction action)
    {
        if (Controls is { Monitor: { } monitor } state &&
            Apply(window => windowApi.MoveWindowToMonitor(window, monitor, action)))
            state.FitMonitor = action == NativeWindowMonitorAction.Fit ? monitor.DeviceName : null;
    }

    void SetBounds()
    {
        if (Controls is not { X: { } x, Y: { } y, Width: { } width, Height: { } height } state || width < 1 || height < 1)
        {
            ShowOperationError();
            return;
        }
        if (Apply(window => windowApi.SetWindowToolBounds(window, new((int)x, (int)y, (int)width, (int)height))))
            state.FitMonitor = null;
    }

    WindowRuleChoice? FindRule(string? processName) =>
        WindowRules.FirstOrDefault(item => string.Equals(item.Rule.ProcessName, processName, StringComparison.OrdinalIgnoreCase));

    void SaveRule()
    {
        if (SelectWindow is not { Name: { Length: > 0 } processName } window || Controls is not { } state ||
            windowApi.GetWindowToolInfo(window) is not { } info)
        {
            ShowOperationError();
            return;
        }

        var options = info.Options;
        SaveRule(new(processName, state.AutoApply, info.Borderless, options.Topmost, options.Opacity, state.FitMonitor,
            options.MuteInBackground, options.BackgroundPriority));
    }

    void SaveRule(NativeWindowRule rule)
    {
        var choice = new WindowRuleChoice(rule);
        if (FindRule(rule.ProcessName) is { } previous)
            WindowRules[WindowRules.IndexOf(previous)] = choice;
        else
            WindowRules.Add(choice);
        SelectedRule = choice;
        PersistRules();
    }

    void ToggleRule()
    {
        if (SelectedRule is { Rule: var rule })
            SaveRule(rule with { AutoApply = !rule.AutoApply });
    }

    void RemoveRule()
    {
        if (SelectedRule is not { } rule)
            return;
        WindowRules.Remove(rule);
        SelectedRule = null;
        PersistRules();
    }

    void PersistRules()
    {
        var rules = WindowRules.Select(rule => rule.Rule).ToList();
        GameWindowToolsSettings.Rules.Value = rules;
        windowApi.SetWindowRules(rules);
        foreach (var (window, state) in windowControls)
            state.AutoApply = FindRule(window.Name)?.Rule.AutoApply == true;
    }
#endif
}
