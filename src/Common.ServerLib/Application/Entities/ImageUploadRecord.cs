using System.Application.Columns;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace System.Application.Entities
{
    /// <summary>
    /// 图片上传纪录表
    /// </summary>
    [Table("ImageUploadRecords")]
    [DebuggerDisplay("{DebuggerDisplay(),nq}")]
    public class ImageUploadRecord<TUserKey, TUser, TUploadImageType> : INEWSEQUENTIALID, ICreationTime
        where TUser : IUser<TUserKey>
        where TUserKey : IEquatable<TUserKey>
        where TUploadImageType : Enum
    {
        string DebuggerDisplay() => $"{ImagesId}, {Type}, {User?.NickName ?? (UserId?.ToString())}";

        [Key] // EF 主键
        public Guid Id { get; set; }

        /// <summary>
        /// 上传的用户ID
        /// </summary>
        [Required] // EF not null
        [NotNull, DisallowNull] // C# 8 not null
        public TUserKey? UserId { get; set; }

        /// <summary>
        /// 上传的用户
        /// </summary>
        public virtual TUser? User { get; set; }

        /// <summary>
        /// 图片ID
        /// </summary>
        public Guid ImagesId { get; set; }

        public DateTimeOffset CreationTime { get; set; }

        /// <summary>
        /// 上传图片类型（用途）
        /// </summary>
        [Required] // EF not null
        [NotNull, DisallowNull] // C# 8 not null
        public TUploadImageType? Type { get; set; }
    }
}