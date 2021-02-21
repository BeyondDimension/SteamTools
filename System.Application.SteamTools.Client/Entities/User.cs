using SQLite;
using System.Diagnostics;

namespace System.Application.Entities
{
    [Table("D5428AED")]
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public sealed class User : IEntity<Guid>
    {
        string DebuggerDisplay => Id.ToString();

        [Column("5E72F0AE")]
        [PrimaryKey]
        public Guid Id { get; set; }

        [Column("A931B798")]
        public byte[]? NickName { get; set; }

        [Column("4C12F9EE")]
        public Guid? Avatar { get; set; }
    }
}