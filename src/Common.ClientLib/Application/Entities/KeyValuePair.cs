#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
using SQLite;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using NotNull = System.Diagnostics.CodeAnalysis.NotNullAttribute;
using SQLiteNotNull = SQLite.NotNullAttribute;
using SQLiteTable = SQLite.TableAttribute;

namespace System.Application.Entities
{
    [SQLiteTable("C2F5F5F5")]
    [DebuggerDisplay("{DebuggerDisplay(),nq}")]
    public sealed class KeyValuePair : IEntity<string>
    {
        [Column("B1E54167")]
        [PrimaryKey]
        [SQLiteNotNull]
        [NotNull, DisallowNull] // C# 8 not null
        public string? Id { get; set; }

        [Column("70E8B6F4")]
        [SQLiteNotNull]
        [NotNull, DisallowNull] // C# 8 not null
        public byte[]? Value { get; set; }

        string DebuggerDisplay() => $"{Id}, {Value}";
    }
}
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。