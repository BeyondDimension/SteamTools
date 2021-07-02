#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
using SQLite;
using System.Application.Columns;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using MPIgnore = MessagePack.IgnoreMemberAttribute;
using MPObject = MessagePack.MessagePackObjectAttribute;
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
    [MPObject(keyAsPropertyName: true)]
    public sealed class GameAccountPlatformAuthenticator : IEntity<ushort>, IOrder, IOrderGAPAuthenticator
    {
        public const string TableName = "E4401864";
        public const string ColumnName_ServerId = "C9835F84";
        public const string ColumnName_Id = "1DEF5924";
        public const string ColumnName_Index = "41B24805";

        string DebuggerDisplay => $"{Name}, {Id}";

        [Column(ColumnName_Id)]
        [PrimaryKey]
        [AutoIncrement]
        [MPIgnore]
        public ushort Id { get; set; }

        [Column("41B24805")]
        [SQLiteNotNull]
        [NotNull, DisallowNull] // C# 8 not null
        public int Index { get; set; }

        /// <summary>
        /// 显示名称
        /// </summary>
        [Column("D4117D89")]
        public byte[]? Name { get; set; }

        /// <summary>
        /// [EncryptionMode][[GamePlatform]Ciphertext]
        /// </summary>
        [Column("60FAF486")]
        [SQLiteNotNull]
        [NotNull, DisallowNull] // C# 8 not null
        public byte[]? Value { get; set; }

        /// <summary>
        /// 是否未启用本地加密
        /// </summary>
        [Column("44FF3988")]
        public bool IsNotLocal { get; set; }

        /// <summary>
        /// 是否需要二级密码
        /// </summary>
        [Column("4AF8A895")]
        public bool IsNeedSecondaryPassword { get; set; }

        [Column("7D808E24")]
        [SQLiteNotNull]
        [NotNull, DisallowNull] // C# 8 not null
        public DateTimeOffset Created { get; set; }

        [Column("38A7E919")]
        [SQLiteNotNull]
        [NotNull, DisallowNull] // C# 8 not null
        public DateTimeOffset LastUpdate { get; set; }

        /// <summary>
        /// 云同步Id
        /// </summary>
        [Column(ColumnName_ServerId)]
        [MPIgnore]
        public Guid? ServerId { get; set; }

        int IOrder.Order
        {
            get => Index;
            set => Index = value;
        }
    }
}
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。