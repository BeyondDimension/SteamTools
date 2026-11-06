using System.Reflection;
using BD.WTTS.Services.Implementation;

namespace BD.WTTS.UnitTest;

public sealed class AcceleratorIpcPathTest
{
    [Test]
    public void AllowsOnlyTheMainProgramAndItsAcceleratorInBothDirections()
    {
        var main = Path.GetFullPath(Path.Combine("test-app", "Steam++.exe"));
        var packaged = Path.Combine(Path.GetDirectoryName(main)!, "modules", "Accelerator", "Steam++.Accelerator.exe");
        var debug = Path.GetFullPath(Path.Combine("test-project", "bin", "Steam++.Accelerator.exe"));
        var unrelated = Path.GetFullPath(Path.Combine("other", "Steam++.Accelerator.exe"));
        var method = typeof(IPCSubProcessServiceImpl).GetMethod("IsAllowedPeerPath", BindingFlags.Static | BindingFlags.NonPublic);

        Assert.That(method, Is.Not.Null, "The IPC path validator must accept explicit paths for verification.");
        bool Allowed(string local, string peer) => (bool)method!.Invoke(null, new object?[] { local, peer, debug })!;

        Assert.Multiple(() =>
        {
            Assert.That(Allowed(main, main), Is.True);
            Assert.That(Allowed(main, packaged), Is.True);
            Assert.That(Allowed(packaged, main), Is.True);
            Assert.That(Allowed(main, debug), Is.True);
            Assert.That(Allowed(debug, main), Is.True);
            Assert.That(Allowed(main, unrelated), Is.False);
            Assert.That(Allowed(unrelated, main), Is.False);
        });
    }
}
