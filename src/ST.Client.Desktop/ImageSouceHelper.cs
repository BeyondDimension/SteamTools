using System.Diagnostics.CodeAnalysis;
using System.IO;
using static System.Application.ImageSouce;

namespace System.Application
{
    public static class ImageSouceHelper
    {
        /// <summary>
        /// 将图片文件本地路径或 Avalonia 资源路径转为图像源
        /// </summary>
        /// <param name="filePathOrAvaloniaResPath"></param>
        /// <param name="isCircle">是否为圆形</param>
        /// <param name="config">图像可配置选项</param>
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