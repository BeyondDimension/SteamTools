using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace System.Application
{
    public static class ImageSouce
    {
        public sealed class ClipStream : IDisposable
        {
            bool disposedValue;

            ClipStream(Stream stream)
            {
                Stream = stream;
            }

            public Stream Stream { get; init; }

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
                        Stream.Dispose();
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
                if (filePath == null) return null;
                try
                {
                    ClipStream? clipStream = IOPath.OpenRead(filePath);
                    return clipStream;
                }
                catch
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// 将图片文件本地路径或 Avalonia 资源路径转为图像源
        /// </summary>
        /// <param name="filePathOrAvaloniaResPath"></param>
        /// <param name="isCircle">是否为圆形</param>
        /// <param name="config">是否为圆形</param>
        /// <returns></returns>
        public static object? TryParse(string? filePathOrAvaloniaResPath, bool isCircle = false, Action<ClipStream>? config = null)
        {
            if (filePathOrAvaloniaResPath == null)
                return null;
            if (filePathOrAvaloniaResPath.StartsWith("avares:"))
                return filePathOrAvaloniaResPath;
            ClipStream? clipStream = filePathOrAvaloniaResPath;
            if (clipStream != null)
            {
                clipStream.Circle = isCircle;
                config?.Invoke(clipStream);
            }
            return clipStream;
        }
    }
}