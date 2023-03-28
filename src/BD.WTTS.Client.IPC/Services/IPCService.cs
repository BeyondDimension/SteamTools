namespace BD.WTTS.Services;

public interface IPCService<TPacket> where TPacket : class
{
    /// <summary>
    /// 启动 IPC 服务
    /// </summary>
    /// <param name="pipeName"></param>
    /// <returns></returns>
    ValueTask<int> RunAsync(string pipeName);

    /// <summary>
    /// 发送数据包
    /// </summary>
    /// <param name="packet"></param>
    void Send(TPacket packet);
}