using System.Buffers.Binary;
using System.Numerics;

namespace BD.WTTS.Services.GameTools;

internal readonly record struct LogicalCpu(ushort Group, int Index);

internal sealed record CpuCore(byte EfficiencyClass, IReadOnlyList<LogicalCpu> Threads);

internal enum CpuPreset
{
    All,
    Performance,
    Efficiency,
    OneThreadPerCore,
    Ccd0,
    Ccd1,
    LeaveFirstCoreFree,
}

internal sealed class CpuTopology
{
    CpuTopology(CpuCore[] cores, Dictionary<uint, LogicalCpu[]> numaNodes, LogicalCpu[][] l3Groups)
    {
        Cores = cores;
        NumaNodes = numaNodes;
        L3Groups = l3Groups;
        LogicalProcessors = cores.SelectMany(core => core.Threads).ToArray();
        Groups = LogicalProcessors.Select(cpu => cpu.Group).Distinct().Order().ToArray();
        HighestEfficiencyClass = cores.Select(core => core.EfficiencyClass).DefaultIfEmpty().Max();
        IsHybrid = cores.Any(core => core.EfficiencyClass != HighestEfficiencyClass);
    }

    public IReadOnlyList<CpuCore> Cores { get; }
    public IReadOnlyDictionary<uint, LogicalCpu[]> NumaNodes { get; }
    public IReadOnlyList<LogicalCpu[]> L3Groups { get; }
    public IReadOnlyList<LogicalCpu> LogicalProcessors { get; }
    public IReadOnlyList<ushort> Groups { get; }
    public byte HighestEfficiencyClass { get; }
    public bool IsHybrid { get; }

    public IReadOnlyList<LogicalCpu> GetPreset(CpuPreset preset) => preset switch
    {
        CpuPreset.All => LogicalProcessors,
        CpuPreset.Performance when IsHybrid => Cores.Where(core => core.EfficiencyClass == HighestEfficiencyClass).SelectMany(core => core.Threads).ToArray(),
        CpuPreset.Efficiency when IsHybrid => Cores.Where(core => core.EfficiencyClass < HighestEfficiencyClass).SelectMany(core => core.Threads).ToArray(),
        CpuPreset.OneThreadPerCore => Cores.Select(core => core.Threads[0]).ToArray(),
        CpuPreset.Ccd0 when L3Groups.Count > 0 => L3Groups[0],
        CpuPreset.Ccd1 when L3Groups.Count > 1 => L3Groups[1],
        CpuPreset.LeaveFirstCoreFree => Cores.Skip(1).SelectMany(core => core.Threads).ToArray(),
        _ => [],
    };

    public static CpuTopology Parse(ReadOnlySpan<byte> buffer, int pointerSize)
    {
        if (pointerSize is not (4 or 8))
            throw new ArgumentOutOfRangeException(nameof(pointerSize));

        var cores = new List<CpuCore>();
        var numaNodes = new Dictionary<uint, LogicalCpu[]>();
        var caches = new List<(uint Size, LogicalCpu[] Threads)>();
        while (!buffer.IsEmpty)
        {
            const int headerSize = 8;
            if (buffer.Length < headerSize)
                throw new InvalidDataException("Incomplete processor relationship header.");

            int relationship = BinaryPrimitives.ReadInt32LittleEndian(buffer);
            int size = BinaryPrimitives.ReadInt32LittleEndian(buffer[4..]);
            if (size < headerSize || size > buffer.Length)
                throw new InvalidDataException("Invalid processor relationship size.");

            var record = buffer[..size];
            switch (relationship)
            {
                case 0: // RelationProcessorCore
                    var threads = ReadAffinities(record, 32, pointerSize);
                    if (threads.Length != 0)
                        cores.Add(new(record[9], threads));
                    break;
                case 1: // RelationNumaNode
                case 6: // RelationNumaNodeEx
                    var nodeThreads = ReadAffinities(record, 32, pointerSize);
                    uint node = BinaryPrimitives.ReadUInt32LittleEndian(record[8..]);
                    numaNodes[node] = numaNodes.TryGetValue(node, out var preceding)
                        ? preceding.Union(nodeThreads).ToArray()
                        : nodeThreads;
                    break;
                case 2: // RelationCache
                    if (size <= headerSize)
                        throw new InvalidDataException("Incomplete cache relationship.");
                    if (record[8] == 3)
                    {
                        var cacheThreads = ReadAffinities(record, 40, pointerSize);
                        if (cacheThreads.Length != 0)
                            caches.Add((BinaryPrimitives.ReadUInt32LittleEndian(record[12..]), cacheThreads));
                    }
                    break;
            }
            buffer = buffer[size..];
        }

        // Give the largest L3 cache CCD0. Break ties by logical CPU address.
        var l3Groups = caches.OrderByDescending(cache => cache.Size)
            .ThenBy(cache => cache.Threads[0].Group)
            .ThenBy(cache => cache.Threads[0].Index)
            .Select(cache => cache.Threads)
            .ToArray();
        return new(cores.OrderBy(core => core.Threads[0].Group)
            .ThenBy(core => core.Threads[0].Index).ToArray(), numaNodes, l3Groups);
    }

    static LogicalCpu[] ReadAffinities(ReadOnlySpan<byte> record, int maskOffset, int pointerSize)
    {
        if (record.Length < maskOffset)
            throw new InvalidDataException("Incomplete processor group header.");
        // Older Windows used the GroupCount field as reserved space for cache and NUMA records.
        int groupCount = Math.Max(1, (int)BinaryPrimitives.ReadUInt16LittleEndian(record[(maskOffset - 2)..]));
        int affinitySize = pointerSize + 8;
        if (groupCount > (record.Length - maskOffset) / affinitySize)
            throw new InvalidDataException("Incomplete processor group masks.");

        var threads = new List<LogicalCpu>();
        for (int i = 0; i < groupCount; i++)
        {
            var affinity = record[(maskOffset + i * affinitySize)..];
            ulong mask = pointerSize == 8
                ? BinaryPrimitives.ReadUInt64LittleEndian(affinity)
                : BinaryPrimitives.ReadUInt32LittleEndian(affinity);
            ushort group = BinaryPrimitives.ReadUInt16LittleEndian(affinity[pointerSize..]);
            while (mask != 0)
            {
                threads.Add(new(group, BitOperations.TrailingZeroCount(mask)));
                mask &= mask - 1;
            }
        }
        return threads.OrderBy(cpu => cpu.Group).ThenBy(cpu => cpu.Index).ToArray();
    }
}
