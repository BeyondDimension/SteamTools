// https://github.com/dotnetcore/FastGithub/blob/2.1.4/FastGithub.FlowAnalyze/FlowType.cs

// ReSharper disable once CheckNamespace
namespace System.Application;

/// <summary>
/// 流量类型
/// </summary>
public enum EFlowType : byte
{
    /// <summary>
    /// 读取
    /// </summary>
    Read,

    /// <summary>
    /// 写入
    /// </summary>
    Wirte
}
