using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

// ReSharper disable once CheckNamespace
namespace System;

public static class ObjectExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T ThrowIsNull<T>([NotNull] this T? argument, [CallerArgumentExpression("argument")] string? paramName = null) where T : class
    {
        if (argument is null)
        {
            Throw(paramName);
        }
        return argument;
    }

    [DoesNotReturn]
    static void Throw(string? paramName) => throw new ArgumentNullException(paramName);
}