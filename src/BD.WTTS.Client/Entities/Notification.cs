namespace BD.WTTS.Entities;

/// <summary>
/// 通知记录
/// </summary>
[SQLiteTable("A765AD32")]
[DebuggerDisplay("{DebuggerDisplay,nq}")]
public sealed class Notification : IEntity<Guid>
{
    string DebuggerDisplay => Id.ToString();

    [Column("2ABD43F2")]
    [PrimaryKey]
    public Guid Id { get; set; }

    [Column("4557AC21")]
    public DateTimeOffset ExpirationTime { get; set; } = DateTimeOffset.Now;

    [Column("8E7393C5")]
    public bool HasRead { get; set; }
}
