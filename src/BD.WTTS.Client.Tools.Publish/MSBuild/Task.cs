namespace Microsoft.Build.Utilities;

/// <summary>
/// 此帮助器基类提供用于任务的默认功能。 此类只能以派生形式实例化。
/// </summary>
public abstract class Task
{
    /// <summary>
    /// 获取包含任务记录方法的 TaskLoggingHelper 类的实例。 taskLoggingHelper 是一个 MarshallByRef 对象，如果父任务正在创建 appdomain 并将该对象封送到其中，则该对象需要调用 MarkAsInactive。 如果在任务执行结束时未卸载 appdomain，并且未调用 MarkAsInactive 方法，则将导致在其中创建任务的 appdomain 中的任务实例泄漏。
    /// </summary>
    public TaskLoggingHelper Log => TaskLoggingHelper.Instance;

    /// <summary>
    /// 必须由派生类实现。
    /// </summary>
    /// <returns></returns>
    public abstract bool Execute();
}
