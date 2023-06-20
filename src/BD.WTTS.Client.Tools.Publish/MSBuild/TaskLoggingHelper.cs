using Microsoft.Build.Framework;

namespace Microsoft.Build.Utilities;

public class TaskLoggingHelper
{
    private TaskLoggingHelper() { }

    public static TaskLoggingHelper Instance { get; } = new();

    const string TAG = "MSBuild";

    /// <summary>
    /// 使用指定的字符串记录警告。 线程安全。
    /// </summary>
    /// <param name="message"></param>
    /// <param name="messageArgs"></param>
    public void LogWarning(string message, params object[] messageArgs)
    {
        Log.Warn(TAG, message, messageArgs);
    }

    /// <summary>
    /// 使用指定的字符串记录具有给定重要性的消息。 线程安全。
    /// </summary>
    /// <param name="importance"></param>
    /// <param name="message"></param>
    /// <param name="messageArgs"></param>
    public void LogMessage(MessageImportance importance, string message, params object[] messageArgs)
    {
        switch (importance)
        {
            case MessageImportance.High:
                Log.Warn(TAG, message, messageArgs);
                break;
            case MessageImportance.Normal:
                Log.Info(TAG, message, messageArgs);
                break;
            case MessageImportance.Low:
                Log.Debug(TAG, message, messageArgs);
                break;
        }
    }
}
