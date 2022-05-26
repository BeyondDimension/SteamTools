using SQLite;
using System.Diagnostics;
using SQLiteNotNull = SQLite.NotNullAttribute;
using SQLiteTable = SQLite.TableAttribute;

namespace System.Application.Entities;

[SQLiteTable("C2F5F5F5")]
[DebuggerDisplay("{DebuggerDisplay(),nq}")]
public sealed class KeyValuePair : IEntity<string>
{
    [Column("B1E54167")]
    [PrimaryKey]
    [SQLiteNotNull]
    public string Id { get; set; } = string.Empty;

    [Column("70E8B6F4")]
    [SQLiteNotNull]
    public byte[] Value { get; set; } = Array.Empty<byte>();

    string DebuggerDisplay() => $"{Id}, {Value}";
}