namespace System.IO;

public sealed class FileStreamWrapper : Stream, IFileStreamWrapper
{
    FileStreamWrapper(string path)
    {
        Name = path;
    }

    public string Name { get; set; }

    public override bool CanRead => throw new NotImplementedException();

    public override bool CanSeek => throw new NotImplementedException();

    public override bool CanWrite => throw new NotImplementedException();

    public override long Length => throw new NotImplementedException();

    public override long Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public override void Flush()
    {
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        throw new NotImplementedException();
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        throw new NotImplementedException();
    }

    public override void SetLength(long value)
    {
        throw new NotImplementedException();
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        throw new NotImplementedException();
    }

    public static FileStreamWrapper? Parse(string? filePath) => filePath == null ? null : new(filePath);

    public static implicit operator FileStreamWrapper?(string? filePath) => Parse(filePath);
}
