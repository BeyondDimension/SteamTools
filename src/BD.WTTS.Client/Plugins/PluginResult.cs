namespace BD.WTTS.Plugins;

/// <summary>
/// 插件数据结果
/// </summary>
/// <typeparam name="TData">结果数据类型</typeparam>
/// <param name="IsDisable">是否禁用</param>
/// <param name="Data">插件数据</param>
public sealed class PluginResult<TData>
    : ReactiveObject, IEquatable<PluginResult<TData>>,
    IEquatable<TData>, IReactiveObject
    where TData : notnull
{
    [SetsRequiredMembers]
    public PluginResult(bool isDisable, TData data)
    {
        IsDisable = isDisable;
        Data = data;
    }

    [property: Reactive]
    public bool IsDisable { get; set; }

    public required TData Data { get; init; }

    public override int GetHashCode() => Data.GetHashCode();

    public bool Equals(PluginResult<TData>? other)
        => EqualityComparer<TData?>.Default.Equals(other == default ? default : other.Data, Data);

    public bool Equals(TData? other)
        => EqualityComparer<TData>.Default.Equals(other, Data);

    public override bool Equals(object? obj)
    {
        if (obj is TData obj1) return Equals(obj1);
        if (obj is PluginResult<TData> obj2) return Equals(obj2);
        return base.Equals(obj);
    }
}