using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

// ReSharper disable once CheckNamespace
namespace System;

public static class ObjectExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T ThrowIsNull<T>([NotNull] this T? argument, [CallerArgumentExpression("argument")] string? paramName = null) where T : class
    {
        // https://github.com/dotnet/runtime/blob/v6.0.5/src/libraries/System.Private.CoreLib/src/System/ArgumentNullException.cs#L59
        // https://github.com/CommunityToolkit/dotnet/blob/v8.0.0-preview3/CommunityToolkit.Diagnostics/Guard.cs#L63
        if (argument is null)
        {
            Throw(paramName);
        }
        return argument;
    }

    [DoesNotReturn]
    static void Throw(string? paramName) => throw new ArgumentNullException(paramName);
}