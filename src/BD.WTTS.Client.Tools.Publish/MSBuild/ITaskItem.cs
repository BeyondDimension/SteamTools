namespace Microsoft.Build.Framework;

public interface ITaskItem
{
    /// <summary>
    /// 获取或设置项“规格”，例如对于基于磁盘的项，这将是文件路径。
    /// </summary>
    string ItemSpec { get; set; }

    /// <summary>
    /// 获取该项上所有元数据的名称。 包含内置元数据，如“FullPath”。
    /// </summary>
    ICollection<string> MetadataNames { get; }

    /// <summary>
    /// 允许查询此项的元数据的值。
    /// </summary>
    /// <param name="metadataName"></param>
    /// <returns></returns>
    string GetMetadata(string metadataName);
}
