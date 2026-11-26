using Avalonia.Media.Imaging;
using BD.WTTS.Services.GameTools;
using ReactiveUI.Reactive;
using System.Diagnostics;
using System.Numerics;

namespace BD.WTTS.UI.ViewModels;

internal sealed record CpuProcessSample(int Pid, string Name, bool HasWindow, long WorkingSet,
    long Timestamp, CpuProcessState State, Bitmap? Icon);

public sealed class CpuProcessRow : ReactiveObject
{
    CpuProcessSample sample;
    double? usage;
    bool writeAccessDenied;
    bool protectedWrite;

    internal CpuProcessRow(CpuProcessSample sample, int processorCount)
    {
        this.sample = sample;
        ProcessorCount = processorCount;
    }

    internal CpuProcessState State => sample.State;
    internal bool HasWindow => sample.HasWindow;
    internal int ProcessorCount { get; }
    public int Pid => sample.Pid;
    public string Name => sample.Name;
    public string? ImagePath => State.ImagePath;
    public Bitmap? Icon => sample.Icon;
    public bool IsProtected => State.IsProtected || protectedWrite;
    public string CpuUsage => usage?.ToString("P1") ?? Strings.GameTools_Cpu_Unknown;
    public string Memory => sample.WorkingSet >= 0
        ? string.Format(Strings.GameTools_Cpu_MemoryUsage, sample.WorkingSet / (1024d * 1024d))
        : Strings.GameTools_Cpu_Unknown;
    public string Priority => CpuSchedulingPageViewModel.GetPriorityLabel(State.Priority);
    public string Affinity => State.AffinityMask is ulong mask && ProcessorCount != 0
        ? string.Format(Strings.GameTools_Cpu_AffinitySetting, BitOperations.PopCount(mask), ProcessorCount)
        : Strings.GameTools_Cpu_Unknown;
    public string Status => IsProtected ? Strings.GameTools_Cpu_Protected
        : State.AccessDenied || writeAccessDenied ? Strings.GameTools_Cpu_AccessDenied
        : string.Empty;
    public bool HasStatus => Status.Length != 0;

    internal void Update(CpuProcessSample next)
    {
        var elapsed = Stopwatch.GetElapsedTime(sample.Timestamp, next.Timestamp);
        if (State.CreationTime != 0 && next.State.CreationTime == State.CreationTime && elapsed > TimeSpan.Zero)
        {
            long cpuTime = next.State.CpuTime - State.CpuTime;
            if (cpuTime >= 0)
                usage = Math.Clamp(cpuTime / (elapsed.Ticks * (double)Math.Max(1, ProcessorCount)), 0, 1);
        }
        sample = next;
        this.RaisePropertyChanged(string.Empty);
    }

    internal void SetWriteStatus(int result)
    {
        writeAccessDenied = result == CpuSchedulingResult.AccessDenied;
        protectedWrite |= result == CpuSchedulingResult.ProtectedProcess;
        this.RaisePropertyChanged(nameof(Status));
        this.RaisePropertyChanged(nameof(HasStatus));
    }

    internal void UpdateState(CpuProcessState state)
    {
        sample = sample with { State = state };
        this.RaisePropertyChanged(nameof(Priority));
        this.RaisePropertyChanged(nameof(Affinity));
        this.RaisePropertyChanged(nameof(Status));
        this.RaisePropertyChanged(nameof(HasStatus));
    }
}

public sealed record CpuChoice(int Value, string Label)
{
    public override string ToString() => Label;
}

public sealed class CpuTile : ReactiveObject
{
    bool selected;

    internal CpuTile(string label, LogicalCpu cpu)
    {
        Label = label;
        Cpu = cpu;
    }

    public string Label { get; }
    internal LogicalCpu Cpu { get; }

    public bool IsSelected
    {
        get => selected;
        set => this.RaiseAndSetIfChanged(ref selected, value);
    }
}

public sealed record CpuCoreRow(string Label, IReadOnlyList<CpuTile> Tiles);

public sealed class CpuRuleRow
{
    internal CpuRuleRow(CpuSchedulingRule rule)
    {
        Rule = rule;
        var parts = new List<string>();
        if (rule.Priority.HasValue)
            parts.Add(string.Format(Strings.GameTools_Cpu_PrioritySetting, CpuSchedulingPageViewModel.GetPriorityLabel(rule.Priority)));
        if (rule.AffinityMask.HasValue)
            parts.Add(string.Format(Strings.GameTools_Cpu_SavedAffinity, rule.AffinityMask.Value));
        if (rule.EcoMode.HasValue)
            parts.Add(string.Format(Strings.GameTools_Cpu_EcoSetting, CpuSchedulingPageViewModel.GetEcoLabel(rule.EcoMode.Value)));
        Summary = string.Join(" · ", parts);
    }

    internal CpuSchedulingRule Rule { get; }
    public string Name => Rule.ImagePath ?? Rule.ExeName;
    public string Summary { get; }
    public string ToggleLabel => Rule.Enabled ? Strings.GameTools_Cpu_DisableRule : Strings.GameTools_Cpu_EnableRule;
}
