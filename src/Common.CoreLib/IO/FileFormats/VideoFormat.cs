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
        static readonly Lazy<IReadOnlyDictionary<VideoFormat, string>> mEnumMime = new(() =>
        {
            var value = new Dictionary<VideoFormat, string>()
            {
                { VideoFormat.WebM, MediaTypeNames.WEBM },
                { VideoFormat.MP4, MediaTypeNames.MP4 },
            };
            return value;
        });

        static readonly Lazy<IReadOnlyDictionary<string, VideoFormat>> mMimeEnum = new(() =>
        {
            var value = EnumMime.ToDictionary(x => x.Value, x => x.Key, StringComparer.OrdinalIgnoreCase);
            return value;
        });

        static IReadOnlyDictionary<VideoFormat, string> EnumMime => mEnumMime.Value;

        static IReadOnlyDictionary<string, VideoFormat> MimeEnum => mMimeEnum.Value;

        /// <summary>
        /// 获取MIME
        /// </summary>
        /// <param name="videoFormat"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static string GetMIME(this VideoFormat videoFormat)
        {
            if (EnumMime.ContainsKey(videoFormat)) return EnumMime[videoFormat];
            throw new ArgumentOutOfRangeException(nameof(videoFormat), videoFormat, null);
        }

        public static VideoFormat? GetFormat(string? mime)
        {
            if (mime == null) return null;
            if (MimeEnum.ContainsKey(mime)) return MimeEnum[mime];
            return null;
        }
    }
}