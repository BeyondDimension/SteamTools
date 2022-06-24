// https://github.com/dotnetcore/FastGithub/blob/2.1.4/FastGithub.FlowAnalyze/FlowStatistics.cs

namespace System.Application.Models;

/// <summary>
/// 流量统计
/// </summary>
public record FlowStatistics
{
    /// <summary>
    /// 获取总读上行
    /// </summary>
    public long TotalRead { get; init; }

    /// <summary>
    /// 获取总下行
    /// </summary>
    public long TotalWrite { get; init; }

    /// <summary>
    /// 获取读取速率
    /// </summary>
    public double ReadRate { get; init; }

    /// <summary>
    /// 获取写入速率
    /// </summary>
    public double WriteRate { get; init; }

#if DEBUG
    [Obsolete("use IOPath.GetSizeString", true)]
    public static string ToNetworkSizeString(long value)
    {
        if (value < 1024)
        {
            return $"{value} B";
        }
        if (value < 1024 * 1024)
        {
            return $"{value / 1024d:0.00} KB";
        }
        return $"{value / 1024d / 1024d:0.00} MB";
    }
#endif
}
