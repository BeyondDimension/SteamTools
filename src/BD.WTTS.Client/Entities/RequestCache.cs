namespace BD.WTTS.Entities;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public sealed class RequestCache : IEntity<string>
{
    string DebuggerDisplay => Id.ToString();

    public string Id { get; set; } = null!;

    public string RequestUri { get; set; } = null!;

    public string RelativePath { get; set; } = null!;

    public string HttpMethod { get; set; } = null!;

    /// <summary>
    /// 使用时间，可根据此时间倒序删除不常用的数据
    /// </summary>
    public long UsageTime { get; set; } = DateTimeOffset.UtcNow.Ticks;
}
