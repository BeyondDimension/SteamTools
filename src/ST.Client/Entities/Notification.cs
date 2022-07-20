using SQLite;
using System.Application.Columns;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using NotNull = System.Diagnostics.CodeAnalysis.NotNullAttribute;
using SQLiteNotNull = SQLite.NotNullAttribute;
using SQLiteTable = SQLite.TableAttribute;

namespace System.Application.Entities
{
    /// <summary>
    /// 通知记录
    /// </summary>
    [SQLiteTable("A765AD32")]
    [DebuggerDisplay("{DebuggerDisplay(),nq}")]
    public class Notification : IEntity<Guid>
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
}
