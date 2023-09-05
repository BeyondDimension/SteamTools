namespace BD.WTTS.Entities;

[SQLiteTable(TableName)]
[DebuggerDisplay("{DebuggerDisplay,nq}")]
public sealed class RequestCache : IEntity<string>
{
    public const string TableName = "BA35C8FA";
    public const string ColumnName_Id = "5151BA72";
    public const string ColumnName_UsageTime = "0786D029";

    string DebuggerDisplay => Id.ToString();

    [Column(ColumnName_Id)]
    [SQLiteNotNull]
    [PrimaryKey]
    public string Id { get; set; } = null!;

    [SQLiteNotNull]
    [Column("9A6A7973")]
    public string RequestUri { get; set; } = null!;

    [SQLiteNotNull]
    [Column("58943668")]
    public string RelativePath { get; set; } = null!;

    [SQLiteNotNull]
    [Column("A2B49CB8")]
    public string HttpMethod { get; set; } = null!;

    /// <summary>
    /// 使用时间，可根据此时间倒序删除不常用的数据
    /// </summary>
    [SQLiteNotNull]
    [Column(ColumnName_UsageTime)]
    public long UsageTime { get; set; } = DateTimeOffset.UtcNow.Ticks;
}
