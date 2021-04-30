using System.Application.Columns;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.IO.FileFormats;
using System.Security.Cryptography;

namespace System.Application.Entities
{
    /// <summary>
    /// 图片表
    /// </summary>
    [Table("Images")]
    [DebuggerDisplay("{DebuggerDisplay(),nq}")]
    public class Image<TUserKey, TUser, TUploadImageType> : IImage, INEWSEQUENTIALID
        where TUser : IUser<TUserKey>
        where TUserKey : IEquatable<TUserKey>
        where TUploadImageType : Enum
    {
        string DebuggerDisplay() => $"{Id}, {Type}, {SHA384}";

        [Key] // EF 主键
        public Guid Id { get; set; }

        public ImageFormat Type { get; set; }

        public long Size { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        [Required] // EF not null
        [MaxLength(Hashs.String.Lengths.SHA384 + 3)]
        [MinLength(Hashs.String.Lengths.SHA384)]
        public string SHA384 { get; set; } = string.Empty;

        public bool SoftDeleted { get; set; }

        /// <summary>
        /// 上传纪录
        /// </summary>
        public virtual List<ImageUploadRecord<TUserKey, TUser, TUploadImageType>>? Records { get; set; }

        public string GetFileName() => SHA384 + Type.GetExtension();
    }
}