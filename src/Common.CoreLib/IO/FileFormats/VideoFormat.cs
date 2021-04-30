using System.Collections.Generic;
using System.IO.FileFormats;
using System.Linq;

namespace System.IO.FileFormats
{
    public enum VideoFormat : byte
    {
        MP4 = 1,

        WebM,
    }
}

namespace System
{
    /// <summary>
    /// Enum 扩展 <see cref="VideoFormat"/>
    /// </summary>
    public static partial class VideoFormatEnumExtensions
    {
        static readonly Lazy<IReadOnlyDictionary<VideoFormat, string>> _enum_mime = new(() =>
        {
            var value = new Dictionary<VideoFormat, string>()
            {
                { VideoFormat.WebM, MediaTypeNames.WEBM },
                { VideoFormat.MP4, MediaTypeNames.MP4 },
            };
            return value;
        });
        static readonly Lazy<IReadOnlyDictionary<string, VideoFormat>> _mime_enum = new(() =>
        {
            var value = enum_mime.ToDictionary(x => x.Value, x => x.Key, StringComparer.OrdinalIgnoreCase);
            return value;
        });

        static IReadOnlyDictionary<VideoFormat, string> enum_mime => _enum_mime.Value;
        static IReadOnlyDictionary<string, VideoFormat> mime_enum => _mime_enum.Value;

        /// <summary>
        /// 获取MIME
        /// </summary>
        /// <param name="imageFormat"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static string GetMIME(this VideoFormat videoFormat)
        {
            if (enum_mime.ContainsKey(videoFormat)) return enum_mime[videoFormat];
            throw new ArgumentOutOfRangeException(nameof(videoFormat), videoFormat, null);
        }

        public static VideoFormat? GetFormat(string? mime)
        {
            if (mime == null) return null;
            if (mime_enum.ContainsKey(mime)) return mime_enum[mime];
            return null;
        }
    }
}