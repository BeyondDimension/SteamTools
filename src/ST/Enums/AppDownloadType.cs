namespace System.Application
{
    /// <summary>
    /// APP下载类型
    /// </summary>
    public enum AppDownloadType : byte
    {
        /// <summary>
        /// 安装包
        /// </summary>
        Install,

        /// <summary>
        /// 压缩包(Tar/GZip)
        /// </summary>
        Compressed_GZip,

        /// <summary>
        /// 压缩包(7z)
        /// </summary>
        Compressed_7z,

        /// <summary>
        /// 压缩包(Tar/Brotli)
        /// </summary>
        Compressed_Br,

        /// <summary>
        /// 压缩包(Tar/XZ)
        /// </summary>
        Compressed_XZ,

        /// <summary>
        /// 压缩包(Tar/Zstd)
        /// </summary>
        Compressed_Zstd,
    }
}