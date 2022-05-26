using System.Application.Models;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

// ReSharper disable once CheckNamespace
namespace System;

public static class ExplicitHasValueExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HasValue([NotNullWhen(true)] this IExplicitHasValue? obj)
    {
        return obj != null && obj.ExplicitHasValue();
    }
}
