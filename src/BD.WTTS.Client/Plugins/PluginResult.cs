namespace BD.WTTS.Plugins;

/// <summary>
/// 插件数据结果
/// </summary>
/// <typeparam name="TData">结果数据类型</typeparam>
/// <param name="IsDisable">是否禁用</param>
/// <param name="Data">插件数据</param>
public readonly record struct PluginResult<TData>(bool IsDisable, TData Data)
    : IEquatable<PluginResult<TData>>,
    IEquatable<TData>
    where TData : notnull
{
    public override readonly int GetHashCode() => Data.GetHashCode();

    public readonly bool Equals(PluginResult<TData> other)
        => EqualityComparer<TData>.Default.Equals(other.Data, Data);

    public readonly bool Equals(TData? other)
        => EqualityComparer<TData>.Default.Equals(other, Data);
}