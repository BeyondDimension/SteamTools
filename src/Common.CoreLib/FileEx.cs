namespace System
{
    /// <summary>
    /// 文件扩展名(FileExtensions)
    /// </summary>
    public static class FileEx
    {
        public const string JSON = ".json";

        public const string BMP = ".bmp";

        public const string GIF = ".gif";

        public const string TXT = ".txt";

        public const string JPG = ".jpg";

        public const string JPEG = ".jpeg";

        public const string PNG = ".png";

        public const string ICO = ".ico";

        public const string WEBP = ".webp";

        public const string HTML = ".html";

        public const string CSS = ".css";

        public const string JS = ".js";

        public const string SVG = ".svg";

        public const string HEIF = ".heif";

        public const string HEIC = ".heic";

        public const string AMR = ".amr";

        public const string WAV = ".wav";

        public const string CAF = ".caf";

        public const string MP3 = ".mp3";

        public const string APK = ".apk";

        public const string ImageSource = ".imgsrc";

        public const string LOG = ".log";

        public const string TAR_GZ = ".tgz";

        public const string TAR_BR = ".tbr";

        public const string BIN = ".bin";

        public const string APNG = ".apng";

        public const string EXE = ".exe";

        public const string DMG = ".dmg";

        public const string _7Z = ".7z";

        public const string APPX = ".appx";

        public const string MSIX = ".msix";

        public const string APPX_Bundle = ".appxbundle";

        public const string MSIX_Bundle = ".msixbundle";

        public const string AppInstaller = ".appinstaller";

        public const string MPO = ".mpo";

        public const string DownloadCache = ".download_cache";

        public const string RPM = ".rpm";

        public const string CPIO = ".cpio";

        public const string DEB = ".deb";

        public const string DEB_TAR = ".deb.tar";

        public const string DEB_TAR_XZ = ".deb.tar.xz";

        public const string PKG = ".pkg";

        public const string MSI = ".msi";

        public const string TAR_XZ = ".tar.xz";

        public const string TAR_ZST = ".tar.zst";

        public const string CMD = ".cmd";

        public static string Clean(string extension, bool trimLeadingPeriod = false)
        {
            if (string.IsNullOrWhiteSpace(extension))
                return string.Empty;

            extension = extension.TrimStart('*');
            extension = extension.TrimStart('.');

            if (!trimLeadingPeriod)
                extension = "." + extension;

            return extension;
        }
    }
}