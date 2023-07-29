namespace Microsoft.Build.Framework;

/// <summary>
/// 此枚举针对消息提供 3 个重要性级别。
/// </summary>
[Serializable]
public enum MessageImportance
{
    /// <summary>
    /// 重要性高，出现在详细程度较低的日志中
    /// </summary>
    High,

    /// <summary>
    /// 重要性一般
    /// </summary>
    Normal,

    /// <summary>
    /// 重要性低，出现在详细程度较高的日志中
    /// </summary>
    Low,
}
