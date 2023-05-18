namespace BD.WTTS.Entities;

[SQLiteTable("BA35C8FA")]
[DebuggerDisplay("{DebuggerDisplay,nq}")]
public sealed class RequestCache : IEntity<string>
{
    string DebuggerDisplay => Id.ToString();

    [Column("5151BA72")]
    [SQLiteNotNull]
    [PrimaryKey]
    public string Id { get; set; } = null!;

    [SQLiteNotNull]
    [Column("9A6A7973")]
    public string RequestUri { get; set; } = null!;

    [SQLiteNotNull]
    [Column("58943668")]
    public byte[] Response { get; set; } = null!;

    [SQLiteNotNull]
    [Column("A2B49CB8")]
    public string HttpMethod { get; set; } = null!;
}
