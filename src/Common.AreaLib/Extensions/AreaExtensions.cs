using System.Application.Entities;
using System.Application.Services;

// ReSharper disable once CheckNamespace
namespace System;

public static class AreaExtensions
{
    /// <inheritdoc cref="IAreaResourceHelper{TArea}.GetSecondaryOrThis(TArea)"/>
    public static TArea GetSecondaryOrThis<TArea>(this TArea area) where TArea : class, IArea
    {
        var s = DI.Get<IAreaResourceHelper<TArea>>();
        return s.GetSecondaryOrThis(area);
    }

    /// <inheritdoc cref="IAreaResourceHelper{TArea}.GetFullName(TArea)"/>
    public static string GetFullName<TArea>(this TArea area) where TArea : class, IArea
    {
        var s = DI.Get<IAreaResourceHelper<TArea>>();
        return s.GetFullName(area);
    }
}