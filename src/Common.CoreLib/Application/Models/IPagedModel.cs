namespace System.Application.Models;

/// <summary>
/// 分页模型
/// </summary>
public interface IPagedModel : IExplicitHasValue
{
    /// <summary>
    /// 当前页码，页码从1开始，不能取负数，如果&lt;=0，则必定是空集合
    /// </summary>
    int Current { get; set; }

    /// <summary>
    /// 页大小，即每页条数，不能取负数，如果&lt;=0，则必定是空集合
    /// </summary>
    int PageSize { get; set; }

    /// <summary>
    /// 默认的当前页码
    /// </summary>
    public const int DefaultCurrent = 1;

    /// <summary>
    /// 默认的页大小
    /// </summary>
    public const int DefaultPageSize = 10;

    /// <summary>
    /// 总数据数
    /// </summary>
    int Total { get; set; }

    /// <summary>
    /// 总页数
    /// </summary>
    public int PageCount
    {
        get
        {
            float total = Total;
            float pageSize = PageSize;
            var pageCount = (int)MathF.Ceiling(total / pageSize);
            return pageCount < 0 ? 0 : pageCount;
        }
    }
}

/// <inheritdoc cref="IPagedModel"/>
public interface IPagedModel<T> : IPagedModel
{
    /// <inheritdoc cref="IPagedModel.DataSource"/>
    IList<T> DataSource { get; set; }
}

/// <inheritdoc cref="IPagedModel"/>
public interface IReadOnlyPagedModel<out T> : IPagedModel
{
    /// <inheritdoc cref="IPagedModel.DataSource"/>
    IReadOnlyList<T> DataSource { get; }
}