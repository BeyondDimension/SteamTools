using BD.WTTS.Services;
using System.Text;

namespace BD.WTTS.UnitTest;

public sealed class LogConsoleBufferTest
{
    [Test]
    public void ReadingAfterEveryWriteKeepsOnlyRecentLogs()
    {
        var source = new LogConsoleService.Source();
        var bytes = Encoding.UTF8.GetBytes(new string('a', 8191) + "\n");
        for (int i = 0; i < 160; i++)
        {
            source.Append(bytes);
            var snapshot = source.ReadSince(-1);
            Assert.That(snapshot.Utf8.Length, Is.LessThanOrEqualTo(LogConsoleService.Source.MaxBytes));
        }
        source.Append("newest line"u8);
        Assert.That(Encoding.UTF8.GetString(source.ReadSince(-1).Utf8), Does.EndWith("newest line"));
    }

    [Test]
    public void OversizedEntryKeepsValidUtf8AndLatestContent()
    {
        var source = new LogConsoleService.Source();
        source.Append("x"u8);
        var text = string.Concat(Enumerable.Repeat("🙂", 100_000)) + "end";
        source.Append(Encoding.UTF8.GetBytes(text));
        var tail = source.ReadSince(0);
        var snapshot = Encoding.UTF8.GetString(tail.Utf8);
        Assert.That(tail.Reset, Is.True);
        Assert.That(snapshot, Does.StartWith("🙂"));
        Assert.That(snapshot, Does.EndWith("🙂end"));
        Assert.That(snapshot, Does.Not.Contain("\uFFFD"));
        Assert.That(tail.Utf8.Length, Is.InRange(LogConsoleService.Source.MaxBytes - 3, LogConsoleService.Source.MaxBytes));
    }

    [Test]
    public void ReaderContinuesAcrossRolloverOrRecoversFromAnExpiredCursor()
    {
        var source = new LogConsoleService.Source();
        source.Append("old line\n"u8);
        var expired = source.ReadSince(-1);
        var block = Encoding.UTF8.GetBytes(new string('a', 4095) + "\n");
        for (var i = 0; i < LogConsoleService.Source.MaxBytes / block.Length - 1; i++)
            source.Append(block);
        var caughtUp = source.ReadSince(expired.End);
        Assert.That(caughtUp.Reset, Is.False);
        Assert.That(caughtUp.Utf8.AsSpan().StartsWith(block), Is.True);

        source.Append(block);
        source.Append("最新🙂\n"u8);
        var delta = source.ReadSince(caughtUp.End);
        Assert.That(delta.Reset, Is.False);
        Assert.That(Encoding.UTF8.GetString(delta.Utf8), Is.EqualTo(Encoding.UTF8.GetString(block) + "最新🙂\n"));
        var idle = source.ReadSince(delta.End);
        Assert.That(idle.Reset, Is.False);
        Assert.That(idle.Utf8, Is.Empty);

        var retained = source.ReadSince(expired.End);
        Assert.That(retained.Reset, Is.True);
        var text = Encoding.UTF8.GetString(retained.Utf8);
        Assert.That(text, Does.StartWith(Encoding.UTF8.GetString(block)).And.EndWith("最新🙂\n").And.Not.Contain("old line"));
    }

    [Test]
    public void ConcurrentWritersAndReadersKeepAllCompleteEntriesBelowTheLimit()
    {
        var source = new LogConsoleService.Source();
        Parallel.For(0, 400, i =>
        {
            source.Append(Encoding.UTF8.GetBytes($"line {i}\n"));
            _ = source.ToString();
        });
        var lines = source.ToString()!.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        Assert.That(lines, Is.EquivalentTo(Enumerable.Range(0, 400).Select(i => $"line {i}")));
    }
}
