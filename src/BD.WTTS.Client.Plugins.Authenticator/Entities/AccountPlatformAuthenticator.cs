// ReSharper disable once CheckNamespace
namespace BD.WTTS.Entities;

/// <summary>
/// 账号平台令牌数据
/// </summary>
[SQLiteTable(TableName)]
[DebuggerDisplay("{DebuggerDisplay,nq}")]
[MPObj(keyAsPropertyName: true), MP2Obj]
public sealed partial class AccountPlatformAuthenticator : IEntity<ushort>, IOrder, IOrderAuthenticator
{
    public const string TableName = "E4401864";
    public const string ColumnName_ServerId = "C9835F84";
    public const string ColumnName_Id = "1DEF5924";
    public const string ColumnName_Index = "41B24805";

    string DebuggerDisplay => $"{Name}, {Id}";

    [Column(ColumnName_Id)]
    [PrimaryKey]
    [AutoIncrement]
    [MPIgnore, MP2Ignore]
    public ushort Id { get; set; }

    [Column("41B24805")]
    [SQLiteNotNull]
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
    public DateTimeOffset Created { get; set; }

    [Column("38A7E919")]
    [SQLiteNotNull]
    public DateTimeOffset LastUpdate { get; set; }

    /// <summary>
    /// 服务端Id
    /// </summary>
    [Column(ColumnName_ServerId)]
    [MPIgnore]
    public Guid? ServerId { get; set; }

    long IOrder.Order
    {
        get => Index;
        set => Index = (int)value;
    }
}