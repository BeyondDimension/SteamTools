using System.IO.FileFormats;
using System.Application.Columns;

namespace System.Application.Entities
{
    public interface IImage : IEntity<Guid>, ISoftDeleted
    {
        ImageFormat Type { get; set; }

        /// <summary>
        /// 文件大小
        /// </summary>
        long Size { get; set; }

        /// <summary>
        /// 图片宽度
        /// </summary>
        int Width { get; set; }

        /// <summary>
        /// 图片高度
        /// </summary>
        int Height { get; set; }

        /// <summary>
        /// 哈希值（SHA384）
        /// </summary>
        string SHA384 { get; set; }

        /// <summary>
        /// 获取文件名
        /// </summary>
        /// <returns></returns>
        string GetFileName();
    }
}