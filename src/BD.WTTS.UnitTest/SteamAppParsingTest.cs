using System.Text;
using BD.SteamClient.Models;

namespace BD.WTTS.UnitTest;

public sealed class SteamAppParsingTest
{
    static readonly string[] StringPool = ["appinfo", "common", "name", "type"];

    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    public void DisplayLoadReadsAdjacentRecordsWithoutRetainingEditData(int version)
    {
        using var stream = new MemoryStream();
        using (var writer = new BinaryWriter(stream, Encoding.UTF8, true))
        {
            WriteEntry(writer, 10, "中文 Game", version, padding: 13);
            WriteEntry(writer, 20, "Second Game", version, padding: 0);
            writer.Write(0u);
        }
        stream.Position = 0;
        using var reader = new BinaryReader(stream, Encoding.UTF8, true);
        var installedIds = new HashSet<uint> { 20 };
        var first = SteamApp.FromReader(reader, version == 3 ? StringPool : null, installedIds,
            isMagicNumberV2: version == 2, isMagicNumberV3: version == 3)!;
        var second = SteamApp.FromReader(reader, version == 3 ? StringPool : null, installedIds,
            isMagicNumberV2: version == 2, isMagicNumberV3: version == 3)!;
        Assert.Multiple(() =>
        {
            Assert.That(first.AppId, Is.EqualTo(10));
            Assert.That(first.Name, Is.EqualTo("中文 Game"));
            Assert.That(first.IsInstalled, Is.False);
            Assert.That(second.AppId, Is.EqualTo(20));
            Assert.That(second.Name, Is.EqualTo("Second Game"));
            Assert.That(second.IsInstalled, Is.True);
            Assert.That(first.OriginalData, Is.Null);
            Assert.That(first.ChangesData, Is.Null);
        });
        Assert.That(SteamApp.FromReader(reader), Is.Null);
    }

    [Test]
    public void EditLoadPreservesOriginalBytesAndWritesChangedProperties()
    {
        using var stream = new MemoryStream();
        using (var writer = new BinaryWriter(stream, Encoding.UTF8, true))
            WriteEntry(writer, 10, "Original", version: 3, padding: 0);
        var originalPayload = stream.ToArray()[8..];
        stream.Position = 0;
        using var reader = new BinaryReader(stream, Encoding.UTF8, true);
        var app = SteamApp.FromReader(reader, StringPool, isSaveProperties: true, isMagicNumberV3: true)!;
        app.ChangesData!.SetPropertyValue(BD.SteamClient.Enums.SteamAppPropertyType.String,
            "Changed 中文", "appinfo", "common", "name");
        using var output = new MemoryStream();
        var stringTable = new List<string>();
        using (var writer = new BinaryWriter(output, Encoding.UTF8, true))
            app.Write(writer, stringTable, new Dictionary<string, uint>());
        output.Position = 0;
        using var outputReader = new BinaryReader(output, Encoding.UTF8, true);
        var reloaded = SteamApp.FromReader(outputReader, stringTable.ToArray(), isMagicNumberV3: true)!;
        Assert.Multiple(() =>
        {
            Assert.That(app.OriginalData, Is.EqualTo(originalPayload));
            Assert.That(reloaded.AppId, Is.EqualTo(10));
            Assert.That(reloaded.Name, Is.EqualTo("Changed 中文"));
        });
    }

    static void WriteEntry(BinaryWriter writer, uint id, string name, int version, int padding)
    {
        using var payload = new MemoryStream();
        using (var data = new BinaryWriter(payload, Encoding.UTF8, true))
        {
            data.Write(new byte[16]);
            data.Write(new byte[20]);
            data.Write(1u);
            if (version >= 2) data.Write(new byte[20]);
            Key(data, 0, 0);
            Key(data, 0, 1);
            Key(data, 1, 2);
            data.WriteAppInfoString(name);
            Key(data, 1, 3);
            data.WriteAppInfoString("game");
            data.Write(new byte[] { 8, 8, 8 });
            data.Write(new byte[padding]);
        }
        writer.Write(id);
        writer.Write((int)payload.Length);
        writer.Write(payload.GetBuffer(), 0, (int)payload.Length);
        void Key(BinaryWriter data, byte type, int index)
        {
            data.Write(type);
            if (version == 3) data.Write(index);
            else data.WriteAppInfoString(StringPool[index]);
        }
    }
}
