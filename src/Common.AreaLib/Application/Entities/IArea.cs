namespace System.Application.Entities;

public partial interface IArea : IEntity<int>
{
    public const int Length_AreaName = 90;

    /// <summary>
    /// 地区名
    /// </summary>
    string? Name { get; set; }

    /// <summary>
    /// 地区等级
    /// </summary>
    AreaLevel Level { get; set; }

    /// <summary>
    /// 上一级地区Id
    /// </summary>
    int? Up { get; set; }

    /// <summary>
    /// 短名称
    /// </summary>
    string? ShortName { get; set; }

    protected static string ToString(IArea area)
    {
        if (!string.IsNullOrWhiteSpace(area.ShortName)) return area.ShortName;
        if (!string.IsNullOrWhiteSpace(area.Name)) return area.Name;
        return area.Id.ToString();
    }
}