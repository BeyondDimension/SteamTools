#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
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
    /// 脚本
    /// </summary>
    [SQLiteTable(TableName)]
    [DebuggerDisplay("{DebuggerDisplay(),nq}")]
    public class Script : IEntity<int>, IOrder
    {
        public const string TableName = "1B2D820C";
        public const string ColumnName_Id = "E386BC36";
        public const string ColumnName_Enable = "6DB11594";
        string DebuggerDisplay() => $"{Name}, {Id}";

        [Column(ColumnName_Id)]
        [PrimaryKey]
        [AutoIncrement]
        public int Id { get; set; }

        [Column("8061969F")]
        public int Order { get; set; } = 10;
        /// <summary>
        /// 显示名称
        /// </summary>
        [Column("7C85E5C4")]
        [SQLiteNotNull]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 脚本作者
        /// </summary>
        [Column("23BE056E")]
        public string Author { get; set; } = string.Empty;

        /// <summary>
        /// 版本号
        /// </summary>
        [Column("8037C4C3")]
        [SQLiteNotNull]
        public string Version { get; set; } = string.Empty;

        /// <summary>
        /// 服务器关联Guid
        /// </summary>
        [Column("F1DDBF5B")]
        public Guid? Pid { get; set; }


        /// <summary>
        /// 文件地址
        /// </summary>
        [Column("C9719845")]
        [SQLiteNotNull]
        public string FilePath { get; set; } = string.Empty;

        /// <summary>
        /// 缓存文件地址
        /// </summary>
        [Column("997C6B59")]
        [SQLiteNotNull]
        public string CachePath { get; set; } = string.Empty;

        /// <summary>
        /// 来源地址(脚本主页)
        /// </summary>
        [Column("233E579D")]
        public string SourceLink { get; set; } = string.Empty;

        /// <summary>
        /// 下载地址(当前版本下载地址)
        /// </summary>
        [Column("50EBB673")]
        public string DownloadLink { get; set; } = string.Empty;

        /// <summary>
        /// 更新地址(直链)
        /// </summary>
        [Column("A5AD2FBF")]
        public string UpdateLink { get; set; } = string.Empty;

        /// <summary>
        /// 说明
        /// </summary>
        [Column("B49B2587")]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 脚本匹配域名，分号分割多个
        /// </summary>
        [Column("29296F97")]
        public string MatchDomainNames { get; set; } = string.Empty;

        /// <summary>
        /// 是否启用
        /// </summary>
        [Column(ColumnName_Enable)]
        public bool Enable { get; set; } = false;

        /// <summary>
        /// 脚本图标
        /// </summary>
        [Column("4732B219")]
        public string? Icon { get; set; }

        ///// <summary>
        ///// 是否依赖油猴函数
        ///// </summary>
        //[Column("A5B0A899")]
        //public bool DependentGreasyForkFunction { get; set; }

        /// <summary>
        /// 脚本排除域名，分号分割多个
        /// </summary>
        [Column("569086A0")]
        public string? ExcludeDomainNames { get; set; }

        /// <summary>
        /// 依赖外部JS，分号分割多个
        /// </summary>
        [Column("5C02E4AA")]
        public string? RequiredJs { get; set; }


        /// <summary>
        /// 哈希值（MD5）
        /// </summary> 
        [Column("D965B4F5")]
        public string MD5 { get; set; }

        /// <summary>
        /// 哈希值（SHA512）
        /// </summary> 
        [Column("C5645EF3")]
        public string SHA512 { get; set; }


        [Column("58FF2FF5")]
        public DateTime CreationTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 哈希值（MD5）
        /// </summary> 
        [Column("8B9C94E5")]
        public bool IsBuild { get; set; } = true;



    }
}
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。