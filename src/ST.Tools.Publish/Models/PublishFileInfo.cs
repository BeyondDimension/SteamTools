using System.IO;

namespace System.Application.Models
{
    /// <summary>
    /// 发布文件信息
    /// </summary>
    public class PublishFileInfo
    {
        public PublishFileInfo()
        {
        }

        const string CEF = "CEF";
        const string Bin = "Bin";

        public PublishFileInfo(string path, string relativeTo)
        {
            Path = path;
            FileEx = IO.Path.GetExtension(path);
            Length = new FileInfo(path).Length;
            RelativePath = IO.Path.GetRelativePath(relativeTo, path);
            if (RelativePath.StartsWith(CEF, StringComparison.OrdinalIgnoreCase))
            {
                RelativePath = Bin + IO.Path.DirectorySeparatorChar + CEF + RelativePath.Substring(CEF.Length, RelativePath.Length - CEF.Length);
            }
        }

        /// <summary>
        /// 文件路径
        /// </summary>
        public string Path { get; set; } = string.Empty;

        /// <summary>
        /// 文件相对路径
        /// </summary>
        public string RelativePath { get; set; } = string.Empty;

        /// <summary>
        /// 文件原哈希值
        /// </summary>
        public string SHA256 { get; set; } = string.Empty;

        /// <summary>
        /// 文件扩展名
        /// </summary>
        public string FileEx { get; set; } = string.Empty;

        /// <summary>
        /// 数字签名后的哈希值
        /// </summary>
        public string DigitalSignSHA256 { get; set; } = string.Empty;

        /// <summary>
        /// 文件大小
        /// </summary>
        public long Length { get; set; }
    }
}