namespace BD.WTTS.Models;

/// <summary>
/// 反向代理 IPC 数据包
/// </summary>
[MemoryPackable]
[MemoryPackUnion(0, typeof(ReverseProxyPacket))]
[MemoryPackUnion(1, typeof(FlowStatistics))]
public partial interface IReverseProxyPacket
{

}

[MemoryPackable]
public sealed partial record ReverseProxyPacket(ReverseProxyCommand Command, string? Data) : IReverseProxyPacket { }