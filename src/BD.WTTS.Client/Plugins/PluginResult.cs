namespace BD.WTTS.Plugins;

/// <summary>
/// 插件数据结果
/// </summary>
/// <typeparam name="TData">结果数据类型</typeparam>
/// <param name="IsDisable">是否禁用</param>
/// <param name="Data">插件数据</param>
public record PluginResult<TData>
    : ReactiveRecord, IEquatable<PluginResult<TData>>,
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

    required public TData Data { get; init; }

    public override int GetHashCode() => Data.GetHashCode();

    bool IEquatable<PluginResult<TData>>.Equals(PluginResult<TData>? other)
        => EqualityComparer<TData>.Default.Equals(other.Data, Data);

    public bool Equals(TData? other)
        => EqualityComparer<TData>.Default.Equals(other, Data);
}