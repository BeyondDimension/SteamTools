#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
using SQLite;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using NotNull = System.Diagnostics.CodeAnalysis.NotNullAttribute;
using SQLiteNotNull = SQLite.NotNullAttribute;
using SQLiteTable = SQLite.TableAttribute;

namespace System.Application.Entities
{
    /// <summary>
    /// 游戏账号平台令牌
    /// </summary>
    [SQLiteTable(TableName)]
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public sealed class GameAccountPlatformAuthenticator : IEntity<ushort>
    {
        public const string TableName = "E4401864";
        public const string ColumnName_ServerId = "C9835F84";

        string DebuggerDisplay => $"{Name}, {Id}";

        [Column("1DEF5924")]
        [PrimaryKey]
        [AutoIncrement]
        public ushort Id { get; set; }

        /// <summary>
        /// 显示名称
        /// </summary>
        [Column("D4117D89")]
        [SQLiteNotNull]
        [NotNull, DisallowNull] // C# 8 not null
        public byte[]? Name { get; set; }

        /// <summary>
        /// [EncryptionMode][[GamePlatform]Ciphertext]
        /// </summary>
        [Column("60FAF486")]
        [SQLiteNotNull]
        [NotNull, DisallowNull] // C# 8 not null
        public byte[]? Value { get; set; }

        /// <summary>
        /// 云同步Id
        /// </summary>
        [Column(ColumnName_ServerId)]
        public Guid? ServerId { get; set; }
    }
}
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。