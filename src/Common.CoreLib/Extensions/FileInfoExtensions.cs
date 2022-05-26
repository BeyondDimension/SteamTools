using System.Text;

// ReSharper disable once CheckNamespace
namespace System;

public static class FileInfoExtensions
{
    public static StreamReader? OpenText(this FileInfo fileInfo, Encoding? encoding = null)
    {
        var f = IOPath.OpenRead(fileInfo.FullName);
        if (f == null) return null;
        return new StreamReader(f, encoding ?? EncodingCache.UTF8NoBOM);
    }
}