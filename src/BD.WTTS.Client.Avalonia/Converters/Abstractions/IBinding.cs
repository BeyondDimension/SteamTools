global using BD.WTTS.Converters.Abstractions;
#pragma warning disable IDE0079 // 请删除不必要的忽略
#pragma warning disable SA1211 // Using alias directives should be ordered alphabetically by alias name
using CommonCA = BD.Common.Converters.Abstractions;
#pragma warning restore SA1211 // Using alias directives should be ordered alphabetically by alias name
#pragma warning restore IDE0079 // 请删除不必要的忽略

namespace BD.WTTS.Converters.Abstractions;

public interface IBinding : CommonCA.IBinding
{
    object CommonCA.IBinding.DoNothing => this.DoNothing();

    object CommonCA.IBinding.UnsetValue => this.UnsetValue();
}

public static class BindingExtensions
{
    /// <inheritdoc cref="CommonCA.IBinding.DoNothing"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static object DoNothing(this CommonCA.IBinding _)
#if AVALONIA
        => BindingOperations.DoNothing;
#endif

    /// <inheritdoc cref="CommonCA.IBinding.UnsetValue"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static object UnsetValue(this CommonCA.IBinding _)
#if AVALONIA
        => BindingValueType.UnsetValue;
#endif
}