using BD.WTTS.Logging;
using System.Text;

namespace BD.WTTS.UnitTest;

public sealed class LogViewerTest
{
    [Test]
    public void BufferDeltasSkipOldTextAndSignalReplacement()
    {
        var buffer = new LogTextBuffer();
        buffer.Append("first\r\n");
        var first = buffer.ReadSince(0);
        buffer.Append("你好\n");
        var delta = buffer.ReadSince(first.End);
        Assert.That(delta.Text, Is.EqualTo("你好\n"));
        Assert.That(buffer.ReadSince(delta.End).Text, Is.Empty);

        buffer.Replace("new session\n");
        var replacement = buffer.ReadSince(delta.End);
        Assert.That(replacement.First, Is.GreaterThan(delta.End));
        Assert.That(replacement.Text, Is.EqualTo("new session\n"));
        buffer.Clear();
        var cleared = buffer.ReadSince(replacement.End);
        Assert.That(cleared.First, Is.GreaterThan(replacement.End));
        Assert.That(cleared.Text, Is.Empty);
    }

    [Test]
    public void EvictionPreservesTheNewestSurrogatePairsAcrossAReadCursor()
    {
        var buffer = new LogTextBuffer();
        var head = new string('a', LogTextBuffer.MaxCharacters - 2);
        buffer.Append("🙂" + head);
        var before = buffer.ReadSince(0);
        buffer.Append("b");
        var after = buffer.ReadSince(before.End);
        Assert.That(after.First, Is.EqualTo(2));
        Assert.That(after.Text, Is.EqualTo("b"));
        Assert.That(buffer.ReadSince(0).Text, Is.EqualTo(head + "b"));

        buffer.Append("\nWarning: final line\n");
        var wrapped = buffer.ReadSince(0);
        Assert.That(wrapped.Text, Is.EqualTo((head + "b\nWarning: final line\n")[^LogTextBuffer.MaxCharacters..]));
    }

    [Test]
    public async Task DiscoveryListsKnownSourcesAndSortsNewestFirst()
    {
        var folder = Directory.CreateTempSubdirectory();
        try
        {
            var logRoot = folder.CreateSubdirectory("Logs");
            var accelerator = logRoot.CreateSubdirectory("Accelerator");
            var unrelated = logRoot.CreateSubdirectory("Other");
            var asf = folder.CreateSubdirectory("asf");
            var main = Path.Combine(logRoot.FullName, "main.log");
            var proxy = Path.Combine(accelerator.FullName, "service.log");
            var asfLog = Path.Combine(asf.FullName, "log.txt");
            await File.WriteAllTextAsync(main, "main");
            await File.WriteAllTextAsync(proxy, "proxy");
            await File.WriteAllTextAsync(asfLog, "asf");
            await File.WriteAllTextAsync(Path.Combine(unrelated.FullName, "unrelated.log"), "excluded");
            await File.WriteAllTextAsync(Path.Combine(asf.FullName, "unrelated.txt"), "excluded");
            var start = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            File.SetLastWriteTimeUtc(main, start);
            File.SetLastWriteTimeUtc(proxy, start.AddMinutes(1));
            File.SetLastWriteTimeUtc(asfLog, start.AddMinutes(2));

            var result = await LogFileReader.DiscoverAsync(logRoot.FullName, asf.FullName, CancellationToken.None);
            Assert.That(result.Files.Select(file => file.DisplayPath), Is.EqualTo(new[]
            {
                Path.Combine("ASF", "log.txt"), Path.Combine("Accelerator", "service.log"), "main.log",
            }));
        }
        finally { folder.Delete(recursive: true); }
    }

    [Test]
    public async Task TailIsReadableWhileWritingAndDropsPartialUtf8AtEitherEnd()
    {
        var folder = Directory.CreateTempSubdirectory();
        try
        {
            var path = Path.Combine(folder.FullName, "active.log");
            await using var writer = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.Read);
            // The tail starts inside an emoji and ends with an unfinished UTF-8 code point.
            await writer.WriteAsync(Encoding.UTF8.GetBytes("🙂" + new string('a', LogFileReader.MaxTailBytes - 7) + "end"));
            await writer.WriteAsync(new byte[] { 0xf0, 0x9f });
            await writer.FlushAsync();
            var (files, _) = await LogFileReader.DiscoverAsync(folder.FullName, null, CancellationToken.None);
            var tail = await LogFileReader.ReadTailAsync(files.Single(), CancellationToken.None);
            Assert.That(tail.IsTail, Is.True);
            Assert.That(tail.Text, Is.EqualTo(new string('a', LogFileReader.MaxTailBytes - 7) + "end"));
        }
        finally { folder.Delete(recursive: true); }
    }
}
