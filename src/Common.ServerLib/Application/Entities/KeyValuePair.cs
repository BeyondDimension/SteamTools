using System.Application.Columns;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace System.Application.Entities
{
    /// <summary>
    /// 键值对表
    /// <para>https://stackoverflow.com/questions/514603/key-value-pairs-in-a-database-table</para>
    /// </summary>
    [Table("KeyValuePairs")]
    [DebuggerDisplay("{DebuggerDisplay(),nq}")]
    public sealed class KeyValuePair : IEntity<string>, ISoftDeleted
    {
        [Key] // EF 主键
        [NotNull, DisallowNull] // C# 8 not null
        public string? Id { get; set; }

        [Required] // EF not null
        [NotNull, DisallowNull] // C# 8 not null
        public string? Value { get; set; }

        public bool SoftDeleted { get; set; }

        string DebuggerDisplay() => $"{Id}, {Value}";
    }
}