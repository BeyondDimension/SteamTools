using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace System.Application.Models
{
    public class ImageClipStream : IDisposable
    {
        bool disposedValue;

        public ImageClipStream(Stream? stream)
        {
            Stream = stream;
        }

        public Stream? Stream { get; set; }

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

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)
                    Stream?.Dispose();
                }

                // TODO: 释放未托管的资源(未托管的对象)并重写终结器
                // TODO: 将大型字段设置为 null
                disposedValue = true;
            }
        }

        // // TODO: 仅当“Dispose(bool disposing)”拥有用于释放未托管资源的代码时才替代终结器
        // ~ImageClipStream()
        // {
        //     // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

    public class CircleImageStream : ImageClipStream
    {
        private CircleImageStream(Stream stream) : base(stream)
        {
            Circle = true;
        }

        private CircleImageStream(string filePath) : base(IOPath.OpenRead(filePath))
        {
            Circle = true;
        }

        [return: NotNullIfNotNull("stream")]
        static CircleImageStream? Convert(Stream? stream)
            => stream == null ? null : new(stream);

        [return: NotNullIfNotNull("stream")]
        public static implicit operator CircleImageStream?(Stream? stream) => Convert(stream);

        [return: NotNullIfNotNull("filePath")]
        static CircleImageStream? Convert(string? filePath)
            => filePath == null ? null : new(filePath);

        /// <summary>
        /// 将图片文件路径或 Avalonia 资源路径转为圆形图像源
        /// </summary>
        /// <param name="filePathOrResPath"></param>
        /// <returns></returns>
        public static object? TryConvert(string? filePathOrAvaloniaResPath)
        {
            if (filePathOrAvaloniaResPath == null)
                return null;
            if (filePathOrAvaloniaResPath.StartsWith("avares:"))
                return filePathOrAvaloniaResPath;
            try
            {
                return Convert(filePathOrAvaloniaResPath);
            }
            catch
            {
                return null;
            }
        }

        [return: NotNullIfNotNull("filePath")]
        public static implicit operator CircleImageStream?(string? filePath) => Convert(filePath);
    }
}