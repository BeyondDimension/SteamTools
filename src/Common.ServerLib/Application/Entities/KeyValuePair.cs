using System.Application.Columns;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;

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
        public string Id { get; set; } = string.Empty;

        [Required] // EF not null
        public string Value { get; set; } = string.Empty;

        public bool SoftDeleted { get; set; }

        string DebuggerDisplay() => $"{Id}, {Value}";
    }
}