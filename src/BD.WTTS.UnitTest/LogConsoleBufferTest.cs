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
        var limit = Encoding.UTF8.GetMaxByteCount(100_000);
        for (int i = 0; i < 160; i++)
        {
            source.Append(bytes);
            var snapshot = source.ToString()!;
            Assert.That(snapshot.Length, Is.LessThanOrEqualTo(limit));
        }
        source.Append("newest line"u8.ToArray());
        Assert.That(source.ToString(), Does.EndWith("newest line"));
    }

    [Test]
    public void OversizedEntryKeepsValidUtf8AndLatestContent()
    {
        var source = new LogConsoleService.Source();
        source.Append("x"u8.ToArray());
        var text = string.Concat(Enumerable.Repeat("🙂", 100_000)) + "end!";
        source.Append(Encoding.UTF8.GetBytes(text));
        var snapshot = source.ToString()!;
        Assert.That(snapshot, Does.StartWith("🙂"));
        Assert.That(snapshot, Does.EndWith("🙂end!"));
        Assert.That(snapshot, Does.Not.Contain("\uFFFD"));
        Assert.That(Encoding.UTF8.GetByteCount(snapshot), Is.LessThanOrEqualTo(Encoding.UTF8.GetMaxByteCount(100_000)));
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
