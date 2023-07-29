// https://github.com/dotnetcore/FastGithub/blob/2.1.4/FastGithub.FlowAnalyze/FlowStatistics.cs

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Models;

/// <summary>
/// 流量统计
/// </summary>
[MP2Obj(SerializeLayout.Explicit)]
public partial record FlowStatistics
{
    /// <summary>
    /// 获取总读上行
    /// </summary>
    [MP2Key(0)]
    public long TotalRead { get; init; }

    /// <summary>
    /// 获取总下行
    /// </summary>
    [MP2Key(1)]
    public long TotalWrite { get; init; }

    /// <summary>
    /// 获取读取速率
    /// </summary>
    [MP2Key(2)]
    public double ReadRate { get; init; }

    /// <summary>
    /// 获取写入速率
    /// </summary>
    [MP2Key(3)]
    public double WriteRate { get; init; }
}