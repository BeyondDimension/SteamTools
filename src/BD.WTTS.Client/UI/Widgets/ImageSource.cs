// ReSharper disable once CheckNamespace
namespace BD.WTTS.UI;

public static class ImageSource
{
    public sealed class ClipStream : IDisposable, IFileStreamWrapper
    {
        bool disposedValue;
        Stream? stream;

        ClipStream(Stream stream)
        {
            OriginStream = this.stream = stream;
        }

        public ClipStream Clone()
        {
            ClipStream clipStream = new(OriginStream)
            {
                Top = Top,
                Bottom = Bottom,
                Left = Left,
                Right = Right,
                Circle = Circle,
                Radius_X = Radius_X,
                Radius_Y = Radius_Y,
            };
            return clipStream;
        }

        public Stream? Stream
        {
            get
            {
                if (stream is IFileStreamWrapper wrapper)
                {
                    stream = wrapper.FileStream;
                }
                return stream;
            }
        }

        public Stream OriginStream { get; }

        /// <inheritdoc cref="FileStream.Name"/>
        public string Name
        {
            get
            {
                if (OriginStream is IFileStreamWrapper wrapper) return wrapper.Name;
                else if (OriginStream is FileStream file) return file.Name;
                return string.Empty;
            }
        }

        public float Top { get; set; }

        public float Left { get; set; }

        public float Right { get; set; }

        public float Bottom { get; set; }

        public float TopBottom
        {
            set
            {
                Top = value;
                Bottom = value;
            }
        }

        public float LeftRight
        {
            set
            {
                Left = value;
                Right = value;
            }
        }

        public float Radius_X { get; set; }

        public float Radius_Y { get; set; }

        public float Radius
        {
            set
            {
                Radius_X = value;
                Radius_Y = value;
            }
        }

        public bool Circle { get; set; }

        void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)
                    stream?.Dispose();
                    OriginStream.Dispose();
                }

                // TODO: 释放未托管的资源(未托管的对象)并重写终结器
                // TODO: 将大型字段设置为 null
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        [return: NotNullIfNotNull("stream")]
        public static implicit operator ClipStream?(Stream? stream) => stream == null ? null : new(stream);

        public static implicit operator ClipStream?(string? filePath)
        {
            FileStreamWrapper? wrapper = filePath;
            ClipStream? clipStream = wrapper;
            return clipStream;
        }
    }

    /// <summary>
    /// 将图片文件本地路径或资源路径转为图像源
    /// </summary>
    /// <param name="filePathOrResUri"></param>
    /// <param name="isCircle">是否为圆形</param>
    /// <param name="config">图像可配置选项</param>
    /// <returns></returns>
    public static object? TryParse(string? filePathOrResUri, bool isCircle = false, Action<ClipStream>? config = null)
    {
        if (filePathOrResUri == null)
            return null;
        if (IApplication.Instance.IsAvaloniaUI())
        {
            if (filePathOrResUri.StartsWith("avares:"))
                return filePathOrResUri;
        }
        ClipStream? clipStream = filePathOrResUri;
        if (clipStream != null)
        {
            clipStream.Circle = isCircle;
            config?.Invoke(clipStream);
        }
        return clipStream;
    }

    /// <summary>
    /// 将图片文件本地路径或资源路径转为图像源
    /// </summary>
    /// <param name="filePathOrResUri"></param>
    /// <param name="isCircle">是否为圆形</param>
    /// <param name="config">图像可配置选项</param>
    /// <returns></returns>
    public static ClipStream? TryParse(Stream? imageStream, bool isCircle = false, Action<ClipStream>? config = null)
    {
        if (imageStream == null)
            return null;

        ClipStream? clipStream = imageStream;
        if (clipStream != null)
        {
            clipStream.Circle = isCircle;
            config?.Invoke(clipStream);
        }
        return clipStream;
    }

    public static async Task<ClipStream?> GetAsync(
        string requestUri,
        bool isPolly = true,
        bool cache = false,
        HttpHandlerCategory category = HttpHandlerCategory.UserInitiated,
        bool isCircle = false,
        Action<ClipStream>? config = null,
        CancellationToken cancellationToken = default)
    {
        var imageHttpClientService = Ioc.Get_Nullable<IImageHttpClientService>();
        if (imageHttpClientService == default)
            return default;

        var imageMemoryStream = await imageHttpClientService.GetImageMemoryStreamAsync(requestUri, isPolly, cache, category, cancellationToken);
        if (imageMemoryStream == default)
            return default;

        ClipStream? clipStream = imageMemoryStream;
        if (clipStream != null)
        {
            clipStream.Circle = isCircle;
            config?.Invoke(clipStream);
        }
        return clipStream;
    }

    public static bool IsImage(string extension)
    {
        return string.Equals(extension, FileEx.JPG, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(extension, FileEx.JPEG, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(extension, FileEx.PNG, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(extension, FileEx.WEBP, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(extension, FileEx.HEIC, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(extension, FileEx.HEIF, StringComparison.OrdinalIgnoreCase);
    }
}