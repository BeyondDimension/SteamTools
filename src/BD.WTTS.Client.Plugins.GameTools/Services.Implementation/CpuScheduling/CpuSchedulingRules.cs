using BD.WTTS.Plugins;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace BD.WTTS.Services.GameTools;

internal sealed record CpuSchedulingRule(
    string ExeName,
    string? ImagePath,
    ProcessPriorityClass? Priority,
    ulong? AffinityMask,
    CpuEcoMode? EcoMode,
    bool Enabled = true);

internal sealed record CpuSchedulingRuleSettings(
    bool Disabled = false,
    CpuSchedulingRule[]? Rules = null);

[JsonSourceGenerationOptions(WriteIndented = true, UseStringEnumConverter = true)]
[JsonSerializable(typeof(CpuSchedulingRuleSettings))]
internal partial class CpuSchedulingRuleJsonContext : JsonSerializerContext;

#if WINDOWS
internal sealed class CpuSchedulingRules
{
    const int MaxAttempts = 3;
    static readonly TimeSpan pollInterval = TimeSpan.FromSeconds(3);
    static readonly StringComparer pathComparer = StringComparer.OrdinalIgnoreCase;
    static readonly Lazy<CpuSchedulingRules> instance = new(() => new(Plugin.Instance.AppDataDirectory));

    readonly string settingsPath;
    readonly SemaphoreSlim gate = new(1);
    readonly Dictionary<CpuSchedulingRule, long> startTimes = new();
    readonly Dictionary<(int ProcessId, long CreationTime), (CpuSchedulingRule? Rule, int Attempts)> seen = new();
    volatile CpuSchedulingRuleSettings settings;
    CancellationTokenSource? watcher;
    bool stopped;

    CpuSchedulingRules(string directory)
    {
        settingsPath = Path.Combine(directory, "CpuSchedulingRules.json");
        settings = Load();
    }

    public static CpuSchedulingRules Instance => instance.Value;
    public CpuSchedulingRuleSettings Settings => settings;

    public static Task StopIfCreatedAsync() => instance.IsValueCreated
        ? Instance.StopAsync()
        : Task.CompletedTask;

    CpuSchedulingRuleSettings Load()
    {
        try
        {
            if (File.Exists(settingsPath))
            {
                using var file = File.OpenRead(settingsPath);
                var loaded = System.Text.Json.JsonSerializer.Deserialize(file, CpuSchedulingRuleJsonContext.Default.CpuSchedulingRuleSettings);
                if (loaded is not null)
                {
                    return loaded with
                    {
                        Rules = loaded.Rules?.Where(rule => IsValid(rule)).ToArray(),
                    };
                }
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or System.Text.Json.JsonException)
        {
            Log.Error(nameof(CpuSchedulingRules), ex, "Could not read CPU scheduling rules.");
        }
        return new();
    }

    static bool IsValid(CpuSchedulingRule? rule) => rule is not null
        && !string.IsNullOrWhiteSpace(rule.ExeName)
        && string.Equals(rule.ExeName, Path.GetFileName(rule.ExeName), StringComparison.Ordinal)
        && rule.ExeName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)
        && (rule.Priority is null || (Enum.IsDefined(rule.Priority.Value) && rule.Priority != ProcessPriorityClass.RealTime))
        && (rule.EcoMode is null || Enum.IsDefined(rule.EcoMode.Value))
        && rule.AffinityMask != 0
        && (rule.Priority.HasValue || rule.AffinityMask.HasValue || rule.EcoMode.HasValue);

    public async Task StartAsync()
    {
        await gate.WaitAsync().ConfigureAwait(false);
        try
        {
            UpdateWatcher();
        }
        finally
        {
            gate.Release();
        }
    }

    public Task SaveRuleAsync(CpuSchedulingRule rule)
    {
        if (!IsValid(rule))
            throw new ArgumentException("Invalid CPU scheduling rule.", nameof(rule));
        return UpdateAsync(current => current with { Rules = [.. WithoutRule(current, rule), rule] });
    }

    public Task RemoveRuleAsync(CpuSchedulingRule rule) =>
        UpdateAsync(current => current with { Rules = WithoutRule(current, rule).ToArray() });

    public Task SetDisabledAsync(bool disabled) => UpdateAsync(current => current with { Disabled = disabled });

    static IEnumerable<CpuSchedulingRule> WithoutRule(CpuSchedulingRuleSettings settings, CpuSchedulingRule rule) =>
        (settings.Rules ?? []).Where(candidate =>
            !pathComparer.Equals(candidate.ExeName, rule.ExeName) || !pathComparer.Equals(candidate.ImagePath, rule.ImagePath));

    async Task UpdateAsync(Func<CpuSchedulingRuleSettings, CpuSchedulingRuleSettings> update)
    {
        await gate.WaitAsync().ConfigureAwait(false);
        try
        {
            var next = update(settings);
            await Task.Run(() => Save(next)).ConfigureAwait(false);
            settings = next;
            UpdateWatcher();
        }
        finally
        {
            gate.Release();
        }
    }

    void Save(CpuSchedulingRuleSettings next)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(settingsPath)!);
        string tempPath = settingsPath + ".tmp";
        try
        {
            using (var file = File.Create(tempPath))
            {
                System.Text.Json.JsonSerializer.Serialize(file, next, CpuSchedulingRuleJsonContext.Default.CpuSchedulingRuleSettings);
                file.Flush(flushToDisk: true);
            }
            File.Move(tempPath, settingsPath, overwrite: true);
        }
        finally
        {
            File.Delete(tempPath);
        }
    }

    void UpdateWatcher()
    {
        var enabled = stopped || settings.Disabled ? [] : (settings.Rules ?? []).Where(rule => rule.Enabled).ToArray();
        foreach (var oldRule in startTimes.Keys.Except(enabled).ToArray())
            startTimes.Remove(oldRule);
        var now = DateTime.UtcNow.ToFileTimeUtc();
        foreach (var rule in enabled)
            startTimes.TryAdd(rule, now);

        if (enabled.Length == 0)
        {
            watcher?.Cancel();
            watcher?.Dispose();
            watcher = null;
            seen.Clear();
        }
        else if (watcher is null)
        {
            watcher = new();
            var cancellationToken = watcher.Token;
            _ = Task.Run(() => WatchAsync(cancellationToken), cancellationToken);
        }
    }

    async Task WatchAsync(CancellationToken cancellationToken)
    {
        using var timer = new PeriodicTimer(pollInterval);
        try
        {
            while (await timer.WaitForNextTickAsync(cancellationToken).ConfigureAwait(false))
            {
                await gate.WaitAsync(cancellationToken).ConfigureAwait(false);
                try
                {
                    await ApplyNewProcessesAsync(cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    Log.Error(nameof(CpuSchedulingRules), ex, "CPU scheduling rule scan failed.");
                }
                finally
                {
                    gate.Release();
                }
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
    }

    async Task ApplyNewProcessesAsync(CancellationToken cancellationToken)
    {
        var platform = IPlatformService.Instance;
        var candidates = startTimes.Keys.ToLookup(rule => rule.ExeName, pathComparer);
        var livePids = new HashSet<int>();
        var processes = Process.GetProcesses();
        try
        {
            foreach (var process in processes)
            {
                cancellationToken.ThrowIfCancellationRequested();
                int pid = process.Id;
                livePids.Add(pid);
                string name;
                try
                {
                    name = process.ProcessName;
                }
                catch (Exception ex) when (ex is InvalidOperationException or System.ComponentModel.Win32Exception)
                {
                    continue;
                }
                var rules = candidates[name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) ? name : name + ".exe"];
                if (!rules.Any() || platform.IsCpuSchedulingProtectedProcess(pid, name))
                    continue;

                var state = platform.GetCpuSchedulingProcessState(pid);
                if (state.CreationTime == 0 || state.IsProtected || state.ImagePath is null)
                    continue;
                var identity = (pid, state.CreationTime);
                var rule = rules.LastOrDefault(match => pathComparer.Equals(match.ImagePath, state.ImagePath))
                    ?? rules.LastOrDefault(match => match.ImagePath is null);
                var preceding = seen.GetValueOrDefault(identity);
                if (preceding.Attempts >= MaxAttempts)
                    continue;
                if (rule is null || (preceding.Rule is not null && preceding.Rule != rule) ||
                    state.CreationTime <= startTimes[rule] || !pathComparer.Equals(rule.ExeName, Path.GetFileName(state.ImagePath)))
                {
                    seen[identity] = (null, MaxAttempts);
                    continue;
                }

                var result = await platform.ApplyCpuSchedulingAsync(pid, state.CreationTime, (int?)rule.Priority ?? 0,
                    rule.AffinityMask ?? 0, (int?)rule.EcoMode ?? -1, 0, false).ConfigureAwait(false);
                int attempt = result <= 0 ? MaxAttempts : preceding.Attempts + 1;
                seen[identity] = (rule, attempt);
                if (result != 0 && attempt >= MaxAttempts)
                    Log.Warn(nameof(CpuSchedulingRules), $"Could not apply rule for {rule.ExeName} (PID {pid}, error {result}).");
            }
        }
        finally
        {
            foreach (var process in processes)
                process.Dispose();
        }
        // Failed queries must not forget a previously processed instance while its PID still exists.
        foreach (var identity in seen.Keys.Where(identity => !livePids.Contains(identity.ProcessId)).ToArray())
            seen.Remove(identity);
    }

    async Task StopAsync()
    {
        await gate.WaitAsync().ConfigureAwait(false);
        try
        {
            stopped = true;
            UpdateWatcher();
        }
        finally
        {
            gate.Release();
        }
    }
}
#endif
