namespace BD.WTTS.Models;

/// <summary>
/// 反向代理 IPC 数据包
/// </summary>
[MemoryPackable]
[MemoryPackUnion(0, typeof(ReverseProxyPacket))]
[MemoryPackUnion(1, typeof(FlowStatistics))]
public partial interface IReverseProxyPacket
{
    /// <summary>
    /// 从流中读取一个 <see cref="IReverseProxyPacket"/>
    /// </summary>
    /// <param name="stream"></param>
    /// <returns></returns>
    static IReverseProxyPacket? ReadBytes(Stream stream)
    {
        Span<byte> inBuffer = stackalloc byte[sizeof(int)];
        stream.Read(inBuffer);
        var len = BitConverter.ToInt32(inBuffer);
        if (len <= 0) return default;
        inBuffer = stackalloc byte[len];
        stream.Read(inBuffer);
        var obj = MemoryPackSerializer.Deserialize<IReverseProxyPacket>(inBuffer);
        return obj;
    }

    /// <summary>
    /// 将一个 <see cref="IReverseProxyPacket"/> 写入流中
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="packet"></param>
    static void Write(Stream stream, IReverseProxyPacket packet)
    {
        byte[] outBuffer = MemoryPackSerializer.Serialize(packet);
        stream.Write(BitConverter.GetBytes(outBuffer.Length));
        stream.Write(outBuffer);
        stream.Flush();
    }
}

[MemoryPackable]
public sealed partial record ReverseProxyPacket(ReverseProxyCommand Command) : IReverseProxyPacket { }