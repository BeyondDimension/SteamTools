#if !AREA_LIB
using System.Diagnostics;
using MPKey = MessagePack.KeyAttribute;
using MPObj = MessagePack.MessagePackObjectAttribute;
using N_JsonProperty = Newtonsoft.Json.JsonPropertyAttribute;
using S_JsonProperty = System.Text.Json.Serialization.JsonPropertyNameAttribute;
using System.Linq;
#if ORM_SQLITE_PCL
using SQLiteTable = SQLite.TableAttribute;
using SQLiteColumn = SQLite.ColumnAttribute;
using SQLiteMaxLength = SQLite.MaxLengthAttribute;
using SQLitePrimaryKey = SQLite.PrimaryKeyAttribute;
using SQLiteRequired = SQLite.NotNullAttribute;
using SQLiteIndexed = SQLite.IndexedAttribute;
#endif
#if ORM_EF_CORE
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
#endif

namespace System.Application.Entities;

/// <summary>
/// 地区表、模型
/// </summary>
[MPObj]
[DebuggerDisplay("{DebuggerDisplay(),nq}")]
#if ORM_SQLITE_PCL
[SQLiteTable("T99719D58")]
#endif
public sealed class Area : IArea
{
    string DebuggerDisplay() => $"{Name}, {Id}";

    /// <summary>
    /// 地区Id
    /// </summary>
#if ORM_SQLITE_PCL
    [SQLiteColumn("C27B42999")]
    [SQLitePrimaryKey] // SQLitePCL 主键
#endif
#if ORM_EF_CORE
    [Key] // EF 主键
    [DatabaseGenerated(DatabaseGeneratedOption.None)] // EF 不自增
#endif
    [MPKey(0)]
    [N_JsonProperty("0")]
    [S_JsonProperty("0")]
    public int Id { get; set; }

#if ORM_SQLITE_PCL
    [SQLiteColumn("CD57FE61B")]
    [SQLiteRequired] // SQLitePCL not null
    [SQLiteMaxLength(IArea.Length_AreaName)] // SQLitePCL MaxLength
#endif
#if ORM_EF_CORE
    [Required] // EF not null
    [MaxLength(IArea.Length_AreaName)] // EF MaxLength
#endif
    [MPKey(1)]
    [N_JsonProperty("1")]
    [S_JsonProperty("1")]
    public string? Name { get; set; }

#if ORM_SQLITE_PCL
    [SQLiteColumn("CF967FC52")]
    [SQLiteRequired] // SQLitePCL not null
    [SQLiteIndexed] // SQLitePCL 索引
#endif
#if ORM_EF_CORE
    [Required] // EF not null
#endif
    [MPKey(2)]
    [N_JsonProperty("2")]
    [S_JsonProperty("2")]
    public AreaLevel Level { get; set; }

#if ORM_SQLITE_PCL
    [SQLiteColumn("CDB19A0B9")]
    [SQLiteIndexed] // SQLitePCL 索引
#endif
#if ORM_EF_CORE
#endif
    [MPKey(3)]
    [N_JsonProperty("3")]
    [S_JsonProperty("3")]
    public int? Up { get; set; }

    /// <summary>
    /// 短名称
    /// </summary>
#if ORM_SQLITE_PCL
    [SQLiteColumn("C996F8471")]
#endif
    [MPKey(4)]
    [N_JsonProperty("4")]
    [S_JsonProperty("4")]
    public string? ShortName { get; set; }

#if ORM_EF_CORE

    /// <summary>
    /// 设置索引
    /// </summary>
    /// <param name="b"></param>
    public static void Indexed(ModelBuilder b) => b.Entity<Area>().HasIndex(x => new { x.Level, x.Up });

#endif

    public override string ToString() => IArea.ToString(this);
}
#endif