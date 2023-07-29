// ReSharper disable once CheckNamespace
namespace BD.Common.Enums;

// Length 对应的毫秒数值参考文章 https://www.cnblogs.com/kborid/p/14078744.html

public enum ToastLength
{
    /// <summary>
    /// Show the view or text notification for a short period of time.
    /// </summary>
    Short = 2000,

    /// <summary>
    /// Show the view or text notification for a long period of time.
    /// </summary>
    Long = 3500,
}