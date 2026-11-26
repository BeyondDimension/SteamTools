using System.Buffers.Binary;
using BD.WTTS.Services.GameTools;

namespace BD.WTTS.UnitTest;

public sealed class CpuTopologyTest
{
    [TestCase(4)]
    [TestCase(8)]
    public void ParseHybridAndMultiGroupTopology(int pointerSize)
    {
        var buffer = Core(0, 0b11, 1, pointerSize)
            .Concat(Core(0, 0b100, 0, pointerSize))
            .Concat(Core(1, 0b10, 0, pointerSize))
            .Concat(NumaNode(0, 0b111, 12, pointerSize))
            .Concat(Cache(1, 0b10, 128, pointerSize))
            .Concat(Cache(0, 0b111, 64, pointerSize))
            .ToArray();
        var topology = CpuTopology.Parse(buffer, pointerSize);
        var p0 = new LogicalCpu(0, 0);
        var p1 = new LogicalCpu(0, 1);
        var e0 = new LogicalCpu(0, 2);
        var e1 = new LogicalCpu(1, 1);

        Assert.Multiple(() =>
        {
            Assert.That(topology.Groups, Is.EqualTo(new ushort[] { 0, 1 }));
            Assert.That(topology.Cores[0].Threads, Is.EqualTo(new[] { p0, p1 }));
            Assert.That(topology.NumaNodes[12], Is.EqualTo(new[] { p0, p1, e0 }));
            Assert.That(topology.GetPreset(CpuPreset.Performance), Is.EqualTo(new[] { p0, p1 }));
            Assert.That(topology.GetPreset(CpuPreset.Efficiency), Is.EqualTo(new[] { e0, e1 }));
            Assert.That(topology.GetPreset(CpuPreset.OneThreadPerCore), Is.EqualTo(new[] { p0, e0, e1 }));
            Assert.That(topology.GetPreset(CpuPreset.LeaveFirstCoreFree), Is.EqualTo(new[] { e0, e1 }));
            Assert.That(topology.GetPreset(CpuPreset.Ccd0), Is.EqualTo(new[] { e1 }));
            Assert.That(topology.GetPreset(CpuPreset.Ccd1), Is.EqualTo(new[] { p0, p1, e0 }));
        });
    }

    [Test]
    public void ParseNumaAndCacheMasksSpanningMultipleGroups()
    {
        const int pointerSize = 8;
        var numa = NumaNode(0, 0b1, 7, pointerSize);
        BinaryPrimitives.WriteInt32LittleEndian(numa, 6);
        var cache = Cache(0, 0b1, 64, pointerSize);
        var topology = CpuTopology.Parse(
            [
                .. Core(0, 0b1, 0, pointerSize),
                .. Core(1, 0b100, 0, pointerSize),
                .. AppendGroupMask(numa, 32, pointerSize),
                .. AppendGroupMask(cache, 40, pointerSize),
            ],
            pointerSize);
        var expected = new[] { new LogicalCpu(0, 0), new LogicalCpu(1, 2) };
        Assert.That(topology.NumaNodes[7], Is.EqualTo(expected));
        Assert.That(topology.GetPreset(CpuPreset.Ccd0), Is.EqualTo(expected));
    }

    [Test]
    public void NonHybridPresetsAreUnavailableAndInvalidRecordsAreRejected()
    {
        var buffer = Core(0, 0b11, 0, 8);
        var topology = CpuTopology.Parse(buffer, 8);
        Assert.Multiple(() =>
        {
            Assert.That(topology.GetPreset(CpuPreset.Performance), Is.Empty);
            Assert.That(topology.GetPreset(CpuPreset.Efficiency), Is.Empty);
            Assert.That(topology.GetPreset(CpuPreset.Ccd1), Is.Empty);
            Assert.That(topology.GetPreset(CpuPreset.LeaveFirstCoreFree), Is.Empty);
            Assert.That(() => CpuTopology.Parse(buffer.AsSpan(0, buffer.Length - 1), 8), Throws.TypeOf<InvalidDataException>());
        });
    }

    static byte[] AppendGroupMask(byte[] firstRecord, int maskOffset, int pointerSize)
    {
        int affinitySize = pointerSize + 8;
        var second = Record(0, maskOffset, 1, 0b100, pointerSize);
        byte[] combined = [.. firstRecord, .. second.AsSpan(maskOffset, affinitySize)];
        BinaryPrimitives.WriteInt32LittleEndian(combined.AsSpan(4), combined.Length);
        BinaryPrimitives.WriteUInt16LittleEndian(combined.AsSpan(maskOffset - 2), 2);
        return combined;
    }

    static byte[] Core(ushort group, byte mask, byte efficiency, int pointerSize)
    {
        var record = Record(0, 32, group, mask, pointerSize);
        record[9] = efficiency;
        BinaryPrimitives.WriteUInt16LittleEndian(record.AsSpan(30), 1);
        return record;
    }

    static byte[] NumaNode(ushort group, byte mask, uint node, int pointerSize)
    {
        var record = Record(1, 32, group, mask, pointerSize);
        BinaryPrimitives.WriteUInt32LittleEndian(record.AsSpan(8), node);
        return record;
    }

    static byte[] Cache(ushort group, byte mask, uint bytes, int pointerSize)
    {
        var record = Record(2, 40, group, mask, pointerSize);
        record[8] = 3;
        BinaryPrimitives.WriteUInt32LittleEndian(record.AsSpan(12), bytes);
        return record;
    }

    static byte[] Record(int relationship, int maskOffset, ushort group, byte mask, int pointerSize)
    {
        var record = new byte[maskOffset + pointerSize + 8];
        BinaryPrimitives.WriteInt32LittleEndian(record, relationship);
        BinaryPrimitives.WriteInt32LittleEndian(record.AsSpan(4), record.Length);
        record[maskOffset] = mask;
        BinaryPrimitives.WriteUInt16LittleEndian(record.AsSpan(maskOffset + pointerSize), group);
        return record;
    }
}
