using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.CompilerServices;
using BD.WTTS.Models;
using BD.WTTS.Services;
using BD.WTTS.UI.ViewModels.Abstractions;
using MemoryPack;
using ReactiveUI.Reactive.Builder;
using System.Reactive.Concurrency;

namespace BD.WTTS.UnitTest;

public sealed class ViewModelSnapshotLifetimeTest
{
    [OneTimeSetUp]
    public void InitializeReactiveUI() => RxAppBuilder.CreateReactiveUIBuilder()
        .WithMainThreadScheduler(ImmediateScheduler.Instance).BuildApp();

    [Test]
    public void RestoredPlatformSettingsRaisePropertyChangedOnlyForNewValues()
    {
        var saved = new PlatformSettings { PlatformPath = "Steam" };
        var restored = MemoryPackSerializer.Deserialize<PlatformSettings>(MemoryPackSerializer.Serialize(saved))!;
        Assert.That(restored.PlatformPath, Is.EqualTo("Steam"));

        var changed = new List<string?>();
        restored.PropertyChanged += (_, args) => changed.Add(args.PropertyName);
        restored.PlatformPath = "Steam";
        restored.PlatformPath = "SteamNew";
        restored.PlatformPath = "SteamNew";

        Assert.That(changed, Is.EqualTo(new[] { nameof(PlatformSettings.PlatformPath) }));
    }

    [TestCase(false)]
    [TestCase(true)]
    public void RestoredViewModelDoesNotRetainItsSerializedCopy(bool runtimeType)
    {
        var state = RestoreLargeSnapshot(runtimeType);
        using var manager = state.Manager;
        using var restored = state.Restored;
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        Assert.Multiple(() =>
        {
            Assert.That(state.Snapshot.TryGetTarget(out _), Is.False,
                "Restoring a large page must release the redundant serialized payload.");
            Assert.That(restored.Payload[0], Is.EqualTo(37));
            Assert.That(restored.Payload[^1], Is.EqualTo(91));
            Assert.That(manager.Get<SnapshotViewModel>(), Is.SameAs(restored));
        });
        manager.Dispose(restored);
        using var next = (SnapshotViewModel)manager.Get(typeof(SnapshotViewModel));
        Assert.That(next.Payload, Is.EqualTo(restored.Payload));
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    static (IViewModelManager Manager, SnapshotViewModel Restored, WeakReference<byte[]> Snapshot) RestoreLargeSnapshot(bool runtimeType)
    {
        var type = typeof(IViewModelManager).Assembly.GetType("BD.WTTS.Services.Implementation.ViewModelManager", true)!;
        var manager = (IViewModelManager)Activator.CreateInstance(type, nonPublic: true)!;
        using var source = new SnapshotViewModel { Payload = new byte[2 * 1024 * 1024] };
        source.Payload[0] = 37;
        source.Payload[^1] = 91;
        manager.Dispose(source);
        var data = (ConcurrentDictionary<Type, byte[]>)type.GetField("viewModelDataCaches", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(manager)!;
        var weak = new WeakReference<byte[]>(data[typeof(SnapshotViewModel)]);
        var restored = runtimeType ? (SnapshotViewModel)manager.Get(typeof(SnapshotViewModel)) : manager.Get<SnapshotViewModel>();
        return (manager, restored, weak);
    }
}

[MemoryPackable]
public sealed partial class SnapshotViewModel : ViewModelBase
{
    public byte[] Payload { get; set; } = [];
}
