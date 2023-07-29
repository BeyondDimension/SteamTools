// ReSharper disable once CheckNamespace
namespace BD.WTTS;

static class ReflectionHelper
{
    /// <summary>
    /// 是否为内部调用，防止动态反射调用
    /// </summary>
    /// <param name="thisAssembly"></param>
    /// <param name="stackTrace"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsInternalCall(Assembly thisAssembly, StackTrace stackTrace)
    {
        if (stackTrace.FrameCount >= 1)
        {
            for (int i = 1; i < stackTrace.FrameCount; i++)
            {
                try
                {
                    var method = stackTrace.GetFrame(i)?.GetMethod();
                    var assembly = method?.Module.Assembly;
                    if (assembly == thisAssembly)
                    {
                        return true;
                    }
                }
                catch
                {

                }
                if (i == 2)
                    break;
            }
        }
        return false;
    }

    /// <inheritdoc cref="IsInternalCall(Assembly, StackTrace)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsInternalCall<T>(StackTrace stackTrace)
        => IsInternalCall(typeof(T).Assembly, stackTrace);
}
