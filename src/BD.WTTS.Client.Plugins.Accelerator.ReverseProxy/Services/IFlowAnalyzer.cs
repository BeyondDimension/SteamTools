// https://github.com/dotnetcore/FastGithub/blob/2.1.4/FastGithub.FlowAnalyze/FlowAnalyzer.cs

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services;

/// <summary>
/// 流量分析器
/// </summary>
public interface IFlowAnalyzer
{
    /// <summary>
    /// 收到数据
    /// </summary>
    /// <param name="flowType"></param>
    /// <param name="length"></param>
    void OnFlow(FlowType flowType, int length);

    /// <summary>
    /// 获取速率
    /// </summary>
    /// <returns></returns>
    FlowStatistics GetFlowStatistics();
}