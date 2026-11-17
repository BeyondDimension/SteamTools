using BD.WTTS.Models;
using MessagePack;
using MessagePack.Formatters;
using MessagePack.Resolvers;

namespace BD.WTTS.Serialization;

internal static class AccelerateCacheSerializer
{
    // Keep the existing MessagePack/LZ4 cache wire format without constructing collection formatters at runtime.
    static readonly MessagePackSerializerOptions options = System.Serializable.lz4Options.WithResolver(
        CompositeResolver.Create(
            new IMessagePackFormatter[]
            {
                new GroupFormatter(),
                new ProjectFormatter(),
                new MessagePack.Formatters.InterfaceReadOnlyListFormatter<AccelerateProjectGroupDTO>(),
                new MessagePack.Formatters.ListFormatter<AccelerateProjectGroupDTO>(),
                new MessagePack.Formatters.ListFormatter<AccelerateProjectDTO>(),
            },
            new IFormatterResolver[] { BuiltinResolver.Instance }));

    internal static void Serialize(Stream stream, IReadOnlyList<AccelerateProjectGroupDTO> groups)
        => MessagePackSerializer.Serialize(stream, groups, options);

    internal static List<AccelerateProjectGroupDTO>? Deserialize(Stream stream)
        => MessagePackSerializer.Deserialize<List<AccelerateProjectGroupDTO>>(stream, options);

    // These array slots are the existing MPKey contracts (5 group fields, 14 project fields).
    // Unknown trailing slots are skipped so newer cache files remain readable.
    sealed class GroupFormatter : IMessagePackFormatter<AccelerateProjectGroupDTO>
    {
        public void Serialize(ref MessagePackWriter writer, AccelerateProjectGroupDTO value, MessagePackSerializerOptions options)
        {
            if (value is null) { writer.WriteNil(); return; }
            writer.WriteArrayHeader(5);
            writer.Write(value.Name);
            options.Resolver.GetFormatterWithVerify<List<AccelerateProjectDTO>>().Serialize(ref writer, value.Items!, options);
            writer.Write(value.IconUrl);
            writer.Write(value.Checked);
            writer.Write(value.Order);
        }

        public AccelerateProjectGroupDTO Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            if (reader.TryReadNil()) return null!;
            var count = reader.ReadArrayHeader();
            options.Security.DepthStep(ref reader);
            try
            {
                var value = new AccelerateProjectGroupDTO();
                for (var index = 0; index < count; index++)
                {
                    switch (index)
                    {
                        case 0: value.Name = reader.ReadString()!; break;
                        case 1: value.Items = options.Resolver.GetFormatterWithVerify<List<AccelerateProjectDTO>>().Deserialize(ref reader, options); break;
                        case 2: value.IconUrl = reader.ReadString(); break;
                        case 3: value.Checked = reader.ReadBoolean(); break;
                        case 4: value.Order = reader.ReadInt64(); break;
                        default: reader.Skip(); break;
                    }
                }
                return value;
            }
            finally { reader.Depth--; }
        }
    }

    sealed class ProjectFormatter : IMessagePackFormatter<AccelerateProjectDTO>
    {
        public void Serialize(ref MessagePackWriter writer, AccelerateProjectDTO value, MessagePackSerializerOptions options)
        {
            if (value is null) { writer.WriteNil(); return; }
            writer.WriteArrayHeader(14);
            writer.Write(value.Name);
            writer.Write(value.Port);
            writer.Write(value.MatchDomainNames);
            writer.Write(value.ForwardDomainNames);
            writer.Write(value.IgnoreSSLCertVerification);
            writer.Write(value.FakeServerName);
            writer.Write((int)value.ProxyType);
            writer.Write(value.ListenDomainNames);
            writer.Write(value.Checked);
            options.Resolver.GetFormatterWithVerify<Guid>().Serialize(ref writer, value.Id, options);
            writer.Write(value.Order);
            writer.Write(value.FakeUserAgent);
            options.Resolver.GetFormatterWithVerify<List<AccelerateProjectDTO>>().Serialize(ref writer, value.Items!, options);
            writer.Write(value.Version);
        }

        public AccelerateProjectDTO Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            if (reader.TryReadNil()) return null!;
            var count = reader.ReadArrayHeader();
            options.Security.DepthStep(ref reader);
            try
            {
                var value = new AccelerateProjectDTO();
                for (var index = 0; index < count; index++)
                {
                    switch (index)
                    {
                        case 0: value.Name = reader.ReadString()!; break;
                        case 1: value.Port = reader.ReadUInt16(); break;
                        case 2: value.MatchDomainNames = reader.ReadString()!; break;
                        case 3: value.ForwardDomainNames = reader.ReadString()!; break;
                        case 4: value.IgnoreSSLCertVerification = reader.ReadBoolean(); break;
                        case 5: value.FakeServerName = reader.ReadString(); break;
                        case 6: value.ProxyType = (BD.WTTS.Enums.ProxyType)reader.ReadInt32(); break;
                        case 7: value.ListenDomainNames = reader.ReadString()!; break;
                        case 8: value.Checked = reader.ReadBoolean(); break;
                        case 9: value.Id = options.Resolver.GetFormatterWithVerify<Guid>().Deserialize(ref reader, options); break;
                        case 10: value.Order = reader.ReadInt64(); break;
                        case 11: value.FakeUserAgent = reader.ReadString(); break;
                        case 12: value.Items = options.Resolver.GetFormatterWithVerify<List<AccelerateProjectDTO>>().Deserialize(ref reader, options); break;
                        case 13: value.Version = reader.ReadString()!; break;
                        default: reader.Skip(); break;
                    }
                }
                return value;
            }
            finally { reader.Depth--; }
        }
    }
}
