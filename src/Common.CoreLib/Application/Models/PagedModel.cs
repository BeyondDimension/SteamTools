using System.Diagnostics;
using MPKey = MessagePack.KeyAttribute;
using MPObj = MessagePack.MessagePackObjectAttribute;
using N_JsonProperty = Newtonsoft.Json.JsonPropertyAttribute;
using S_JsonProperty = System.Text.Json.Serialization.JsonPropertyNameAttribute;

namespace System.Application.Models;

/// <inheritdoc cref="IPagedModel"/>
[MPObj]
[Serializable]
[DebuggerDisplay("{DebuggerDisplay(),nq}")]
public sealed class PagedModel<T> : IPagedModel<T>, IReadOnlyPagedModel<T>
{
    string DebuggerDisplay() => $"Current: {Current}, Total: {Total}, Count: {mDataSource?.Count ?? 0}, PageSize: {PageSize}";

    List<T>? mDataSource;

    [MPKey(0)]
    [N_JsonProperty("0")]
    [S_JsonProperty("0")]
    public List<T> DataSource
    {
        get
        {
            if (mDataSource == null) mDataSource = new List<T>();
            return mDataSource;
        }

        set => mDataSource = value;
    }

    [MPKey(1)]
    [N_JsonProperty("1")]
    [S_JsonProperty("1")]
    public int Current { get; set; } = IPagedModel.DefaultCurrent;

    [MPKey(2)]
    [N_JsonProperty("2")]
    [S_JsonProperty("2")]
    public int PageSize { get; set; } = IPagedModel.DefaultPageSize;

    [MPKey(3)]
    [N_JsonProperty("3")]
    [S_JsonProperty("3")]
    public int Total { get; set; }

    bool IExplicitHasValue.ExplicitHasValue()
    {
        return Total >= 0 && PageSize > 0 && Current > 0;
        //&&
        //Current <= ((IPagedModel)this).PageCount &&
        //mDataSource.Any_Nullable();
    }

    IReadOnlyList<T> IReadOnlyPagedModel<T>.DataSource => DataSource;

    IList<T> IPagedModel<T>.DataSource
    {
        get => DataSource;
        set => DataSource = value is List<T> list ? list : value.ToList();
    }
}