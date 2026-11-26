using ReactiveUI.Reactive;

namespace BD.WTTS.UI.ViewModels;

public sealed record WindowPriorityChoice(NativeWindowBackgroundPriority Priority, string Label);

public sealed record WindowRuleChoice(NativeWindowRule Rule)
{
    public string Label => string.Format(Rule.AutoApply ? Strings.GameTools_Win_RuleEnabled : Strings.GameTools_Win_RuleDisabled,
        Rule.ProcessName);
}

public sealed class BorderlessWindowControls : ReactiveObject
{
    NativeWindowToolOptions options = new();
    readonly Func<NativeWindowToolOptions, NativeWindowToolOptions, bool> applyOptions;
    readonly Action wallpaperInputChanged;
    readonly IReadOnlyList<WindowPriorityChoice> priorities;
    bool wallpaperMouse = true, wallpaperKeyboard, wallpaperAttached, wallpaperPaused, autoApply;
    decimal? x, y, width, height;
    NativeWindowMonitor? monitor;

    internal BorderlessWindowControls(IReadOnlyList<WindowPriorityChoice> priorities,
        Func<NativeWindowToolOptions, NativeWindowToolOptions, bool> applyOptions, Action wallpaperInputChanged)
    {
        this.priorities = priorities;
        this.applyOptions = applyOptions;
        this.wallpaperInputChanged = wallpaperInputChanged;
    }

    public bool Topmost
    {
        get => options.Topmost;
        set => SetOptions(options with { Topmost = value });
    }

    public double Opacity
    {
        get => options.Opacity;
        set => SetOptions(options with { Opacity = (int)Math.Round(value) });
    }

    public bool ClickThrough
    {
        get => options.ClickThrough;
        set => SetOptions(options with { ClickThrough = value });
    }

    public bool ConfineCursor
    {
        get => options.ConfineCursor;
        set => SetOptions(options with { ConfineCursor = value });
    }

    public bool MuteInBackground
    {
        get => options.MuteInBackground;
        set => SetOptions(options with { MuteInBackground = value });
    }

    public WindowPriorityChoice Priority
    {
        get => priorities.FirstOrDefault(item => item.Priority == options.BackgroundPriority) ?? priorities[0];
        set
        {
            if (value is not null)
                SetOptions(options with { BackgroundPriority = value.Priority });
        }
    }

    public bool WallpaperMouse
    {
        get => wallpaperMouse;
        set
        {
            if (wallpaperMouse != value)
            {
                this.RaiseAndSetIfChanged(ref wallpaperMouse, value);
                wallpaperInputChanged();
            }
        }
    }

    public bool WallpaperKeyboard
    {
        get => wallpaperKeyboard;
        set
        {
            if (wallpaperKeyboard != value)
            {
                this.RaiseAndSetIfChanged(ref wallpaperKeyboard, value);
                wallpaperInputChanged();
            }
        }
    }

    public bool WallpaperAttached
    {
        get => wallpaperAttached;
        private set => this.RaiseAndSetIfChanged(ref wallpaperAttached, value);
    }

    public bool WallpaperPaused
    {
        get => wallpaperPaused;
        private set => this.RaiseAndSetIfChanged(ref wallpaperPaused, value);
    }

    public bool AutoApply
    {
        get => autoApply;
        set => this.RaiseAndSetIfChanged(ref autoApply, value);
    }

    public decimal? X { get => x; set => this.RaiseAndSetIfChanged(ref x, value); }
    public decimal? Y { get => y; set => this.RaiseAndSetIfChanged(ref y, value); }
    public decimal? Width { get => width; set => this.RaiseAndSetIfChanged(ref width, value); }
    public decimal? Height { get => height; set => this.RaiseAndSetIfChanged(ref height, value); }
    public NativeWindowMonitor? Monitor
    {
        get => monitor;
        set => this.RaiseAndSetIfChanged(ref monitor, value);
    }

    internal string? FitMonitor { get; set; }

    internal void Read(NativeWindowToolInfo info)
    {
        X = info.Bounds.X;
        Y = info.Bounds.Y;
        Width = info.Bounds.Width;
        Height = info.Bounds.Height;
        WallpaperAttached = info.WallpaperAttached;
        WallpaperPaused = info.WallpaperPaused;
        if (options != info.Options)
        {
            options = info.Options;
            NotifyOptions();
        }
    }

    void SetOptions(NativeWindowToolOptions value)
    {
        if (value != options)
        {
            if (applyOptions(options, value))
                options = value;
            NotifyOptions();
        }
    }

    void NotifyOptions()
    {
        this.RaisePropertyChanged(nameof(Topmost));
        this.RaisePropertyChanged(nameof(Opacity));
        this.RaisePropertyChanged(nameof(ClickThrough));
        this.RaisePropertyChanged(nameof(ConfineCursor));
        this.RaisePropertyChanged(nameof(MuteInBackground));
        this.RaisePropertyChanged(nameof(Priority));
    }
}
