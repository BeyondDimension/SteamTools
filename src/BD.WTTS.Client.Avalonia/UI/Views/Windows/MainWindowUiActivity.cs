using System;
using System.Threading;

namespace BD.WTTS.UI.Views.Windows;

internal static class MainWindowUiActivity
{
    static int activeCount;

    internal static bool IsActive => Volatile.Read(ref activeCount) != 0;

    internal static IDisposable Enter()
    {
        var scope = new Scope();
        Interlocked.Increment(ref activeCount);
        return scope;
    }

    sealed class Scope : IDisposable
    {
        int disposed;

        public void Dispose()
        {
            if (Interlocked.Exchange(ref disposed, 1) == 0)
                Interlocked.Decrement(ref activeCount);
        }
    }
}
