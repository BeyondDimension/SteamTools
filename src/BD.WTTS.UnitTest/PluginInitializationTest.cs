using System.Reflection;
using BD.WTTS.Plugins;
using BD.WTTS.Plugins.Abstractions;

namespace BD.WTTS.UnitTest;

public sealed class PluginInitializationTest
{
    [Test]
    public async Task WaitingPluginDoesNotPreventOtherPluginsFromStarting()
    {
        var waiting = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var started = new List<string>();
        var blocked = new FixturePlugin("blocked", () => { started.Add("blocked"); return new ValueTask(waiting.Task); });
        var throwing = new FixturePlugin("failed", () => { started.Add("failed"); throw new InvalidOperationException("Plugin fixture failed"); });
        var local = new FixturePlugin("local", () => { started.Add("local"); return ValueTask.CompletedTask; });

        var initialize = typeof(PluginsCore).GetMethod("InitializePluginsAsync", BindingFlags.Static | BindingFlags.NonPublic)!;
        var initialization = (Task)initialize.Invoke(null, new object[] { new IPlugin[] { blocked, throwing, local } })!;
        try
        {
            Assert.Multiple(() =>
            {
                Assert.That(started, Is.EqualTo(new[] { "blocked", "failed", "local" }));
                Assert.That(initialization.IsCompleted, Is.False);
            });
        }
        finally { waiting.TrySetResult(); }
        await initialization.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.That(local.Calls, Is.EqualTo(1));
    }
    sealed class FixturePlugin(string name, Func<ValueTask> initialize) : PluginBase
    {
        public int Calls { get; private set; }
        public override string Name => name;
        public override string UniqueEnglishName => name;
        public override string Version => "1.0.0";
        public override string AssemblyLocation => typeof(FixturePlugin).Assembly.Location;
        public override ValueTask OnInitializeAsync()
        {
            Calls++;
            return initialize();
        }
    }
}
