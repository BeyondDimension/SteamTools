using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace System.Application.Models
{
    public class ImageClipStream : IDisposable
    {
        public ImageClipStream(Stream stream)
        {
            Stream = stream;
        }

        public Stream Stream { get; set; }

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

        public void Dispose()
        {
            ((IDisposable)Stream).Dispose();
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
        public static CircleImageStream? Convert(Stream? stream)
            => stream == null ? null : new(stream);

        [return: NotNullIfNotNull("stream")]
        public static implicit operator CircleImageStream?(Stream? stream) => Convert(stream);

        [return: NotNullIfNotNull("filePath")]
        public static CircleImageStream? Convert(string? filePath)
            => filePath == null ? null : new(filePath);

        [return: NotNullIfNotNull("filePath")]
        public static implicit operator CircleImageStream?(string? filePath) => Convert(filePath);
    }
}