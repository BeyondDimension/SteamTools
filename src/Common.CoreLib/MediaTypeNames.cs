namespace System
{
    /// <summary>
    /// MIME 类型
    /// <para>https://developer.mozilla.org/zh-CN/docs/Web/HTTP/Basics_of_HTTP/MIME_Types</para>
    /// </summary>
    public static class MediaTypeNames
    {
        public const string JSON = "application/json";

        public const string MessagePack = "application/x-msgpack";

        public const string BMP = "image/bmp";

        public const string GIF = "image/gif";

        public const string JPG = "image/jpeg";

        public const string PNG = "image/png";

        public const string ICO = "image/x-icon";

        public const string WEBP = "image/webp";

        public const string HEIF = "image/heif";

        public const string HEIFSequence = "image/heif-sequence";

        public const string HEIC = "image/heic";

        public const string HEICSequence = "image/heic-sequence";

        public const string AMR = "audio/amr";

        public const string WAV = "audio/wav";

        public const string CAF = "audio/x-caf";

        public const string MP3 = "audio/mpeg";

        /// <summary>
        /// 二进制流，不知道下载文件类型
        /// </summary>
        public const string Binary = "application/octet-stream";

        public const string TXT = "text/plain";

        public const string HTML = "text/html";

        public const string IMAGE = "image/*";

        public const string APK = "application/vnd.android.package-archive";

        public const string XML = "text/xml";

        public const string XML_APP = "application/xml";

        public const string Security = "application/vnd.sapi+x-msgpack";

        public const string WEBM = "video/webm";

        public const string MP4 = "video/mp4";

        public const string APNG = "image/apng";

        public const string APPX = "application/appx";

        public const string MSIX = "application/msix";

        public const string APPX_Bundle = "application/appxbundle";

        public const string MSIX_Bundle = "application/msixbundle";

        public const string AppInstaller = "application/appinstaller";
    }
}