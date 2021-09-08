using System.IO;
using System.Text;

// ReSharper disable once CheckNamespace
namespace System
{
    public static class FileInfoExtensions
    {
        public static StreamReader OpenText(this FileInfo fileInfo, Encoding encoding)
            => new(fileInfo.OpenRead(), encoding);
    }
}